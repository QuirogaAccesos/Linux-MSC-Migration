using AA.Pango.Model;
using AA.Pango.ServiceLayer.ApiDto;
using AA.Pango.ServiceLayer.ApiDtos;
using AA.Pango.ServiceLayer.Commands;
using AA.Pango.ServiceLayer.Helpers;
using AA.PangoApp.Payments.GlobalCom;
using AA.PangoApp.Payments.Interface;
using AA.PangoApp.Payments.Interface.Dtos;
using AA.ParkingStation.Service;
using log4net;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading;
using ZXing;
using ZXing.Common;
using ZXing.Presentation;
using ZXing.QrCode.Internal;
using ZXing.Rendering;
using static AA.Pango.ServiceLayer.ApiDto.CheckQRCode;
using static AA.Pango.ServiceLayer.ApiDto.WhiteListData;
using BarcodeWriter = ZXing.BarcodeWriter;

namespace AA.Pango.ServiceLayer.Controllers
{
    public enum TerminalState
    {
        StandBy,
        VehiclePresent,
        ReadingBarcode,
        TicketApproved,
        TicketDenied,
        TicketNotFound,
        ReadingPlate,
        PlateValidated,
        PlateNotValidated,
        PlateNotFound,
        PaymentInProgress,
        PaymentApproved,
        PaymentDenied,
        PaymentFailed,
        PaymentFinalized,
        OperationValidated,
        OutOfService,
        TicketEntry,
        NotFound,
        PermitExpired,
        PangoPaymentCVV,
        PangoPassError
    }

    public class BarcodeReadEventArgs
    {
        public BarcodeReadEventArgs(string text)
        {
            Text = text;
        }
        public string Text { get; } // readonly
    }

    // Declare the delegate (if using non-generic pattern).
    public delegate void BarcodeReadEvent(object sender, BarcodeReadEventArgs e);

    /// <summary>
    /// Controller that manages all the operation and behavior of a terminal.
    /// </summary>
    public class TerminalController
    {
        protected ILog _log = LogManager.GetLogger("TerminalController");

        private static TerminalController _instance = null;
        private static SerialPort _readerSerialPort = null;
        private SerialPort _picobSerialPort = null;
        private static Terminal _terminal = null;
        private static volatile PaymentManagerAbstract _paymentProcessorService = null;
        private static decimal? _taxAmount = null;
        private static decimal? _transactionFeeRate = null;

        public Dictionary<string, Template> TemplatesDictionary { get; set; }
        public decimal AmountToCharge { get; set; }

        #region General configuration variables

        private static string installationId = ConfigurationManager.AppSettings["InstallationId"];
        private static string serialNumber = ConfigurationManager.AppSettings["SerialNumber"];
        private static string terminalNumber = ConfigurationManager.AppSettings["TerminalId"];
        private static string readerSource = ConfigurationManager.AppSettings["ReaderSource"];
        private static string TicketQrLogo = ConfigurationManager.AppSettings["TicketQrLogo"];
        private static string TicketQrText = ConfigurationManager.AppSettings["TicketQrText"];
        private static string eventTypeToSend = ConfigurationManager.AppSettings["EventNotificationTypeToSend"] ?? "Entry";
        private static string ipsToUpdate = ConfigurationManager.AppSettings["TerminalsToUpdate"] ?? "";
        private static int maxPicovIncomingMessages = int.Parse(ConfigurationManager.AppSettings["MaxPicovIncomingMessages"] ?? "3");
        private static int printReceiptTimeout = int.Parse(ConfigurationManager.AppSettings["PrintReceiptTimeout"] ?? "8000");
        private static int sendApiEventTimeout = int.Parse(ConfigurationManager.AppSettings["SendApiEventTimeout"] ?? "30000");
        private static int pendingPaymentsInterval = int.Parse(ConfigurationManager.AppSettings["PendingPaymentsInterval"] ?? "3000");
        public static readonly bool PaymentServiceEnabled = bool.Parse(ConfigurationManager.AppSettings["PaymentServiceEnabled"] ?? "false");

        public static readonly bool UseFakePos = bool.Parse(ConfigurationManager.AppSettings["UseFakePos"] ?? "false");
        private static int PaymentRetryTimeout = int.Parse(System.Configuration.ConfigurationManager.AppSettings["PaymentRetryTimeout"] ?? "60000");

        #endregion

        #region Shared control variables

        public volatile string InputValue = null;
        public volatile bool ReadInputValue = false;
        public volatile bool ReadingInputValue = false;
        public volatile string CurrentClientDescription = null;
        public volatile string TextToShowOnUI = null;
        public volatile int TimeInSecsToShowOnUI = 0;
        public volatile bool TextRequireUserResponse = false;
        public volatile bool PrintReceipt = false;
        public volatile bool IsPaymentTransactionInProgress = false;
        public DateTime? LastPaymentApiEventDate = null;
        public volatile bool IsVehiclePresent = false;
        public volatile int WhiteListId = 0;
        public volatile float PaymAmount = 0;
        public volatile string QR = null;
        public volatile string PangoPassError = null;

        #endregion

        #region Events
        // Declare the event.
        public event BarcodeReadEvent CodeWasReadEvent;

        protected virtual void OnCodeWasRead(string barcode)
        {
            // Raise the event in a thread-safe manner using the ?. operator.
            CodeWasReadEvent?.Invoke(this, new BarcodeReadEventArgs(barcode));
        }
        #endregion

        public SerialPort Reader
        {
            get
            {
                return _readerSerialPort;
            }
        }

        public Terminal Terminal
        {
            get
            {
                return _terminal;
            }
        }

        public bool IsEntryTerminal
        {
            get
            {
                return eventTypeToSend == "Entry";
            }
        }

        public volatile TerminalState TerminalState = TerminalState.StandBy;

        public static TerminalController GetInstance(int terminalNumber, string readerComPort, int bauds = 9600)
        {
            if (_instance == null)
            {
                _instance = new TerminalController(terminalNumber, readerComPort, bauds);
            }

            return _instance;
        }

        private TerminalController(int terminalNumber, string readerComPort, int bauds = 9600)
        {
            InitReader(readerComPort, bauds);
            InitPicob(ConfigurationManager.AppSettings["PicovComPort"], int.Parse(ConfigurationManager.AppSettings["PicovBauds"] ?? "9600"));
            _terminal = TerminalService.GetTerminal(terminalNumber.ToString());
        }

        #region Payment Processor functionality

        private decimal? TaxAmountRate
        {
            get
            {
                if (_taxAmount == null)
                {
                    try
                    {
                        var taxFactor = decimal.Parse(ConfigurationManager.AppSettings["TaxFactor"] ?? "0", CultureInfo.InvariantCulture);
                        return taxFactor;
                    }
                    catch (Exception e)
                    {
                        _log.Error(e);
                        _taxAmount = 0M;
                    }
                }

                return _taxAmount;
            }
        }

        private decimal? TransactionFeeRate
        {
            get
            {
                if (_transactionFeeRate == null)
                {
                    try
                    {
                        var taxFactor = decimal.Parse(ConfigurationManager.AppSettings["TransactionFeeRate"] ?? "16");
                        return taxFactor;
                    }
                    catch (Exception e)
                    {
                        _log.Error(e);
                        _taxAmount = 0M;
                    }
                }

                return _taxAmount;
            }
        }

        public static PaymentManagerAbstract PaymentProcessorService
        {
            get
            {
                if (ConfigurationManager.AppSettings["PaymentProcessor"].Equals("AMP"))
                {
                    if (_paymentProcessorService == null)
                    {
                        var validator = ConfigurationManager.AppSettings["PaymentProcessorService-AMP"];
                        if (!string.IsNullOrEmpty(validator))
                        {
                            _paymentProcessorService = (PaymentManagerAbstract)Activator.CreateInstance(Type.GetType(validator));
                        }
                    }
                }
                else
                {
                    if (_paymentProcessorService == null)
                    {
                        var validator = ConfigurationManager.AppSettings["PaymentProcessorService-GlobalCom"];
                        if (!string.IsNullOrEmpty(validator))
                        {
                            _paymentProcessorService = (PaymentManagerAbstract)Activator.CreateInstance(Type.GetType(validator));
                        }
                    }
                }

                return _paymentProcessorService;
            }
        }

        public void ProcessTerminalPayment(PaymentAttempt paymentAttempt)
        {
            try
            {
                IsPaymentTransactionInProgress = true;

                var comPortParam = ConfigurationManager.AppSettings["PaymentProcessor-GlobalCom-ComPort"];
                var baudsParam = ConfigurationManager.AppSettings["PaymentProcessor-GlobalCom-Bauds"];

                if (ConfigurationManager.AppSettings["PaymentProcessor"].Equals("AMP"))
                {
                    comPortParam = ConfigurationManager.AppSettings["PaymentProcessor-AMP-ComPort"];
                    baudsParam = ConfigurationManager.AppSettings["PaymentProcessor-AMP-Bauds"];
                }

                TerminalState = TerminalState.PaymentInProgress;
                this.AmountToCharge = paymentAttempt.AmountToCharge;

                var amount = paymentAttempt.AmountToCharge;
                CurrentClientDescription = " " + amount.ToString("C");

                GeneratePaymentResponse paymentResult = null;

                if (UseFakePos)
                {
                    //README: PAYMENT INTERFACE STUB, NOT INTENTED FOR PRODUCTION USE!
                    //THIS WILL APPROVE ANY INCOMING PAYMENT TRANSACTION. PROCEED WITH CAUTION!
                    paymentResult = new GeneratePaymentResponse
                    {
                        TransactionApproved = true,
                        TransactionNumber = "888888",
                        TransactionStatus = 1,
                        TransactionId = "pppppppp-aaaa-nnnn-gggg-oooooooooooo",
                        XmlData = "<xml></xml>"
                    };

                    /*paymentResult = new GeneratePaymentResponse
                    {
                        TransactionApproved = false,
                        TransactionNumber = "222222",
                        TransactionStatus = 1,
                        ErrorMessage = "Error -002",
                        TransactionId = "eeeeeeee-aaaa-nnnn-gggg-oooooooooooo",
                        Data = (System.Collections.Generic.Dictionary<string, string>)JsonConvert.DeserializeObject<Dictionary<string, string>>("{\"Transaction type\":\"SALE\", \"TID\":\"25076104\", \"Card type\":\"Contactless\", \"Total\":\"USD2.32\", \"Err code\":\"-002\", \"Date\":\"20/06/23\", \"Time\":\"17:02\", \"Merchant\":\"1234567890\", \"Pan\":\"xxxxxxxxxxxx9807\", \"Application\":\"VISA CREDIT\", \"AID\":\"A0000000031010\", \"95\":\"0000000000\", \"9A\":\"230620\", \"9F21\":\"170247\", \"9F10\":\"1F420132A0000000001003027300000000400000000000000000000000000000\", \"5F2A\":\" 840\", \"9F26\":\"795D3C063B501923\", \"9F36\":\"016C\", \"Result\":\"DECLINED OFFLINE\"}")
                    };*/

                    Thread.Sleep(3000);
                }
                else
                {
                    if (ConfigurationManager.AppSettings["PaymentProcessor"].Equals("GlobalCom"))
                    {
                        Thread.Sleep(1500);
                    }

                    paymentResult = PaymentProcessorService.ProcessPayment(
                        new GeneratePaymentRequest
                        {
                            Amount = amount,
                            ComPort = int.Parse(comPortParam),
                            Bauds = int.Parse(baudsParam),
                            TimeoutInSeconds = paymentAttempt.ShowTime.HasValue ? paymentAttempt.ShowTime.Value : 0,
                        }
                    );
                    TerminalState = TerminalState.PaymentInProgress;
                }

                if (paymentResult != null)
                {
                    if (paymentResult?.TransactionApproved == true)
                    {
                        _log.Debug("Payment was captured");
                        TerminalState = TerminalState.PaymentApproved;
                        paymentAttempt.Processed = true;
                        PaymentService.UpsertPaymentAttempt(paymentAttempt);

                        Thread.Sleep(1000);
                    }
                    else
                    {
                        if (paymentResult?.TransactionStatus == 1)
                        {
                            TerminalState = TerminalState.PaymentDenied;
                        }
                        else
                        {
                            TerminalState = TerminalState.PaymentFailed;
                            this.TextToShowOnUI = paymentResult?.UIErrorMessage;
                        }

                        Thread.Sleep(3000);

                        var timeDiff = (DateTime.Now - paymentAttempt.Date.Value).TotalMilliseconds;

                        _log.Debug("Time Diff (ms): " + timeDiff);

                        //marked as processed if after some time, transaction timeout (to assume client left without paying)
                        if (timeDiff > PaymentRetryTimeout && paymentResult?.ErrorMessage?.ToUpper().Contains("TIMEOUT") == true)
                        {
                            _log.DebugFormat("Payment attempt occurred {0} ago, discarding it.", timeDiff.ToString());
                            paymentAttempt.Processed = true;
                            PaymentService.UpsertPaymentAttempt(paymentAttempt);
                        }
                    }

                    //if (LastPaymentApiEventDate.HasValue && (DateTime.Now - LastPaymentApiEventDate.Value).TotalMilliseconds > sendApiEventTimeout)
                    //{
                    //    LastPaymentApiEventDate = DateTime.Now;
                    //    ProcessPaymentToApi(paymentAttempt.Plate, amount, paymentAttempt.TransientId, paymentResult);
                    //}

                    LastPaymentApiEventDate = DateTime.Now;
                    ProcessPaymentToApi(paymentAttempt.Plate, paymentAttempt.Phone, amount, paymentAttempt.TransientId, paymentResult);

                    if (ConfigurationManager.AppSettings["PaymentProcessor"] == "GlobalCom" && paymentResult.Data != null && paymentResult.Data.ContainsKey("Err code") && paymentResult.Data["Err code"].Trim() == "-002")
                    {
                        try
                        {
                            _log.Info("ERROR -002 --> START OPERATIONS --> ResetGlobalCom -- OpenBarrier  -- SendMail");
                            //INCULIR 0x27
                            GlobalComPaymentManager.ResetDevice(int.Parse(baudsParam), "COM" + comPortParam);
                            _log.Info("ERROR -002 -->  ResetGlobalCom -- OK");
                            OpenBarrier();
                            _log.Info("ERROR -002 --> OpenBarrier -- OK");
                            Utils.sendMail("GLOBALCOM NETWORK COMMUNICATION ERROR", "CRITICAL ERROR LG1, GlobalCom CC reader not working as TLS connection cannot be established. CC Reader Reset.");
                            _log.Info("ERROR -002 --> SendMail -- OK");
                            //_log.Info("LANZANDO REINICIO " + ConfigurationManager.AppSettings["RestarProcess"]);
                            //Process.Start(ConfigurationManager.AppSettings["RestarProcess"]);
                        }catch (Exception ex)
                        {
                            _log.Error("ERROR -002 EX:" + ex.ToString());
                            OpenBarrier();
                            Utils.sendMail("GLOBALCOM NETWORK COMMUNICATION ERROR E", "CRITICAL ERROR LG1, GlobalCom CC reader not working as TLS connection cannot be established. CC Reader Reset.");
                        }
                    }


                    TerminalState = TerminalState.PaymentFinalized;
                    Thread.Sleep(2000);
                }
                else
                {
                    PaymentService.UpsertPaymentAttempt(paymentAttempt);
                    TerminalState = TerminalState.PaymentFinalized;
                    _log.Debug("No payment info received. Setting terminal state to PaymentFinalized");
                    Thread.Sleep(2000);
                }

                IsPaymentTransactionInProgress = false;
            }catch (Exception ex)
            {
                _log.Error("GLOBALCOM 002:" + ex);
            }
        }

        /// <summary>
        /// Notifies payment attempt to API and updates payment status on local DB.
        /// </summary>
        /// <param name="plate"></param>
        /// <param name="amount"></param>
        /// <param name="transientId"></param>
        /// <param name="paymentResult"></param>
        private void ProcessPaymentToApi(string plate, string phone, decimal amount, string transientId, GeneratePaymentResponse paymentResult)
        {
            var ticketId = Guid.NewGuid().ToString();
            if (!string.IsNullOrEmpty(paymentResult.TransactionId))
                ticketId = paymentResult.TransactionId;
            _log.Debug("ticketId:" + ticketId + " - TicketVisibleNumber:" + ticketId.Substring(0, 8) + " - NewTicketVisibleNumber:" + ticketId.Substring(0, 9));
            var payment = new Payment
            {
                AuthNumber = paymentResult.TransactionNumber,
                Success = paymentResult.TransactionApproved,
                TicketVisibleNumber = ticketId.Substring(0, 9),
                InstallationId = this.Terminal.InstallationId,
                TerminalId = this.Terminal.TerminalNumber,
                InvoiceConsecutive = paymentResult.TransactionNumber,
                Status = PaymentStatus.Processed,
                TotalAmount = amount,
                DiscountAmount = 0M,
                PaymentDate = DateTime.Now,
                TicketId = ticketId,
                //TaxAmount = amount / (1M + (TaxAmountRate.Value / 100)),
                TransactionFeeAmount = amount / (1M + (TransactionFeeRate.Value / 100)),
            };

            var paymentInserted = PaymentService.UpsertPayment(payment);
            _log.Debug("Payment created: " + paymentInserted);

            if (paymentResult.TransactionApproved)
            {
                this.OpenBarrier();

                Thread.Sleep(printReceiptTimeout);
                if (PrintReceipt)
                {
                    PrintInvoice(payment);
                    PangoService.TapForReceipt(new TapForReceiptRequest { installationID = this.Terminal.InstallationId, terminalId = this.Terminal.TerminalNumber });
                }

                PrintReceipt = false;
            }

            var result = PangoService.PostChargePlateResult(new ApiDtos.ChargePlateResultRequest
            {
                paymentId = paymentResult.TransactionNumber ?? "",
                transactionId = paymentResult.TransactionId ?? "", 
                amount = amount,
                installationID = this.Terminal.InstallationId,
                terminalId = this.Terminal.TerminalNumber,
                result = paymentResult.TransactionApproved ? "OK" : "KO",
                transientID = transientId,
                plate = plate,
                phone = phone,
                errorDescription = !paymentResult.TransactionApproved ? paymentResult.ErrorMessage : ""
            });

            _log.Debug("PostChargePlateResult sent: " + result);
        }

        private void PrintInvoice(Payment transaction)
        {
            if (transaction == null)
            {
                _log.Warn("No receipt can be printed, argument is null.");
                return;
            }

            try
            {
                _log.Debug("Printing invoice...");

                var showTax = bool.Parse(ConfigurationManager.AppSettings["Invoice.ShowTax"] ?? "false");

                Bitmap bitmap = null;
                DateTime now2 = DateTime.Now;
                string[] header = GetInvoiceHeader();

                string[] footer = GetInvoiceFooter();
                //Decimal subTotal = transaction.TotalAmount / (Decimal.One + TaxAmountRate.Value / new Decimal(100));
                //Decimal tax = 0;

                StringBuilder sbTextoFactura = new StringBuilder();

                sbTextoFactura.AppendLine("");
                /*sbTextoFactura.AppendLine("");
                sbTextoFactura.AppendLine("");*/

                // Add logo and header
                /*sbTextoFactura.AppendLine("");
                sbTextoFactura.AppendLine("");
                sbTextoFactura.AppendLine("");
                sbTextoFactura.AppendLine("");
                sbTextoFactura.AppendLine("");*/

                foreach (string str in header)
                {
                    sbTextoFactura.AppendLine(str);
                }

                sbTextoFactura.AppendLine("---------------------------------------------");
                sbTextoFactura.AppendLine(ConfigurationManager.AppSettings["ReceiptTitle"]);
                sbTextoFactura.AppendLine(ConfigurationManager.AppSettings["ReceiptNumber"] + transaction.TicketVisibleNumber);
                sbTextoFactura.AppendLine(ConfigurationManager.AppSettings["ReceiptInvoiceNumber"] + transaction.InvoiceConsecutive);
                sbTextoFactura.AppendLine("---------------------------------------------");

                //tax = transaction.TotalAmount - subTotal;

                sbTextoFactura.AppendLine("");
                sbTextoFactura.AppendLine("");
                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["ReceiptAmount"]))
                    sbTextoFactura.AppendLine(ConfigurationManager.AppSettings["ReceiptAmount"] + "$" + transaction.TotalAmount.ToString("####0.00"));
                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["ReceiptFee"]))
                    sbTextoFactura.AppendLine(ConfigurationManager.AppSettings["ReceiptFee"] + "$" + transaction.TransactionFeeAmount.ToString("####0.00"));
                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["ReceiptTotal"]))
                    sbTextoFactura.AppendLine(ConfigurationManager.AppSettings["ReceiptTotal"] + "$" + transaction.TotalAmount.ToString("####0.00"));
                //sbTextoFactura.AppendLine("Payment Method: Credit Card");

                sbTextoFactura.AppendLine(ConfigurationManager.AppSettings["ReceiptDate"] + transaction.PaymentDate.ToString(ConfigurationManager.AppSettings["ReceiptDateFormat"]));

                sbTextoFactura.AppendLine("");
                sbTextoFactura.AppendLine("");

                sbTextoFactura.AppendLine("");
                sbTextoFactura.AppendLine("");
                sbTextoFactura.AppendLine("");
                sbTextoFactura.AppendLine("");
                sbTextoFactura.AppendLine("");
                sbTextoFactura.AppendLine("");
                sbTextoFactura.AppendLine("");
                sbTextoFactura.AppendLine("");
                sbTextoFactura.AppendLine("");
                sbTextoFactura.AppendLine("");
                sbTextoFactura.AppendLine("");
                sbTextoFactura.AppendLine("");
                sbTextoFactura.AppendLine("");
                sbTextoFactura.AppendLine("");

                foreach (string str in footer)
                {
                    sbTextoFactura.AppendLine(str);
                }

                sbTextoFactura.AppendLine("");
                sbTextoFactura.AppendLine("");

                #region Print QR Code

                var qrCodeWidth = int.Parse(ConfigurationManager.AppSettings["QrLogoWidth"] ?? "150");
                var qrCodeHeight = int.Parse(ConfigurationManager.AppSettings["QrLogoHeight"] ?? "150");
                var printQr = bool.Parse(ConfigurationManager.AppSettings["QrPrint"] ?? "false");

                if (printQr)
                {
                    BarcodeWriter barcodeWriter = new BarcodeWriter();
                    EncodingOptions encodingOptions = new EncodingOptions()
                    {
                        Width = qrCodeWidth,
                        Height = qrCodeHeight,
                        Margin = 0,
                        PureBarcode = false
                    };
                    encodingOptions.Hints.Add(EncodeHintType.ERROR_CORRECTION, ErrorCorrectionLevel.H);
                    barcodeWriter.Renderer = new BitmapRenderer();
                    barcodeWriter.Options = encodingOptions;
                    barcodeWriter.Format = BarcodeFormat.QR_CODE;
                    bitmap = barcodeWriter.Write(TicketQrText);

                    Graphics g = Graphics.FromImage(bitmap);

                    if (!string.IsNullOrEmpty(TicketQrLogo))
                    {
                        var logo = new Bitmap(TicketQrLogo);
                        g.DrawImage(logo, new Point((bitmap.Width - logo.Width) / 2, (bitmap.Height - logo.Height) / 2));
                    }
                }

               

                #endregion

                PrintDocument printDocument = new PrintDocument();
                printDocument.PrintPage +=
                    new PrintPageEventHandler((object sender, PrintPageEventArgs e) =>
                    {
                        e.Graphics.DrawString(sbTextoFactura.ToString(), new Font("Arial", 7), Brushes.Black, 3f, 0.0f, new StringFormat());
                        if (printQr)
                        {
                            e.Graphics.DrawImage(bitmap, new Point(5, 200));
                        }
                        if (bool.Parse(ConfigurationManager.AppSettings["QrBussinesLogo"] ?? "false"))
                        {
                            Bitmap bitmapLogo = new Bitmap(ConfigurationManager.AppSettings["LogoBussines"]);
                            e.Graphics.DrawImage(bitmapLogo, new Point(5, 5));
                        }
                    });
                try
                {
                    printDocument.Print();
                }
                catch (InvalidPrinterException ipe)
                {
                    _log.Error("PRINTER ERROR", ipe);
                }
                catch (Exception e)
                {
                    _log.Error(e);
                }

                sbTextoFactura.Clear();

                _log.DebugFormat("Invoice #{0} printed successfully.", transaction.InvoiceConsecutive);
            }
            catch (Exception ex)
            {
                _log.Error("Cannot print invoice", ex);
            }
        }

        private string[] GetInvoiceHeader()
        {
            return new string[6]
              {
                   ConfigurationManager.AppSettings["ReceiptHeader1"]?? "",
                   ConfigurationManager.AppSettings["ReceiptHeader2"]?? "",
                   ConfigurationManager.AppSettings["ReceiptHeader3"]?? "",
                   ConfigurationManager.AppSettings["ReceiptHeader4"]?? "",
                   ConfigurationManager.AppSettings["ReceiptHeader5"]?? "",
                   ConfigurationManager.AppSettings["ReceiptHeader6"]?? ""
              };
        }

        private static string[] GetInvoiceFooter()
        {
            return new string[5]
            {
                ConfigurationManager.AppSettings["ReceiptFooter1"] ?? "",
                ConfigurationManager.AppSettings["ReceiptFooter2"] ?? "",
                ConfigurationManager.AppSettings["ReceiptFooter3"] ?? "",
                ConfigurationManager.AppSettings["ReceiptFooter4"] ?? "",
                ConfigurationManager.AppSettings["ReceiptFooter5"] ?? "",
            };
        }

        #endregion

        #region Barcode reader functions
        public bool InitReader(string readerComPort, int bauds = 9600)
        {
            try
            {
                if (readerSource == "Serial")
                {
                    _readerSerialPort = new SerialPort
                    {
                        BaudRate = bauds,
                        StopBits = StopBits.One,
                        Parity = Parity.None,
                        PortName = readerComPort,
                        ReadTimeout = 4000,
                        WriteTimeout = 6000,
                        Handshake = Handshake.None,
                        RtsEnable = true,
                        DtrEnable = true,
                        ReceivedBytesThreshold = 9,
                        Encoding = Encoding.Default
                    };
                    _readerSerialPort.Open();
                    _readerSerialPort.ErrorReceived += new SerialErrorReceivedEventHandler(reader_OnSerialErrorReceived);
                    _readerSerialPort.DataReceived += new SerialDataReceivedEventHandler(reader_OnSerialDataReceived);
                }

                if(readerSource == "Output")
                {
                    Thread readerOutputThread = new Thread(() =>
                    {
                        while (true)
                        {
                            try
                            {
                                if (this.ReadInputValue && !ReadingInputValue)
                                {
                                    ReadingInputValue = true;

                                    _log.Debug("Got value from InputValue without trim: " + this.InputValue + "#####");

                                    var outputData = this.InputValue.Trim();

                                    if (!string.IsNullOrEmpty(outputData))
                                    {
                                        _log.Debug("Got value from output: " + outputData);
                                        processQRCode(outputData);

                                        ReadInputValue = false;
                                        InputValue = null;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                _log.Warn(ex);
                            }
                            finally
                            {
                                Thread.Sleep(800);
                            }
                        }
                    });

                    readerOutputThread.IsBackground = true;
                    readerOutputThread.Start();
                }

                return true;
            }
            catch (System.Exception e)
            {
                _log.Error(e);
                return false;
            }
        }
        
        private void reader_OnSerialDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort barcodeSerialPort = (SerialPort)sender;
            _log.Debug("***** Data Received on barcode reader *****");

            try
            {
                var codeRead = string.Empty;
                var info = string.Empty;

                info = barcodeSerialPort.ReadExisting();

                barcodeSerialPort.DiscardInBuffer();
                barcodeSerialPort.DiscardOutBuffer();

                if (!string.IsNullOrEmpty(info))
                {
                    _log.Debug("A code was read: " + info);
                    processQRCode(info);

                    //OnCodeWasRead(info);
                    //TerminalState = TerminalState.ReadingBarcode;

                    //codeRead = info.GetValidCode();

                    //_log.Debug("Get Valid Code: " + codeRead);

                    //#region do ticket validation

                    //_log.Debug("Searching locally for ticket.");
                    //var ticket = TicketService.GetTicketByCode(codeRead);

                    //if (ticket == null)
                    //{
                    //    //check code online
                    //    _log.Debug("Ticket is not on the local whitelist, CheckQR will be called");

                    //    var onlineCodeResponse = CheckQrCode(codeRead);

                    //    if (onlineCodeResponse != null && onlineCodeResponse.allowAccess)
                    //    {
                    //        _log.Debug("Online access granted. Result code: " + onlineCodeResponse.resultCode);

                    //        TerminalState = TerminalState.TicketApproved;

                    //        OpenBarrier();

                    //        NotifyEntryOrExitEvent(codeRead, eventTypeToSend, ticket);

                    //        ReadingInputValue = false;
                    //        TerminalState = TerminalState.StandBy;
                    //    }
                    //    else
                    //    {
                    //        _log.Debug("Ticket not found.");
                    //        if (_picobSerialPort?.IsOpen == true)
                    //        {
                    //            _log.Debug($"ENTRA AL PUERTO - OPEN {info}.");
                    //            _picobSerialPort.Write(InputCommands.SendTicketDeniedOnlineSignal, 0, InputCommands.OpenBarrier.Length);
                    //        }
                    //        TerminalState = TerminalState.TicketNotFound;

                    //        TerminalState = TerminalState.StandBy;

                    //        ReadingInputValue = false;
                    //        return;
                    //    }
                    //}
                    //else
                    //{
                    //    if (!ticket.IsValid)
                    //    {
                    //        _log.Debug("invalid ticket.");
                    //        TerminalState = TerminalState.TicketDenied;

                    //        TerminalState = TerminalState.StandBy;

                    //        ReadingInputValue = false;
                    //        return;
                    //    }

                    //    if ((ticket.HasBeenUsed && eventTypeToSend == "Entry") ||
                    //            !ticket.HasBeenUsed && eventTypeToSend == "Exit")
                    //    {
                    //        //check qr code for ANTIPASSBACK
                    //        var onlineCodeResponse = CheckQrCode(codeRead, true);

                    //        if (onlineCodeResponse != null && onlineCodeResponse.allowAccess)
                    //        {
                    //            _log.Debug("Online access granted. Result code: " + onlineCodeResponse.resultCode);

                    //            TerminalState = TerminalState.TicketApproved;

                    //            OpenBarrier();

                    //            //register entry
                    //            NotifyEntryOrExitEvent(codeRead, eventTypeToSend, ticket);

                    //            ReadingInputValue = false;
                    //            TerminalState = TerminalState.StandBy;
                    //            return;
                    //        }
                    //        else
                    //        {
                    //            _log.Debug($"Online access denied for QR code {info}.");

                    //            if (_picobSerialPort?.IsOpen == true)
                    //            {
                    //                _log.Debug($"ENTRA AL PUERTO - OPEN {info}.");
                    //                _picobSerialPort.Write(InputCommands.SendTicketDeniedOnlineSignal, 0, InputCommands.OpenBarrier.Length);
                    //            }

                    //            TerminalState = TerminalState.TicketDenied;

                    //            TerminalState = TerminalState.StandBy;

                    //            ReadingInputValue = false;
                    //            return;
                    //        }
                    //    }

                    //    var now = DateTime.Now;

                    //    if (now < ticket.StartDate || now > ticket.EndDate)
                    //    {
                    //        var onlineCodeResponse = CheckQrCode(codeRead);

                    //        if (onlineCodeResponse != null && onlineCodeResponse.allowAccess)
                    //        {
                    //            _log.Debug("Online access granted. Result code: " + onlineCodeResponse.resultCode);

                    //            TerminalState = TerminalState.TicketApproved;

                    //            OpenBarrier();

                    //            //register entry
                    //            NotifyEntryOrExitEvent(codeRead, eventTypeToSend, ticket);

                    //            ReadingInputValue = false;
                    //            TerminalState = TerminalState.StandBy;

                    //            return;
                    //        }
                    //        else
                    //        {
                    //            _log.Debug("date out of range, access denied.");
                    //            if (_picobSerialPort?.IsOpen == true)
                    //            {
                    //                _log.Debug($"ENTRA AL PUERTO - OPEN {info}.");
                    //                _picobSerialPort.Write(InputCommands.SendTicketDeniedOnlineSignal, 0, InputCommands.OpenBarrier.Length);
                    //            }

                    //            TerminalState = TerminalState.TicketDenied;

                    //            TerminalState = TerminalState.StandBy;

                    //            ReadingInputValue = false;
                    //            return;
                    //        }
                    //    }

                    //    TerminalState = TerminalState.TicketApproved;

                    //    NotifyEntryOrExitEvent(codeRead, eventTypeToSend, ticket);

                    //    TicketService.UpdateTicket(ticket);

                    //    _log.Debug("entry event sent to pango.");

                    //    OpenBarrier();

                    //    ReadingInputValue = false;
                    //    TerminalState = TerminalState.StandBy;
                    //}

                    //#endregion do ticket validation
                }
            }
            catch (InvalidOperationException ioe)
            {
                _log.Error(ioe);
            }
            catch (Exception ex)
            {
                _log.Warn(ex);
            }
        }

        private void processQRCode(string info)
        {
            _log.Debug("***** processQRCode *****");

            var codeRead = string.Empty;

            new Thread(new ThreadStart(() =>
            {
                OnCodeWasRead(info);
            })).Start();

            TerminalState = TerminalState.ReadingBarcode;

            codeRead = info.GetValidCode();

            _log.Debug("Get Valid Code: " + codeRead);

            #region do ticket validation

            _log.Debug("Searching locally for ticket.");
            var ticket = TicketService.GetTicketByCode(codeRead);

            if (ticket == null)
            {
                //check code online
                _log.Debug("Ticket is not on the local whitelist, CheckQR will be called");

                var onlineCodeResponse = CheckQrCode(codeRead);

                if (onlineCodeResponse != null && onlineCodeResponse.allowAccess)
                {
                    _log.Debug("Online access granted. Result code: " + onlineCodeResponse.resultCode);

                    TerminalState = TerminalState.TicketApproved;

                    OpenBarrier();

                    NotifyEntryOrExitEvent(codeRead, eventTypeToSend, ticket);

                    ReadingInputValue = false;
                    new Thread(new ThreadStart(() =>
                    {
                        Thread.Sleep(3000);
                        TerminalState = TerminalState.StandBy;
                    })).Start();
                }
                else if (onlineCodeResponse != null && onlineCodeResponse.resultCode == 0)
                {
                    _log.Debug("Ignore");
                }
                else if (onlineCodeResponse != null && onlineCodeResponse.resultCode == 2)
                {
                    _log.Debug("Entry Ticket");
                    TerminalState = TerminalState.TicketEntry;

                    new Thread(new ThreadStart(() =>
                    {
                        Thread.Sleep(3000);
                        TerminalState = TerminalState.StandBy;
                    })).Start();
                    ReadingInputValue = false;
                }
                else if (onlineCodeResponse != null && onlineCodeResponse.resultCode == 3)
                {
                    _log.Debug("Not Found");
                    TerminalState = TerminalState.NotFound;

                    new Thread(new ThreadStart(() =>
                    {
                        Thread.Sleep(3000);
                        TerminalState = TerminalState.StandBy;
                    })).Start();
                    ReadingInputValue = false;
                }
                else if (onlineCodeResponse != null && onlineCodeResponse.resultCode == 4)
                {
                    _log.Debug("Permit Expired - New Payment");
                    TerminalState = TerminalState.PermitExpired;
                    WhiteListId = onlineCodeResponse.whiteListId;

                    new Thread(new ThreadStart(() =>
                    {
                        Thread.Sleep(3000);
                        TerminalState = TerminalState.StandBy;
                        WhiteListId = 0;
                    })).Start();
                    ReadingInputValue = false;
                }
                else if (onlineCodeResponse != null && onlineCodeResponse.resultCode == 5)
                {
                    _log.Debug("Pango Payment Need CVV");
                    TerminalState = TerminalState.PangoPaymentCVV;
                    PaymAmount = (float)onlineCodeResponse.amount;
                    QR = codeRead;

                    new Thread(new ThreadStart(() =>
                    {
                        Thread.Sleep(3000);
                        TerminalState = TerminalState.StandBy;
                    })).Start();
                    ReadingInputValue = false;
                }
                else if (onlineCodeResponse != null && onlineCodeResponse.resultCode == -2)
                {
                    _log.Debug("Pango Pass Error - " + onlineCodeResponse.description);
                    TerminalState = TerminalState.PangoPassError;
                    PangoPassError = onlineCodeResponse.description;

                    new Thread(new ThreadStart(() =>
                    {
                        Thread.Sleep(3000);
                        TerminalState = TerminalState.StandBy;
                    })).Start();
                    ReadingInputValue = false;
                }
                else
                {
                    _log.Debug("Ticket not found.");
                    if (_picobSerialPort?.IsOpen == true)
                    {
                        _log.Debug($"ENTRA AL PUERTO - OPEN {info}.");
                        _picobSerialPort.Write(InputCommands.SendTicketDeniedOnlineSignal, 0, InputCommands.OpenBarrier.Length);
                    }
                    TerminalState = TerminalState.TicketNotFound;

                    new Thread(new ThreadStart(() =>
                    {
                        Thread.Sleep(3000);
                        TerminalState = TerminalState.StandBy;
                    })).Start();

                    ReadingInputValue = false;
                    return;
                }
            }
            else
            {
                if (!ticket.IsValid)
                {
                    _log.Debug("invalid ticket.");
                    TerminalState = TerminalState.TicketDenied;

                    new Thread(new ThreadStart(() =>
                    {
                        Thread.Sleep(3000);
                        TerminalState = TerminalState.StandBy;
                    })).Start();

                    ReadingInputValue = false;
                    return;
                }

                if ((ticket.HasBeenUsed && eventTypeToSend == "Entry") ||
                        !ticket.HasBeenUsed && eventTypeToSend == "Exit")
                {
                    //check qr code for ANTIPASSBACK
                    var onlineCodeResponse = CheckQrCode(codeRead, true);

                    if (onlineCodeResponse != null && onlineCodeResponse.allowAccess)
                    {
                        _log.Debug("Online access granted. Result code: " + onlineCodeResponse.resultCode);

                        TerminalState = TerminalState.TicketApproved;

                        OpenBarrier();

                        //register entry
                        NotifyEntryOrExitEvent(codeRead, eventTypeToSend, ticket);

                        ReadingInputValue = false;
                        new Thread(new ThreadStart(() =>
                        {
                            Thread.Sleep(3000);
                            TerminalState = TerminalState.StandBy;
                        })).Start();
                        return;
                    }
                    else if (onlineCodeResponse != null && onlineCodeResponse.resultCode == 2)
                    {
                        _log.Debug("Ticket Expired - New Payment");
                        TerminalState = TerminalState.TicketEntry;

                        new Thread(new ThreadStart(() =>
                        {
                            Thread.Sleep(3000);
                            TerminalState = TerminalState.StandBy;
                        })).Start();

                        ReadingInputValue = false;
                        return;
                    }
                    else if (onlineCodeResponse != null && onlineCodeResponse.resultCode == 4)
                    {
                        _log.Debug("Permit Expired - New Payment");
                        TerminalState = TerminalState.PermitExpired;
                        WhiteListId = onlineCodeResponse.whiteListId;

                        new Thread(new ThreadStart(() =>
                        {
                            Thread.Sleep(3000);
                            TerminalState = TerminalState.StandBy;
                            WhiteListId = 0;
                        })).Start();
                        ReadingInputValue = false;
                        return;
                    }
                    else
                    {
                        _log.Debug($"Online access denied for QR code {info}.");

                        if (_picobSerialPort?.IsOpen == true)
                        {
                            _log.Debug($"ENTRA AL PUERTO - OPEN {info}.");
                            _picobSerialPort.Write(InputCommands.SendTicketDeniedOnlineSignal, 0, InputCommands.OpenBarrier.Length);
                        }

                        TerminalState = TerminalState.TicketDenied;

                        new Thread(new ThreadStart(() =>
                        {
                            Thread.Sleep(3000);
                            TerminalState = TerminalState.StandBy;
                        })).Start();

                        ReadingInputValue = false;
                        return;
                    }
                }

                var now = DateTime.Now;

                if (now < ticket.StartDate || now > ticket.EndDate)
                {
                    var onlineCodeResponse = CheckQrCode(codeRead);

                    if (onlineCodeResponse != null && onlineCodeResponse.allowAccess)
                    {
                        _log.Debug("Online access granted. Result code: " + onlineCodeResponse.resultCode);

                        TerminalState = TerminalState.TicketApproved;

                        OpenBarrier();

                        //register entry
                        NotifyEntryOrExitEvent(codeRead, eventTypeToSend, ticket);

                        ReadingInputValue = false;
                        TerminalState = TerminalState.StandBy;

                        return;
                    }
                    else
                    {
                        _log.Debug("date out of range, access denied.");
                        if (_picobSerialPort?.IsOpen == true)
                        {
                            _log.Debug($"ENTRA AL PUERTO - OPEN {info}.");
                            _picobSerialPort.Write(InputCommands.SendTicketDeniedOnlineSignal, 0, InputCommands.OpenBarrier.Length);
                        }

                        TerminalState = TerminalState.TicketDenied;

                        new Thread(new ThreadStart(() =>
                        {
                            Thread.Sleep(3000);
                            TerminalState = TerminalState.StandBy;
                        })).Start();

                        ReadingInputValue = false;
                        return;
                    }
                }

                TerminalState = TerminalState.TicketApproved;

                NotifyEntryOrExitEvent(codeRead, eventTypeToSend, ticket);

                TicketService.UpdateTicket(ticket);

                _log.Debug("entry event sent to pango.");

                OpenBarrier();

                ReadingInputValue = false;
                TerminalState = TerminalState.StandBy;
            }

            #endregion do ticket validation
        }

        private void reader_OnSerialErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            _log.Error("Error on reader port.");
        }

        #endregion

        #region Picob serial port functions
        private bool InitPicob(string readerComPort, int bauds)//115200
        {
            _picobSerialPort = new SerialPort
            {
                BaudRate = bauds,
                DataBits = 8,
                StopBits = StopBits.One,
                Parity = Parity.None,
                PortName = readerComPort,
                ReadTimeout = 4000,
                WriteTimeout = 6000,
                Handshake = Handshake.None,
                Encoding = Encoding.ASCII,
                ReceivedBytesThreshold = 9,
            };

            _picobSerialPort.ErrorReceived += new SerialErrorReceivedEventHandler(OnSerialErrorReceived);
            _picobSerialPort.DataReceived += new SerialDataReceivedEventHandler(picob_OnSerialDataReceived);

            try
            {
                _picobSerialPort.Open();
                _log.Debug("Picob connected successfully");

                return true;
            }
            catch (UnauthorizedAccessException ex)
            {
                _log.Fatal("CANNOT CONNECT TO PICOB DEVICE!", ex);
                return false;
            }
            catch (IOException ex)
            {
                _log.Fatal("CANNOT CONNECT TO OUTPUT DEVICE!", ex);
                return false;
            }
            return true;
        }

        private void picob_OnSerialDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            //***** Data Received on picob reader ****

            SerialPort picobSerialPort = (SerialPort)sender;
            var outputDeviceResponse = picobSerialPort.ReadExisting();

            try
            {
                var outputResponse = DecodeDeviceResponse(outputDeviceResponse);

                if (outputResponse != null && outputResponse.Count > 0)
                {
                    #region UPDATE TERMINAL STATE

                    //skip processing if we have these READ-ONLY states
                    if (TerminalState == TerminalState.PaymentInProgress)
                    {
                        return;
                    }

                    //IS STANDING BY?
                    if (outputResponse.Keys.Contains("1") && outputResponse["1"].Value == "0" &&
                        outputResponse.Keys.Contains("2") && outputResponse["2"].Value == "0")
                    {
                        IsVehiclePresent = false;

                        if (TerminalState == TerminalState.VehiclePresent)
                        {
                            VehiclePresenceNotDetected();
                        }
                    }
                    else if ((outputResponse.Keys.Contains("1") && outputResponse["1"].Value == "1") ||
                    (outputResponse.Keys.Contains("2") && outputResponse["2"].Value == "1")) //IS VEHICLE DETECTED?
                    {
                        IsVehiclePresent = true;

                        if (TerminalState == TerminalState.StandBy)
                        {
                            VehiclePresenceDetected();
                        }
                    }

                    #endregion UPDATE TERMINAL STATE
                }
            } catch (Exception ex)
            {
                _log.Error("Error on picob_OnSerialDataReceived -- outputDeviceResponse: " + outputDeviceResponse, ex);
            }
        }

        private void OnSerialErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            _log.Error("Error on picob port.");
        }

        #endregion 

        public void Start()
        {
            _log.Debug("Controller Start operation in progress.");

            TerminalState = TerminalState.StandBy;

            TemplatesDictionary = TemplateService.GetTemplatesByTerminal(this.Terminal.TerminalNumber).
                ToDictionary(x => x.Id);

            #region Main Terminal Application Processing Threads

            #region Process api operations Carga inicial

            Thread apiOperationsThread = new Thread(async () =>
            {
                #region Process whitelist

                _log.Debug("Processing Whitelist...");

                var whitelistInterval = int.Parse(ConfigurationManager.AppSettings["WhitelistInterval"] ?? "3600000");

                while (true)
                {
                    DownloadWhitelist();

                    _log.DebugFormat("Waiting {0} ms to download the whitelist again", whitelistInterval);
                    Thread.Sleep(whitelistInterval);
                }

                #endregion Process whitelist
            });

            apiOperationsThread.IsBackground = true;
            apiOperationsThread.Start();

            #endregion Process api operations Carga inicial

            #region Process terminal api operations

            Thread apiTerminalOperationsThread = new Thread(async () =>
            {
                #region

                _log.Debug("Processing Operation...");

                var interval = int.Parse(ConfigurationManager.AppSettings["ApiTerminalInterval"] ?? "1000");

                while (true)
                {
                    try
                    {
                        _terminal = TerminalService.GetTerminal(terminalNumber);

                        if (_terminal != null)
                        {
                            if (_terminal.ForceOpenBarrier)
                            {
                                OpenBarrier();
                                _terminal.ForceOpenBarrier = false;

                                //update terminal
                                TerminalService.UpdateTerminal(_terminal);
                            }

                            if (_terminal.ForceWhitelistUpload)
                            {
                                DeleteExpiredTickets();
                                DownloadWhitelist();
                                _terminal.ForceWhitelistUpload = false;

                                //update terminal
                                TerminalService.UpdateTerminal(_terminal);
                            }
                        }

                        //_log.DebugFormat("Waiting {0} ms to check for incoming operations", interval);
                        Thread.Sleep(interval);
                    }
                    catch (Exception e)
                    {
                        _log.Error("Error processing operation", e);
                    }
                }

                #endregion 
            });

            apiTerminalOperationsThread.IsBackground = true;
            apiTerminalOperationsThread.Start();
            #endregion Process terminal api operations

            #region Process incoming information from reader thread
            /*
            Thread readerOperationsThread = new Thread(() =>
            {
                var codeRead = string.Empty;

                while (true)
                {
                    try
                    {
                        var info = string.Empty;

                        if (readerSource == "Output")
                        {
                            if (this.ReadInputValue && !ReadingInputValue)
                            {
                                ReadingInputValue = true;

                                _log.Debug("Got value from InputValue without trim: " + this.InputValue + "#####");

                                var outputData = this.InputValue.Trim();

                                if (!string.IsNullOrEmpty(outputData))
                                {
                                    info = outputData;

                                    _log.Debug("Got value from output: " + info);

                                    ReadInputValue = false;
                                    InputValue = null;
                                }
                            }
                            else
                            {
                                continue;
                            }
                        }
                        else if (readerSource == "Serial")
                        {
                            //_log.Debug("Trying to get value from serial");
                            info = _readerSerialPort.ReadExisting();
                        }

                        if (!string.IsNullOrEmpty(info))
                        {
                            _log.Debug("A code was read: " + info);

                            TerminalState = TerminalState.ReadingBarcode;

                            codeRead = info.GetValidCode();

                            _log.Debug("Get Valid Code: " + codeRead);
                            //TESTING KEYBOARD ENGLISH
                            //codeRead = codeRead.Replace('¿', '=');
                            //codeRead = codeRead.Replace('¡', '+');

                            #region do ticket validation

                            _log.Debug("Searching locally for ticket.");
                            var ticket = TicketService.GetTicketByCode(codeRead);

                            if (ticket == null)
                            {
                                //check code online
                                _log.Debug("Ticket is not on the local whitelist, CheckQR will be called");

                                var onlineCodeResponse = CheckQrCode(codeRead);

                                if (onlineCodeResponse != null && onlineCodeResponse.allowAccess)
                                {
                                    _log.Debug("Online access granted. Result code: " + onlineCodeResponse.resultCode);

                                    TerminalState = TerminalState.TicketApproved;

                                    OpenBarrier();

                                    NotifyEntryOrExitEvent(codeRead, eventTypeToSend, ticket);

                                    Thread.Sleep(3000);

                                    ReadingInputValue = false;
                                    TerminalState = TerminalState.StandBy;
                                }
                                else if (onlineCodeResponse != null && onlineCodeResponse.resultCode == 2)
                                {
                                    _log.Debug("Entry Ticket");
                                    TerminalState = TerminalState.TicketEntry;
                                    Thread.Sleep(3000);
                                    TerminalState = TerminalState.StandBy;
                                    ReadingInputValue = false;
                                }
                                else
                                {
                                    _log.Debug("Ticket not found.");
                                    if (_picobSerialPort?.IsOpen == true)
                                    {
                                        _log.Debug($"ENTRA AL PUERTO - OPEN {info}.");
                                        _picobSerialPort.Write(InputCommands.SendTicketDeniedOnlineSignal, 0, InputCommands.OpenBarrier.Length);
                                    }
                                    TerminalState = TerminalState.TicketNotFound;

                                    Thread.Sleep(3000);
                                    TerminalState = TerminalState.StandBy;

                                    ReadingInputValue = false;
                                    continue;
                                }
                            }
                            else
                            {
                                if (!ticket.IsValid)
                                {
                                    _log.Debug("invalid ticket.");
                                    TerminalState = TerminalState.TicketDenied;

                                    Thread.Sleep(3000);
                                    TerminalState = TerminalState.StandBy;

                                    ReadingInputValue = false;
                                    continue;
                                }

                                if ((ticket.HasBeenUsed && eventTypeToSend == "Entry") ||
                                        !ticket.HasBeenUsed && eventTypeToSend == "Exit")
                                {
                                    //check qr code for ANTIPASSBACK
                                    var onlineCodeResponse = CheckQrCode(codeRead, true);

                                    if (onlineCodeResponse != null && onlineCodeResponse.allowAccess)
                                    {
                                        _log.Debug("Online access granted. Result code: " + onlineCodeResponse.resultCode);

                                        TerminalState = TerminalState.TicketApproved;

                                        OpenBarrier();

                                        //register entry
                                        NotifyEntryOrExitEvent(codeRead, eventTypeToSend, ticket);

                                        Thread.Sleep(3000);

                                        ReadingInputValue = false;
                                        TerminalState = TerminalState.StandBy;
                                        continue;
                                    }
                                    else
                                    {
                                        _log.Debug($"Online access denied for QR code {info}.");

                                        if (_picobSerialPort?.IsOpen == true)
                                        {
                                            _log.Debug($"ENTRA AL PUERTO - OPEN {info}.");
                                            _picobSerialPort.Write(InputCommands.SendTicketDeniedOnlineSignal, 0, InputCommands.OpenBarrier.Length);
                                        }

                                        TerminalState = TerminalState.TicketDenied;

                                        Thread.Sleep(3000);
                                        TerminalState = TerminalState.StandBy;

                                        ReadingInputValue = false;
                                        continue;
                                    }
                                }

                                var now = DateTime.Now;

                                if (now < ticket.StartDate || now > ticket.EndDate)
                                {
                                    var onlineCodeResponse = CheckQrCode(codeRead);

                                    if (onlineCodeResponse != null && onlineCodeResponse.allowAccess)
                                    {
                                        _log.Debug("Online access granted. Result code: " + onlineCodeResponse.resultCode);

                                        TerminalState = TerminalState.TicketApproved;

                                        OpenBarrier();

                                        //register entry
                                        NotifyEntryOrExitEvent(codeRead, eventTypeToSend, ticket);

                                        Thread.Sleep(3000);

                                        ReadingInputValue = false;
                                        TerminalState = TerminalState.StandBy;

                                        continue;
                                    }
                                    else
                                    {
                                        _log.Debug("date out of range, access denied.");
                                        if (_picobSerialPort?.IsOpen == true)
                                        {
                                            _log.Debug($"ENTRA AL PUERTO - OPEN {info}.");
                                            _picobSerialPort.Write(InputCommands.SendTicketDeniedOnlineSignal, 0, InputCommands.OpenBarrier.Length);
                                        }

                                        TerminalState = TerminalState.TicketDenied;

                                        Thread.Sleep(3000);
                                        TerminalState = TerminalState.StandBy;

                                        ReadingInputValue = false;
                                        continue;
                                    }
                                }

                                TerminalState = TerminalState.TicketApproved;

                                NotifyEntryOrExitEvent(codeRead, eventTypeToSend, ticket);

                                TicketService.UpdateTicket(ticket);

                                _log.Debug("entry event sent to pango.");

                                OpenBarrier();

                                Thread.Sleep(3000);

                                ReadingInputValue = false;
                                TerminalState = TerminalState.StandBy;
                            }

                            #endregion do ticket validation

                            codeRead = string.Empty;
                        }

                        Thread.Sleep(100);
                    }
                    catch (InvalidOperationException ioe)
                    {
                        _log.Error(ioe);

                        if (!_readerSerialPort.IsOpen)
                        {
                            _readerSerialPort.Open();
                            _log.Info("Reader was reconnected successfully.");
                        }
                    }
                    catch (Exception e)
                    {
                        _log.Warn(e);
                    }
                    finally
                    {
                        Thread.Sleep(800);
                    }
                }
            });
            
            readerOperationsThread.IsBackground = true;
            readerOperationsThread.Start();
            */
            #endregion Process incoming information from reader thread

            #region Process incoming information from nano device thread
            /*
            Thread nanoCommandsThread = new Thread(async () =>
            {
                while (true)
                {
                    try
                    {
                        var outputDeviceResponse = _picobSerialPort.ReadExisting();

                        //_log.Debug(outputDeviceResponse);

                        var outputResponse = DecodeDeviceResponse(outputDeviceResponse);

                        if (outputResponse != null && outputResponse.Count > 0)
                        {
                            #region UPDATE TERMINAL STATE

                            //skip processing if we have these READ-ONLY states
                            if (TerminalState == TerminalState.PaymentInProgress)
                            {
                                continue;
                            }

                            //IS STANDING BY?
                            if (outputResponse.Keys.Contains("1") && outputResponse["1"].Value == "0" &&
                                outputResponse.Keys.Contains("2") && outputResponse["2"].Value == "0")
                            {
                                IsVehiclePresent = false;
                                if (TerminalState == TerminalState.VehiclePresent)
                                {
                                    VehiclePresenceNotDetected();

                                }

                            }
                            else if ((outputResponse.Keys.Contains("1") && outputResponse["1"].Value == "1") ||
                            (outputResponse.Keys.Contains("2") && outputResponse["2"].Value == "1")) //IS VEHICLE DETECTED?
                            {
                                //SetPresentVehicleInfo(outputResponse);
                                IsVehiclePresent = true;
                                if (TerminalState == TerminalState.StandBy)
                                {
                                    VehiclePresenceDetected();
                                }

                            }



                            #endregion UPDATE TERMINAL STATE
                        }

                        Thread.Sleep(1000);
                    }
                    catch (InvalidOperationException ioe)
                    {
                        _log.Error(ioe);

                        if (!__picobSerialPortpicov.IsOpen)
                        {

                            var picobRestartAttempts = 5;
                            var picobStarted = false;
                            try
                            {
                                _picobSerialPort.Dispose();
                                Thread.Sleep(100);

                                for (var i = 0; i < picobRestartAttempts; i++)
                                {
                                    picobStarted = InitPicob(ConfigurationManager.AppSettings["PicovComPort"], 9600);
                                    if (picobStarted)
                                    {
                                        break;
                                    }
                                    Thread.Sleep(500);
                                }

                                if (!picobStarted)
                                {
                                    _log.Fatal("Failing to restart picob, app will be restarted...");
                                    //reset?
                                }

                            }
                            catch (Exception fatalEx)
                            {
                                _log.Fatal("Failing to open picob port, app might be in unstable state", fatalEx);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        _log.Fatal("Exception occurred on Picob processing thread. Perhaps there is no serial comm with the nano or a malfunction occured.", e);
                    }
                    finally
                    {
                        Thread.Sleep(300);
                    }
                }
            });

            nanoCommandsThread.Start();
            */
            #endregion Process incoming information from nano device thread

            if (PaymentServiceEnabled)
            {
                Thread checkUnprocessedPaymentsThread = new Thread(() =>
                {
                    while (true)
                    {
                        try
                        {
                            var unprocessedPayments = PaymentService.GetPendingPaymentAttemptsWithinTime(this.Terminal.TerminalNumber);

                            if (unprocessedPayments?.Any() == true)
                            {
                                _log.Debug($"There are {unprocessedPayments.Count()} payments that are pending to be processed");

                                var lastPendingPayment = unprocessedPayments.OrderByDescending(x => x.Date).FirstOrDefault();

                                if (lastPendingPayment != null && !IsPaymentTransactionInProgress)
                                {
                                    _log.Debug("Processing the most recent payment request -- -- PaymentId:"+lastPendingPayment._id);

                                    ProcessTerminalPayment(lastPendingPayment);

                                    _log.Debug("Payment request processed");
                                }
                                else
                                {
                                    _log.Debug("Payment not processed. Transaction in progress " + IsPaymentTransactionInProgress);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            TerminalState = TerminalState.PaymentFailed;
                            _log.Error("Payment Failure occurred", e);

                            IsPaymentTransactionInProgress = false;
                            Thread.Sleep(1000);
                        }
                        finally
                        {
                            IsPaymentTransactionInProgress = false;
                            TerminalState = TerminalState.VehiclePresent;

                            TerminalController.PaymentProcessorService.CurrentReaderStatus = CurrentReaderStatus.Idle;

                            Thread.Sleep(pendingPaymentsInterval);
                        }
                    }
                });

                checkUnprocessedPaymentsThread.IsBackground = true;
                checkUnprocessedPaymentsThread.Start();
            }

            #region Check serial port status

            if (readerSource == "Serial" || ConfigurationManager.AppSettings["ShowTestInput"] == "false")
            {
                Thread checkSerialPortsThread = new Thread(async () =>
                {
                    #region

                    var interval = int.Parse(ConfigurationManager.AppSettings["CheckSerialPortsInterval"] ?? "5000");
                    var readerComPort = ConfigurationManager.AppSettings["ReaderComPort"];
                    var readerBauds = int.Parse(ConfigurationManager.AppSettings["ReaderBauds"] ?? "9600");

                    while (true)
                    {
                        //_log.Debug("Checking serial ports status...");

                        try
                        {
                            if ((bool)!_readerSerialPort?.IsOpen)
                            {
                                _readerSerialPort.Dispose();

                                if (InitReader(readerComPort, readerBauds))
                                {
                                    _log.Debug("BARCODE READER RECONNECTED SUCCESSFULLY");
                                }
                                else
                                {
                                    _log.Fatal("BARCODE READER COULD NOT BE CONNECTED!!");
                                }
                            }
                            else
                            {
                                //_log.Debug("BARCODE READER IS WORKING NORMALLY");
                            }
                        }
                        catch (Exception e)
                        {
                            _log.Error("Error processing serial port checking", e);
                        }
                        finally
                        {
                            //_log.DebugFormat("Waiting {0} ms to check for serial ports", interval);
                            Thread.Sleep(interval);
                        }
                    }
                    
                    #endregion 
                });
                
                checkSerialPortsThread.IsBackground = true;
                checkSerialPortsThread.Start();
            }
            #endregion

            #endregion
        }

        /// <summary>
        /// Checks a QR code read in the terminal into the Pango APIs.
        /// </summary>
        /// <param name="codeRead"></param>
        /// <param name="antiPassBack"></param>
        /// <returns></returns>
        private static CheckQRCodeResponse CheckQrCode(string codeRead, bool antiPassBack = false)
        {
            return PangoService.CheckQR(
                new CheckQRCodeRequest
                {
                    code = codeRead,
                    date = DateTime.UtcNow,
                    installationID = installationId,
                    serialNumber = serialNumber,
                    terminalId = terminalNumber,
                    terminalAccess = eventTypeToSend,
                    type = "QR",
                    antiPassBack = antiPassBack
                }
            );
        }

        /// <summary>
        /// Notify the API and local terminals of an entry or exit event.
        /// </summary>
        /// <param name="codeRead"></param>
        /// <param name="eventTypeToSend"></param>
        public void NotifyEntryOrExitEvent(string codeRead, string eventTypeToSend, AuthorizedTicket ticket)
        {
            if (ticket != null)
            {
                //set ticket state to update
                if (eventTypeToSend == "Entry")
                {
                    //indicate ticket is used (client is inside the parking)
                    ticket.HasBeenUsed = true;
                }
                else
                {
                    //indicate ticket is not used (client is leaving the parking)
                    ticket.HasBeenUsed = false;
                }
            }

            //notify entry/exit event to api
            string evResponse = PostEntryEventNotification(codeRead, eventTypeToSend);
            _log.Debug("Event response: " + evResponse);

            if (ticket != null)
            {
                // notify entry/exit event to other terminals
                var updateThread = new Thread(new ThreadStart(() =>
                {
                    var ips = ipsToUpdate.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                    if (ips?.Any() == true)
                    {
                        foreach (var ip in ips)
                        {
                            var ticketUpdateResponse = UpdateTicketInExternalTerminal(ip, ticket);
                            _log.DebugFormat("Ticket update response on {0}: {1}", ip, ticketUpdateResponse);
                        }
                    }
                }));
                updateThread.IsBackground = true;
                updateThread.Start();
            }
        }

        /// <summary>
        /// Send a charge plate request to Pango API
        /// </summary>
        /// <param name="plate">The plate entered by the user.</param>
        /// <returns>True if the request was received successfully by the api.</returns>
        public (bool, ChargePlateResponse) PostChargePlate(string plate)
        {
            return PangoService.ChargePlate(new ApiDtos.ChargePlateRequest
            {
                Amount = 0M,
                Currency = "USD",
                InstallationID = this.Terminal.InstallationId,
                TerminalId = this.Terminal.TerminalNumber,
                Plate = plate,
            });
        }

        /// <summary>
        /// Private method that sends the event notification to the Pango APIs.
        /// </summary>
        /// <param name="codeRead"></param>
        /// <param name="access"></param>
        /// <returns></returns>
        private static string PostEntryEventNotification(string codeRead, string access = "Entry")
        {
            return PangoService.PostEventNotification(new NotificationRequest
            {
                accessDate = DateTime.UtcNow,
                code = codeRead,
                installationID = installationId,
                serialNumber = serialNumber,
                terminalAccess = access,
                terminalId = terminalNumber,
                type = "QR"
            });
        }

        /// <summary>
        /// Deletes the expired tickets from the local database.
        /// </summary>
        private void DeleteExpiredTickets()
        {
            try
            {
                var deleteTickets = PangoService.DeleteExpiredTickets();
                if (deleteTickets)
                {
                    _log.Debug("Delete Tickets correctly.");
                }
            }
            catch (Exception e)
            {
                _log.Error("Error when Delete Tickets", e);
            }
        }

        /// <summary>
        /// Downloads the whitelist from the API and inserts the information into the local database.
        /// </summary>
        private void DownloadWhitelist()
        {
            try
            {
                var whitelistDownload = PangoService.DownloadWhitelist(
                     new WhiteListRequest
                     {
                         installationID = installationId,
                         serialNumber = serialNumber,
                         terminalId = terminalNumber
                     }
                 );

                if (whitelistDownload)
                {
                    _log.Debug("Whitelist downloaded correctly.");
                }
            }
            catch (Exception e)
            {
                _log.Error("Error when downloading whitelist", e);
            }
        }

        public void OpenBarrier()
        {
            if (_picobSerialPort.IsOpen)
                OpenPicob();
            else
            {
                _log.Debug("RECONNECTING PICOB");
                try
                {
                    _picobSerialPort.Dispose();

                    if (InitPicob(ConfigurationManager.AppSettings["PicovComPort"], int.Parse(ConfigurationManager.AppSettings["PicovBauds"] ?? "9600")))
                    {
                        _log.Debug("PICOB RECONNECTED SUCCESSFULLY");
                        OpenPicob();
                    }
                    else
                    {
                        _log.Error("PICOB COULD NOT BE CONNECTED!!");
                        Utils.sendMail("PICOB CONNECTION ERROR", "PICOB CONNECTION ERROR, Picob serial port is closed and the reconnection does not work.");
                        if (ConfigurationManager.AppSettings["PicobErrorReboot"].Equals("1"))
                        {
                            _log.Info("LANZANDO REINICIO");
                            Process.Start(ConfigurationManager.AppSettings["RestarProcess"]);
                        }
                    }
                }
                catch (Exception e)
                {
                    _log.Error("Error processing picob serial port reconnection", e);
                    Utils.sendMail("PICOB CONNECTION ERROR", "PICOB CONNECTION ERROR, Picob serial port is closed and the reconnection has failed.");
                }
            }
        }

        private void OpenPicob()
        {
            if (_picobSerialPort.IsOpen)
            {
                _picobSerialPort.Write(InputCommands.OpenBarrier, 0, InputCommands.OpenBarrier.Length);
                System.Threading.Thread.Sleep(300);
                var response = _picobSerialPort.ReadExisting();
                _log.Debug("Open Barrier Command sent. RESPONSE:" + response);
                
                _picobSerialPort.DiscardInBuffer();
                _picobSerialPort.DiscardOutBuffer();
            }
            else
                _log.Debug("Picob serial port is closed");
        }

        private bool UpdateTicketInExternalTerminal(string ip, AuthorizedTicket ticket)
        {
            var client = new RestClient("http://" + ip + "/PutTicket");
            client.Timeout = 30000;

            var request = new RestRequest(Method.PUT);
            request.AddHeader("Content-Type", "application/json");
            request.AddJsonBody(ticket);

            IRestResponse response = client.Execute(request);
            _log.Debug("update ticket response from " + ip + ":" + response.Content);

            return response.IsSuccessful;
        }

        private Dictionary<string, TerminalIndicator> DecodeDeviceResponse(string outputDeviceResponse)
        {
            var result = new Dictionary<string, TerminalIndicator>();

            if (!string.IsNullOrEmpty(outputDeviceResponse))
            {
                var splittedList = outputDeviceResponse.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                if (splittedList?.Length > 0)
                {
                    //restrict maximum of appended incoming messages from the get available bytes operation
                    if (splittedList.Length > maxPicovIncomingMessages && maxPicovIncomingMessages != 0)
                    {
                        splittedList = splittedList.Skip(splittedList.Length - maxPicovIncomingMessages).ToArray();
                    }

                    List<string[]> bufferDeviceResponses = new List<string[]>();

                    foreach (var line in splittedList)
                    {
                        bufferDeviceResponses.Add(line.Split(new char[] { InputCommands.CommandSeparator }, StringSplitOptions.RemoveEmptyEntries));
                    }

                    var deviceResponse = splittedList.Last();

                    var deviceResponseList = deviceResponse.Split(new char[] { InputCommands.CommandSeparator }, StringSplitOptions.RemoveEmptyEntries);

                    if (deviceResponseList != null && deviceResponseList.Length > 0)
                    {
                        foreach (var indicatorInfo in deviceResponseList)
                        {
                            var indicatorInfoList = indicatorInfo.Split(new char[] { InputCommands.CommandInnerSeparator }, StringSplitOptions.RemoveEmptyEntries);

                            if (indicatorInfoList != null && indicatorInfoList.Any() && indicatorInfoList.Length >= 2)
                            {
                                var nanoValue = new TerminalIndicator
                                {
                                    Id = indicatorInfoList[0].Trim(),
                                    Value = indicatorInfoList[1].Trim().Replace("&", "")
                                };

                                //in case the buffer returns multiple lines and at least one line contains a 1 value,
                                //assume the value to send has to be 1.

                                if (nanoValue.Id == "3" &&
                                    nanoValue.Value == "0" &&
                                    bufferDeviceResponses.Any(x => x.Contains("3-1")))
                                {
                                    nanoValue.Value = "1";
                                }

                                if (nanoValue.Id == "4" &&
                                    nanoValue.Value == "0" &&
                                    bufferDeviceResponses.Any(x => x.Contains("4-1")))
                                {
                                    nanoValue.Value = "1";
                                }

                                if (nanoValue.Id == "5" &&
                                    nanoValue.Value == "5" &&
                                   bufferDeviceResponses.Any(x => x.Contains("5-1")))
                                {
                                    nanoValue.Value = "1";
                                }

                                result.Add(indicatorInfoList[0], nanoValue);
                            }
                        }
                    }
                }

                return result;
            }
            else
            {
                return new Dictionary<string, TerminalIndicator>();
            }
        }

        #region State Setters
        private void VehiclePresenceNotDetected()
        {
            TerminalState = TerminalState.StandBy;
        }

        private void VehiclePresenceDetected()
        {
            TerminalState = TerminalState.VehiclePresent;
        }

        #endregion
    }
}