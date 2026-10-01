using BAC_ECR;
using BacPayments.Processors.Dtos;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Parso.Classes.DTO;
using Parso.Controllers.RPPaymentController;
using Parso.Utils;
using Parso.Utils.Objects;
using Serilog;

namespace Parso.Classes.Helpers
{
    internal class PaymentHelper
    {
        private ProjectConstants _projectConstants = ProjectConstants.Instance;

        private GeneratePaymentResponse _bacPaymentResponse = new GeneratePaymentResponse();
        private GeneratePaymentRequest _bacPaymentRequest = new GeneratePaymentRequest();
        private CredomaticEcrProcessorService _credomaticEcrProcessorService = new CredomaticEcrProcessorService();

        // Use a shared instance for payment operations to allow cancellation
        private static RPPaymentController _sharedPaymentController = new RPPaymentController();

        private JObject _json;
        private JProperty _operationProperty;
        private JObject _paymentData;

        private DTOResponse _response;

        public string CardPaymentCommand(string command)
        {
            try
            {
                // Parse JSON command
                _json = JObject.Parse(command);
                _operationProperty = _json.Properties().First();

                // Check if this is a cancellation command
                if (_operationProperty.Name == "3")
                {
                    var operationData = (JObject)_operationProperty.Value;

                    // Check for cancellation command specifically
                    if (operationData["cancelPayment"] != null && operationData["cancelPayment"].Value<int>() == 1)
                    {
                        _sharedPaymentController.CancelPayment();
                        return JsonConvert.SerializeObject(new Response(true, 200, true, "SUCCESS: CANCELLATION REQUEST SENT"), Formatting.None);
                    }
                    // If it's not a cancellation, proceed with normal payment flow
                }

                switch (_projectConstants.CARD_PAYMENT_CARD_READER_TYPE)
                {
                    case 0:
                        _response = BcrPayment();
                        break;
                    case 1:
                        _response = RPPayment();
                        break;
                }

                if (!_response.respond) { return ""; }

                if (_response.timeout) { return JsonConvert.SerializeObject(new Response(true, 500, false, $"ERROR: TIMEOUT", JsonConvert.SerializeObject(_response.data, Formatting.None)), Formatting.None); }

                return _response.status
                            ? JsonConvert.SerializeObject(new Response(true, 200, true, $"SUCCESS: TRANSACTION SUCCESSFUL AND COMPLETE", JsonConvert.SerializeObject(_response.data, Formatting.None)), Formatting.None)
                            : JsonConvert.SerializeObject(new Response(true, 200, false, $"SUCCESS: TRANSACTION SUCCESSFUL BUT INCOMPLETE", JsonConvert.SerializeObject(_response.data, Formatting.None)), Formatting.None);
            }
            catch (JsonException ex)
            {
                Log.Error(ex.Message);
                return JsonConvert.SerializeObject(new Response(false, 400, false, $"ERROR: INVALID JSON FORMAT - {ex.Message}"), Formatting.None);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return JsonConvert.SerializeObject(new Response(false, 500, false, $"FATAL ERROR: EXCEPTION ENCOUNTERED - {ex.Message}"), Formatting.None);
            }
        }

        private DTOResponse BcrPayment()
        {
            // Get amount from JSON value
            var amountString = _operationProperty.Value.ToString();
            int.TryParse(amountString, out int amount);

            _bacPaymentRequest.Amount = amount;
            _bacPaymentRequest.ComPort = _projectConstants.CARD_PAYMENT_COM_PORT;
            _bacPaymentRequest.Bauds = _projectConstants.CARD_PAYMENT_BAUD_RATE;

            _bacPaymentResponse = _credomaticEcrProcessorService.ProcessPayment(_bacPaymentRequest);

            DTOResponse response = new DTOResponse(true, _bacPaymentResponse);
            return response;
        }

        private DTOResponse RPPayment()
        {
            _paymentData = (JObject)_operationProperty.Value;

            // Check if this is actually a cancellation command that somehow got here
            if (_paymentData["cancelPayment"] != null && _paymentData["cancelPayment"].Value<int>() == 1) // && _paymentData["cancelPayment"].Value<int>() == 1
            {
                // This should not happen, but as a safety check
                _sharedPaymentController.CancelPayment();
                return new DTOResponse(false);
            }

            var paymentRequest = new PaymentRequest()
            {
                AmountInPennies = _paymentData["amountInPennies"]?.Value<int>() ?? 0,
                CurrencyCode = _projectConstants.CARD_PAYMENT_CURRENCY_CODE,
                EMode = _projectConstants.CARD_PAYMENT_EMODE, // Hex values stored as int
                ClientTransactionID = _paymentData["clientTransactionID"]?.Value<string>(),
                Language = _projectConstants.CARD_PAYMENT_LANGUAGE,
            };

            // Extract method parameters
            int timeoutSeconds = _projectConstants.CARD_PAYMENT_TIMEOUT_SECOND;
            int scanTimerMili = _projectConstants.CARD_PAYMENT_SCAN_TIMER_MS;
            bool testEth = _paymentData["testEth"]?.Value<bool>() ?? false;
            bool includeRPLog = _projectConstants.CARD_PAYMENT_INCLUDE_RP_LOG;
            bool sendRPCardInsertedNotification = _projectConstants.CARD_PAYMENT_SEND_RP_CARD_INSERTED_NOTIFICATION;
            string RPCardInsertedMessage = _projectConstants.CARD_PAYMENT_RP_CARD_INSERTED_MESSAGE;

            // Call transaction method on shared controller
            RPResponse RPresponse = _sharedPaymentController.RetailProtocolTransaction(
                paymentRequest,
                timeoutSeconds,
                scanTimerMili,
                testEth,
                includeRPLog,
                sendRPCardInsertedNotification,
                RPCardInsertedMessage
            );
            //Estos tres se pueden cambiar a un solo llamado sin el IF si arreglamos la lógica del DTO
            if (!RPresponse.Respond) { return new DTOResponse(RPresponse.Success, RPresponse, false); }
            if (RPresponse.Timeout) { return new DTOResponse(RPresponse.Success, RPresponse, true, RPresponse.Timeout);  }
            DTOResponse response = new DTOResponse(RPresponse.Success, RPresponse);
            return response;
        }
    }
}