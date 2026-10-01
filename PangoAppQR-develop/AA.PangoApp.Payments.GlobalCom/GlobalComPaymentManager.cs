using AA.PangoApp.Payments.Interface;
using AA.PangoApp.Payments.Interface.Dtos;
using AA.PangoApp.Payments.Interface.Notifications;
using CCI.Globalcom.GlobalcomRetailProtocol;
using log4net;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Threading;

namespace AA.PangoApp.Payments.GlobalCom
{
    public class GlobalComPaymentManager : PaymentManagerAbstract
    {

        private ILog log = LogManager.GetLogger("GlobalComPaymentManager");
        public override GeneratePaymentResponse ProcessPayment(GeneratePaymentRequest request)
        {

            CurrentReaderStatus = CurrentReaderStatus.Idle;
            String TransactionRef = string.Empty;
            // First parameter is the comport to use to talk
            // to the device.  Help will display available options.
            string comport = "COM" + request.ComPort;

            // Default settings.
            ELanguage lang = ELanguage.English;
            ECurrencies currency = ECurrencies.USD;
            ETranType tranType = ETranType.Sale;
            EModeOfCom comms = EModeOfCom.Ethernet;
            int amountInPennies = Convert.ToInt32(request.Amount * 100);
            int baudRate = request.Bauds;

            log.Debug("Mode: " + comms.ToString() + " on port " + comport);
            log.Debug("Transaction Type: " + tranType.ToString() + " for " + ((decimal)amountInPennies / 100).ToString("N2"));
            log.Debug("Language: " + lang.ToString());
            log.Debug("Currency: " + currency.ToString());

            var startTime = DateTime.Now;
            RetailProtocol RpUI = null;
            // Process the request.
            try
            {
                int timeoutInSeconds = int.Parse(ConfigurationManager.AppSettings["PaymentProcessor-GlobalCom-PurchaseTimeout"] ?? "30");

                if (request.TimeoutInSeconds > 0)
                {
                    timeoutInSeconds = request.TimeoutInSeconds;
                }

                log.Debug("Transaction timeout: " + timeoutInSeconds + "s.");

                Transaction tran = new Transaction();
                tran.Lang = lang;
                tran.Currency = currency;
                tran.AmountInCents = amountInPennies;

                // Unique client ID is optional but for trouble shooting it can have
                // some value to match up to the host.  For uniqueness as batches close
                // each day Im setting this for testing to hour/minute/second.
                tran.ClientTransactionId = DateTime.Now.ToString("yyMMddHHmmss");

                // RetailProtocol will use the connection information to connect once a command is called.
                RpUI = new RetailProtocol(comport, baudRate, 0, (comms == EModeOfCom.MUX), lang);

                // Optional.  If running in mux mode the retail protocol hadles communication
                // but provides feedback through callbacks.
                MuxHandles handle = new MuxHandles();
                if (comms == EModeOfCom.MUX)
                {
                    RpUI.Mux.MuxDebugEvent += MuxHandles.RpUI_MuxDebug;
                    RpUI.Mux.MuxLogDebugEvent += MuxHandles.RpUI_MuxLogDebug;
                    RpUI.Mux.SysLogDebugEvent += MuxHandles.RpUI_SysLog;
                    // You can override the host url and port.  If not it pulls from the terminals configuration.
                    RpUI.Mux.SetTlsOverride("Globalcom", "", -1, false);
                    RpUI.Mux.MUXRefreshUIEvent += MuxHandles.RpUI_RefreshUI;
                    RpUI.Mux.MuxBankingDataLogEvent += MuxHandles.RpUI_MuxBankingDataLog;

                }

                // Wrapper to show the Retail Protocol in use.
                DeviceCommand device = new DeviceCommand(RpUI);

                // Clear any state that may be
                device.EraseInfo(lang, true);

                CurrentReaderStatus = CurrentReaderStatus.WaitingForCard;

                try
                {
                    device.ReadCard(timeoutInSeconds, tran);
                }
                catch (Exception readCardException)
                {
                    log.Error("Error occured when reading the card - " + readCardException.Message, readCardException);

                    // Clear any state that may be
                    if (RpUI != null)
                    {
                        // Clear any state that may be
                        //device.EraseInfo(lang, true);
                        RpUI.C_SerialClose();
                        RpUI.Comport.Dispose();
                        log.Debug("Serial port disposed!");
                    }

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

                device.AnalyzeCard(false);

                CurrentReaderStatus = CurrentReaderStatus.CardInserted;

                var (saleResponse, saleResponseMsg, uiResponseMsg) = device.ProcessSale(tran);

                if (saleResponse != ProcessPaymentStatus.Error)
                {
                    tran.ResponseReceiptXml = device.GetReceipt();
                }

                log.Debug("Transaction Approved: " + (saleResponse.ToString()));

                if (tran.ResponsePairs?.Count > 0)
                {
                    log.Debug("Data Pairs:");
                    log.Debug(Newtonsoft.Json.JsonConvert.SerializeObject(tran.ResponsePairs));
                    if (tran.ResponsePairs.ContainsKey("Ref"))
                    {
                        log.Debug("Data Transaction Reference:" + tran.ResponsePairs["Ref"]);
                        TransactionRef = tran.ResponsePairs["Ref"];
                    }
                }

                try
                {

                    // Clear any state that may be
                    if (RpUI != null)
                    {
                        RpUI.C_SerialClose();
                        RpUI.Comport.Dispose();

                        log.Debug("Serial port disposed!");
                    }
                    log.Debug("Clear state after tx success completed.");
                }
                catch (Exception ex1)
                {
                    log.Error(ex1);
                }

                return new GeneratePaymentResponse
                {
                    TransactionApproved = saleResponse == ProcessPaymentStatus.Approved,
                    TransactionStatus = (int)saleResponse,
                    TransactionNumber = tran.ClientTransactionId,
                    TransactionId= TransactionRef,
                    Data = tran.ResponsePairs,
                    XmlData = tran.ResponseReceiptXml,
                    HasError = saleResponse == ProcessPaymentStatus.Error,
                    ErrorMessage = saleResponseMsg,
                    UIErrorMessage = uiResponseMsg
                };

            }
            // All unexpected conditions are thrown.
            catch (Exception ex)
            {
                log.Error("Error occured when processing the EMV payment - " + ex.Message, ex);
                try
                {
                    log.Info("RESET DEVICE GLOBALCOM 0x27 ");
                    ResetDevice(baudRate, comport);
                    Thread.Sleep(1500);
                    if (RpUI != null)
                    {
                        RpUI.C_SerialClose();
                        RpUI.Comport.Dispose();

                        log.Debug("Serial port disposed!");
                    }
                    log.Info("EMAIL NOTIFICATION DEVICE GLOBALCOM RESET OK AND SERIAL PORT DISPOSED");
                    //EMAIL NOTIFICATION (punto único: enriquecido + Graph/SMTP)
                    MailSender.Send(
                        "CRITICAL ERROR LG1 - GLOBALCOM NOT WORKING RESET GLOBALCOM 0x27",
                        "CRITICAL ERROR - Installation " + ConfigurationManager.AppSettings["InstallationId"] + ", TerminalId " + ConfigurationManager.AppSettings["TerminalId"] + " -  RESET GLOBALCOM 0x27,  Error occured when processing the EMV payment - " + ex.Message);

                }
                catch (Exception exc)
                {
                    log.Error("ERROR RESET DEVICE GLOBALCOM  " + exc);
                    //EMAIL NOTIFICATION (punto único: enriquecido + Graph/SMTP)
                    MailSender.Send(
                        "CRITICAL ERROR LG1 - GLOBALCOM NOT WORKING - POWER OFF y POWE BACK ON",
                        "CRITICAL ERROR LG1 - Installation " + ConfigurationManager.AppSettings["InstallationId"] + ", TerminalId " + ConfigurationManager.AppSettings["TerminalId"] + " - ,  POWER OFF y POWE BACK ON - Error occured when processing the EMV payment - " + ex.Message);

                    Process.Start(ConfigurationManager.AppSettings["RestarProcess"]);
                    log.Info("LANZANDO REINICIO " + ConfigurationManager.AppSettings["RestarProcess"]);
                }


                try
                {

                    // Clear any state that may be
                    if (RpUI != null)
                    {
                        //DeviceCommand device1 = new DeviceCommand(RpUI);
                        //device1.EraseInfo(lang, true);
                        RpUI.C_SerialClose();
                        RpUI.Comport.Dispose();
                        log.Debug("Serial port disposed!");
                        Thread.Sleep(1500);
                    }
                    log.Debug("Clear state after error completed.");
                }
                catch (Exception ex1)
                {
                    log.Error(ex1);
                    if (RpUI != null)
                    {
                        RpUI.C_SerialClose();
                        RpUI.Comport.Dispose();
                        log.Error("Serial port disposed!");

                    }

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

        public static void ResetDevice(int baudRate, string ComPort)
        {
            RetailProtocol RpUI = null;
            // Default settings.
            ELanguage lang = ELanguage.English;
            EModeOfCom comms = EModeOfCom.Ethernet;
            string comport = ComPort;

            // RetailProtocol will use the connection information to connect once a command is called.
            RpUI = new RetailProtocol(comport, baudRate, 0, (comms == EModeOfCom.MUX), lang);

            RpUI.RP_TerminalResetRequest();
        }
    }
}
