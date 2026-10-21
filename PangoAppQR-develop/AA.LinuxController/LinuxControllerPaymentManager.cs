using AA.PangoApp.Payments.Interface;
using AA.PangoApp.Payments.Interface.Dtos;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Xml.Linq;

namespace AA.LinuxController
{
    public class LinuxControllerPaymentManager : PaymentManagerAbstract
    {
        private ILog log = LogManager.GetLogger("LinuxControllerPaymentManager");

        private static int maxPurchaseTimeout = int.Parse(ConfigurationManager.AppSettings["PaymentProcessor-LinuxController-MaxPurchaseTimeout"] ?? "120");

        private const string CardInsertedMessage = "CARDINSERTED";
        private const string CancelRequest = "{\"3\":{\"cancelPayment\":1}}";

        private readonly LinuxControllerClient client = new LinuxControllerClient();
        private volatile bool cardInserted = false;

        public LinuxControllerPaymentManager()
        {
            client.MessageReceived += Client_MessageReceived;
        }

        public override GeneratePaymentResponse ProcessPayment(GeneratePaymentRequest request)
        {
            int amountInPennies = Convert.ToInt32(request.Amount * 100);
            string clientTransactionId = DateTime.Now.ToString("yyMMddHHmmss");

            log.Debug("Transaction Type: Sale for " + ((decimal)amountInPennies / 100).ToString("N2") + " - ClientTransactionId: " + clientTransactionId);

            try
            {
                cardInserted = false;
                CurrentReaderStatus = CurrentReaderStatus.WaitingForCard;

                var paymentRequest = new JObject(new JProperty("3", new JObject(
                    new JProperty("amountInPennies", amountInPennies),
                    new JProperty("clientTransactionID", clientTransactionId),
                    new JProperty("testEth", false))));

                LinuxControllerResponse response = client.Send(paymentRequest.ToString(Formatting.None), maxPurchaseTimeout * 1000);

                if (response == null)
                {
                    if (!cardInserted)
                        CancelPayment();

                    return ErrorResponse(clientTransactionId, "NO RESPONSE FROM LINUX CONTROLLER", "");
                }

                return MapResponse(response, clientTransactionId);
            }
            catch (Exception ex)
            {
                log.Error("Error occured when processing the payment - " + ex.Message, ex);
                return ErrorResponse(clientTransactionId, ex.Message, "");
            }
            finally
            {
                CurrentReaderStatus = CurrentReaderStatus.Idle;
            }
        }

        private GeneratePaymentResponse MapResponse(LinuxControllerResponse response, string clientTransactionId)
        {
            JObject deviceResponse = null;
            if (!string.IsNullOrEmpty(response.DeviceResponse))
            {
                try
                {
                    deviceResponse = JObject.Parse(response.DeviceResponse);
                }
                catch (JsonException ex)
                {
                    log.Error("INVALID PAYMENT DEVICE RESPONSE - " + ex.Message);
                }
            }

            log.Debug("Payment response: " + response.Message + " - LastTransactionStatus: " + deviceResponse?.Value<string>("LastTransactionStatus"));

            if (deviceResponse == null)
                return ErrorResponse(clientTransactionId, response.Message, "");

            if (deviceResponse.Value<bool?>("Timeout") == true)
                return ErrorResponse(clientTransactionId, response.Message, "Timeout reading card");

            string lastOperation = deviceResponse.Value<string>("LastOperation");
            XElement receipt = ParseReceipt(lastOperation);
            string xmlData = receipt != null ? lastOperation : null;

            string errorCode = receipt?.Element("TransactionData")?.Element("Error")?.Value;
            var data = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(errorCode))
                data["Err code"] = errorCode;

            if (response.OperationResult && deviceResponse.Value<bool?>("Success") == true)
            {
                string transactionRef = receipt?.Element("Gateway")?.Descendants("CardEaseReference")
                    .Select(x => x.Value).FirstOrDefault(x => !string.IsNullOrEmpty(x)) ?? "";
                log.Debug("Data Transaction Reference:" + transactionRef);

                return new GeneratePaymentResponse
                {
                    TransactionApproved = true,
                    TransactionStatus = (int)ProcessPaymentStatus.Approved,
                    TransactionNumber = clientTransactionId,
                    TransactionId = transactionRef,
                    Data = data,
                    XmlData = xmlData,
                    HasError = false,
                    ErrorMessage = "",
                    UIErrorMessage = ""
                };
            }

            if (deviceResponse.Value<bool?>("ExceptionEncountered") == true)
            {
                string rpLog = deviceResponse.Value<string>("Log");
                return ErrorResponse(clientTransactionId, string.IsNullOrEmpty(rpLog) ? response.Message : rpLog, "");
            }

            if (receipt == null)
                return ErrorResponse(clientTransactionId, response.Message + " " + deviceResponse.Value<string>("LastTransactionStatus"), "");

            string errorMessage = "Error Code: " + errorCode;
            string acquirerResponseCode = receipt.Element("Gateway")?.Descendants("AcquirerResponseCode").Select(x => x.Value).FirstOrDefault();
            if (!string.IsNullOrEmpty(acquirerResponseCode))
                errorMessage += " Acquirer Response Code: " + acquirerResponseCode;

            bool denied = deviceResponse.Value<string>("LastTransactionStatus") == "Status outcome bad";

            return new GeneratePaymentResponse
            {
                TransactionApproved = false,
                TransactionStatus = (int)(denied ? ProcessPaymentStatus.Denied : ProcessPaymentStatus.Error),
                TransactionNumber = clientTransactionId,
                Data = data,
                XmlData = xmlData,
                HasError = !denied,
                ErrorMessage = errorMessage,
                UIErrorMessage = ""
            };
        }

        private XElement ParseReceipt(string lastOperation)
        {
            if (string.IsNullOrEmpty(lastOperation) || !lastOperation.TrimStart().StartsWith("<"))
                return null;

            try
            {
                return XDocument.Parse(lastOperation).Root;
            }
            catch (Exception ex)
            {
                log.Error("INVALID RECEIPT XML - " + ex.Message);
                return null;
            }
        }

        private GeneratePaymentResponse ErrorResponse(string clientTransactionId, string errorMessage, string uiErrorMessage)
        {
            return new GeneratePaymentResponse
            {
                TransactionApproved = false,
                TransactionStatus = (int)ProcessPaymentStatus.Error,
                TransactionNumber = clientTransactionId,
                Data = null,
                HasError = true,
                ErrorMessage = errorMessage,
                UIErrorMessage = uiErrorMessage
            };
        }

        private void CancelPayment()
        {
            using (var cancelClient = new LinuxControllerClient())
            {
                LinuxControllerResponse response = cancelClient.Send(CancelRequest, 5000);
                log.Info("Payment cancel sent to Linux Controller: " + (response != null ? response.Message : "NO RESPONSE"));
            }
        }

        private void Client_MessageReceived(object sender, string message)
        {
            if (message.Trim() == CardInsertedMessage)
            {
                cardInserted = true;
                CurrentReaderStatus = CurrentReaderStatus.CardInserted;
            }
        }
    }
}