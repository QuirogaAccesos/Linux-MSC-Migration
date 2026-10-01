using AA.PangoApp.Payments.AMP;
using AA.PangoApp.Payments.Interface;
using AA.PangoApp.Payments.Interface.Dtos;
using AMPComm;
using log4net;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Threading;

namespace AA.PangoApp.Payments.AMP
{
    public class AMPPaymentManager : PaymentManagerAbstract
    {

        private ILog log = LogManager.GetLogger("AMPPaymentManager");
        private volatile DeviceCommands device = new DeviceCommands();

        public override GeneratePaymentResponse ProcessPayment(GeneratePaymentRequest request)
        {
            string comport = "COM" + request.ComPort;

            log.Debug("On port " + comport);
            log.Debug("Transaction Type: Purchase for " + (request.Amount).ToString("N2"));
            log.Debug("Currency: USD");
            CurrentReaderStatus = CurrentReaderStatus.WaitingForCard;
            try
            {
                int timeoutInSeconds = int.Parse(ConfigurationManager.AppSettings["PaymentProcessor-AMP-PurchaseTimeout"] ?? "30");

                if (request.TimeoutInSeconds > 0)
                {
                    timeoutInSeconds = request.TimeoutInSeconds;
                }

                //log.Debug("Transaction timeout: " + timeoutInSeconds + "s.");
                 
                try
                {
                    AMPResponsePayment AMPresponse = device.ProcessSale(request);
                    GeneratePaymentResponse Response = null;
                    if (AMPresponse != null && !string.IsNullOrEmpty(AMPresponse.ecrConnectResponseCode) && AMPresponse.ecrConnectResponseCode.Equals("-1"))
                    {
                        log.Error("Error occured when connect device AMPresponse.ecrConnectResponseCode=" + AMPresponse.ecrConnectResponseCode);
                        return new GeneratePaymentResponse
                        {
                            TransactionApproved = false,
                            TransactionNumber = "",
                            Data = null,
                            HasError = true,
                            TransactionStatus = (int)ProcessPaymentStatus.Error,
                            ErrorMessage = "AMP-1111#Internal Block AMP Error",
                            UIErrorMessage = AMPresponse.ErrorMessage,
                        };
                    }
                    else if (AMPresponse != null && !string.IsNullOrEmpty(AMPresponse.ecrConnectResponseCode) && AMPresponse.ecrConnectResponseCode.Equals("-2"))
                    {
                        log.Error("Error occured when connect device AMPresponse.ecrConnectResponseCode=" + AMPresponse.ecrConnectResponseCode);
                        return new GeneratePaymentResponse
                        {
                            TransactionApproved = false,
                            TransactionNumber = "",
                            Data = null,
                            HasError = true,
                            TransactionStatus = (int)ProcessPaymentStatus.Error,
                            ErrorMessage = "AMP-2222#Timeout AMP Error",
                            UIErrorMessage = AMPresponse.ErrorMessage,
                        };
                    }
                    else if (AMPresponse != null && !string.IsNullOrEmpty(AMPresponse.ecrConnectResponseCode) && AMPresponse.ecrConnectResponseCode.Equals("-3"))
                    {
                        log.Error("Error occured when connect device AMPresponse.ecrConnectResponseCode=" + AMPresponse.ecrConnectResponseCode);
                        return new GeneratePaymentResponse
                        {
                            TransactionApproved = false,
                            TransactionNumber = "",
                            Data = null,
                            HasError = true,
                            TransactionStatus = (int)ProcessPaymentStatus.Error,
                            ErrorMessage = "AMP-3333#Closed APP AMP Error",
                            UIErrorMessage = AMPresponse.ErrorMessage,
                        };
                    }
                    else if (AMPresponse != null && !string.IsNullOrEmpty(AMPresponse.ecrConnectResponseCode) && AMPresponse.ecrConnectResponseCode.Equals("-4"))
                    {
                        log.Error("Error occured when connect device AMPresponse.ecrConnectResponseCode=" + AMPresponse.ecrConnectResponseCode);
                        return new GeneratePaymentResponse
                        {
                            TransactionApproved = false,
                            TransactionNumber = "",
                            Data = null,
                            HasError = true,
                            TransactionStatus = (int)ProcessPaymentStatus.Error,
                            ErrorMessage = "AMP-4444#Payment Error or PIN Error",
                            UIErrorMessage = AMPresponse.ErrorMessage,
                        };
                    }
                    else if (AMPresponse != null && (AMPresponse.Payload == null || string.IsNullOrEmpty(AMPresponse.Payload.response_resultcode_key)))
                    {
                        log.Debug("RESULT DATA PAYLOAD = NULL/EMPTY -- The transaction was manually cancelled by the user. TransactionStatus=" + AMPresponse.TransactionStatus);
                        Response = new GeneratePaymentResponse
                        {
                            TransactionApproved = false,
                            TransactionNumber = AMPresponse.UserDefinedEchoData,
                            Data = null,
                            XmlData = null,
                            HasError = true,
                            TransactionStatus = (int)ProcessPaymentStatus.Error,
                            ErrorMessage = "AMP-1001#AMP PAYLOAD EMPTY. Manually cancelled by the user",
                            UIErrorMessage = "The transaction was manually cancelled by the user",
                        };
                    }
                    else if (AMPresponse != null && AMPresponse.Payload.response_resultcode_key.Equals("0000")
                        && ("00".Equals(AMPresponse.Payload.response_code_key) || "10".Equals(AMPresponse.Payload.response_code_key)))
                    {
                        // Para considerar la transaccion APROBADA no basta con response_resultcode_key = "0000"
                        // (eso solo indica que la operacion se completo, no que el emisor la aprobara). Ademas el
                        // codigo de respuesta del emisor (response_code_key) debe ser "00" (aprobada) o "10"
                        // (aprobacion parcial). Cualquier otro valor (p.ej. "06") es una DECLINACION.
                        // Nota: TransactionStatus = 'RESULT_OK' no se valida porque se usa AMP Connect Agent.
                        log.Debug("RESULT DATA 0000 + response_code_key=" + AMPresponse.Payload.response_code_key + ": Approved");
                        Response = new GeneratePaymentResponse
                        {
                            TransactionApproved = true,
                            TransactionNumber = AMPresponse.UserDefinedEchoData,
                            TransactionId = AMPresponse.Payload.response_trace_key?.ToString() ?? "",
                            Data = null,
                            XmlData = AMPresponse.Payload.response_jsonrcpt_key ?? "",
                            HasError = false,
                            TransactionStatus = (int)ProcessPaymentStatus.Approved,
                            ErrorMessage = "AMP-0000#Successful",
                            UIErrorMessage = "Payment Approved Successful",
                        };
                    }
                    else if (AMPresponse != null && AMPresponse.Payload.response_resultcode_key.Equals("0000"))
                    {
                        // La operacion se completo (resultcode "0000") pero el emisor NO aprobo el pago:
                        // response_code_key distinto de "00"/"10" => transaccion DECLINADA (no es un error de
                        // comunicacion). Se marca como Denied para que el terminal muestre PaymentDenied.
                        log.Debug("RESULT DATA 0000 but response_code_key=" + AMPresponse.Payload.response_code_key + ": Declined");
                        Response = new GeneratePaymentResponse
                        {
                            TransactionApproved = false,
                            TransactionNumber = AMPresponse.UserDefinedEchoData,
                            TransactionId = AMPresponse.Payload.response_trace_key?.ToString() ?? "",
                            Data = null,
                            XmlData = AMPresponse.Payload.response_jsonrcpt_key ?? "",
                            HasError = true,
                            TransactionStatus = (int)ProcessPaymentStatus.Denied,
                            ErrorMessage = "AMP-1003#Transaction declined by issuer. response_code_key=" + AMPresponse.Payload.response_code_key,
                            UIErrorMessage = "Transaction has been declined",
                        };
                    }
                    else if (AMPresponse != null && AMPresponse.Payload.response_resultcode_key.Equals("-1001"))
                    {
                        log.Debug("RESULT DATA 1001: The transaction was manually cancelled by the user");
                        Response = new GeneratePaymentResponse
                        {
                            TransactionApproved = false,
                            TransactionNumber = AMPresponse.UserDefinedEchoData,
                            Data = null,
                            XmlData = AMPresponse.Payload.response_jsonrcpt_key,
                            HasError = true,
                            TransactionStatus = (int)ProcessPaymentStatus.Error,
                            ErrorMessage = "AMP-1001#Manually cancelled by the user",
                            UIErrorMessage = "The transaction was manually cancelled by the user",
                        };
                    }
                    else if (AMPresponse != null && AMPresponse.Payload.response_resultcode_key.Equals("-1002"))
                    {
                        log.Debug("RESULT DATA 1002: The transaction timed out");
                        Response = new GeneratePaymentResponse
                        {
                            TransactionApproved = false,
                            TransactionNumber = AMPresponse.UserDefinedEchoData,
                            Data = null,
                            XmlData = AMPresponse.Payload.response_jsonrcpt_key,
                            HasError = true,
                            TransactionStatus = (int)ProcessPaymentStatus.Error,
                            ErrorMessage = "AMP-1002#Card not inserted",
                            UIErrorMessage = "The transaction timed out",
                        };
                    }
                    else if (AMPresponse != null && AMPresponse.Payload.response_resultcode_key.Equals("-1003"))
                    {
                        log.Debug("RESULT DATA 1003: An invalid input (from the user - e.g. unsupported card) was provided");
                        Response = new GeneratePaymentResponse
                        {
                            TransactionApproved = false,
                            TransactionNumber = AMPresponse.UserDefinedEchoData,
                            Data = null,
                            XmlData = AMPresponse.Payload.response_jsonrcpt_key,
                            HasError = true,
                            TransactionStatus = (int)ProcessPaymentStatus.Error,
                            ErrorMessage = "AMP-1003#Card Declined, Magnetic Swipe or Card Brand not supported",
                            UIErrorMessage = "Transaction has been declined",
                        };
                    }
                    else if (AMPresponse != null && AMPresponse.Payload.response_resultcode_key.Equals("-1005"))
                    {
                        log.Debug("RESULT DATA 1005: The record was not found");
                        Response = new GeneratePaymentResponse
                        {
                            TransactionApproved = false,
                            TransactionNumber = AMPresponse.UserDefinedEchoData,
                            Data = null,
                            XmlData = AMPresponse.Payload.response_jsonrcpt_key,
                            HasError = true,
                            TransactionStatus = (int)ProcessPaymentStatus.Error,
                            ErrorMessage = "AMP-1005#An unexpected error has occurred",
                            UIErrorMessage = "An unexpected error has occurred. Please try again",
                        };
                    }
                    else if (AMPresponse != null && AMPresponse.Payload.response_resultcode_key.Equals("-1006"))
                    {
                        log.Debug("RESULT DATA 1006: The transaction specified was not allowed to occur. Either its not supported or its disabled");
                        Response = new GeneratePaymentResponse
                        {
                            TransactionApproved = false,
                            TransactionNumber = AMPresponse.UserDefinedEchoData,
                            Data = null,
                            XmlData = AMPresponse.Payload.response_jsonrcpt_key,
                            HasError = true,
                            TransactionStatus = (int)ProcessPaymentStatus.Error,
                            ErrorMessage = "AMP-1006#An unexpected error has occurred",
                            UIErrorMessage = "An unexpected error has occurred. Please try again",
                        };
                    }
                    else if (AMPresponse != null)
                    {
                        log.Error("RESULT DATA ???: Error desconocido");
                        Response = new GeneratePaymentResponse
                        {
                            TransactionApproved = false,
                            TransactionNumber = AMPresponse.UserDefinedEchoData,
                            Data = null,
                            XmlData = AMPresponse.Payload != null ? AMPresponse.Payload.response_jsonrcpt_key : null,
                            HasError = true,
                            TransactionStatus = (int)ProcessPaymentStatus.Undefined,
                            ErrorMessage = "AMP-1007#An unexpected error has occurred - "+ AMPresponse.ecrConnectResponseCode,
                            UIErrorMessage = "An unexpected error has occurred. Please try again",
                        };

                    }
                    return Response;
                }
                catch (Exception readCardException)
                {
                    log.Error("Error occured when reading the card - " + readCardException.Message, readCardException);
                    //DeviceCommands device = new DeviceCommands();
                    // Clear any state that may be
                    //device.Disconnect();
                    log.Debug("Clear state after error completed.");

                    var uiErrorMessage = "";

                    if (readCardException.Message?.ToLower() == "timeout reading card")
                    {
                        uiErrorMessage = readCardException.Message;
                    }

                    return new GeneratePaymentResponse
                    {
                        TransactionApproved = false,
                        TransactionNumber = "",
                        Data = null,
                        HasError = true,
                        TransactionStatus = (int)ProcessPaymentStatus.Error,
                        ErrorMessage = readCardException.Message,
                        UIErrorMessage = uiErrorMessage,
                    };
                }
            }
            // All unexpected conditions are thrown.
            catch (Exception ex)
            {
                log.Error("Error occured when processing the EMV payment - " + ex.Message, ex);
                try
                {
                    // Clear any state that may be
                    //DeviceCommands device = new DeviceCommands();
                    //device.Disconnect() ;
                    log.Debug("Clear state after error completed.");
                }
                catch (Exception ex1)
                {
                    log.Error(ex1);
                }

                return new GeneratePaymentResponse
                {
                    TransactionApproved = false,
                    TransactionStatus = (int)ProcessPaymentStatus.Error,
                    TransactionNumber = "",
                    HasError = true,
                    ErrorMessage = ex.Message,
                    Data = null,
                    UIErrorMessage = ""
                };
            }


        }

        protected override void OnPaymentResponseObtained(EventArgs e)
        {
            base.OnPaymentResponseObtained(e);
        }
    }
}
