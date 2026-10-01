using AA.Pango.Model;
using AA.Pango.ServiceLayer;
using AA.Pango.ServiceLayer.ApiDtos;
using AA.Pango.ServiceLayer.Controllers;
using AA.Pango.TestWinApp.Utils;
using log4net;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace AA.Pango.App
{
    public partial class FormV2ShowReceipt : Form
    {
        protected ILog _log = LogManager.GetLogger("FormV2PullTicket");
        CultureInfo deC = new CultureInfo("en-US");
        private static string _terminalId = ConfigurationManager.AppSettings["TerminalId"];

        private Thread Worker { get; set; }

        private CheckCCTransientResponse PaymentData { get; set; }

        public FormV2ShowReceipt()
        {
            InitializeComponent();

            var lastPendingPayments = PaymentService.GetPendingPaymentAttemptsWithinTime(_terminalId);

            if (lastPendingPayments?.Any() == true)
            {

                var paymentAttempt = lastPendingPayments.FirstOrDefault();

                if (paymentAttempt != null)
                {
                    var textToDisplay = $"Entry Date: {paymentAttempt.EntryDate}\r\n" +
                    $"Entry Time: {paymentAttempt.EntryTime}\r\n\r\n" +
                    $"Please Pay:\r\n" +
                    $"Amount: ${paymentAttempt.ParkingFee}\r\n" +
                    $"{paymentAttempt.TransactionPercentage}% Transaction Fee: ${paymentAttempt.TransactionFee}\r\n" +
                    $"License Plate: {paymentAttempt.Plate}\r\n\r\n" +
                    $"Total Amount: ${paymentAttempt.AmountToCharge.ToString("#0.00", CultureInfo.InvariantCulture)}";

                    this.lblInstructions.Text = textToDisplay;

                    FormV2.Controller.TerminalState = TerminalState.VehiclePresent;
                }


            }
            else
            {
                _log.Debug("No pending payments to process.");
                CloseForm();
            }

        }

        public FormV2ShowReceipt(PaymentAttempt paymentAttempt)
        {
            InitializeComponent();


            if (paymentAttempt != null)
            {
                var textToDisplay = $"Entry Date: {paymentAttempt.EntryDate}\r\n" +
                $"Entry Time: {paymentAttempt.EntryTime}\r\n\r\n" +
                $"Please Pay:\r\n" +
                $"Amount: ${paymentAttempt.ParkingFee}\r\n" +
                $"{paymentAttempt.TransactionPercentage}% Transaction Fee: ${paymentAttempt.TransactionFee}\r\n" +
                $"License Plate: {paymentAttempt.Plate}\r\n\r\n" +
                $"Total Amount: ${paymentAttempt.AmountToCharge.ToString("#0.00", CultureInfo.InvariantCulture)}";

                this.lblInstructions.Text = textToDisplay;

                FormV2.Controller.TerminalState = TerminalState.VehiclePresent;
            }

        }

        public FormV2ShowReceipt(CheckCCTransientResponse paymentData)
        {
            InitializeComponent();

            try
            {
                PaymentData = paymentData;

                if (PaymentData == null)
                {
                    _log.Debug("payment data is null");
                    CloseForm();
                    return;
                }

                if (!PaymentData.amount.HasValue)
                {
                    _log.Debug("amount is null");
                    CloseForm();
                    return;
                }

                if (PaymentData.amount.Value <= 0M)
                {
                    _log.Debug("amount equal or lower than zero");
                    CloseForm();
                    return;
                }

                if (!paymentData.paramList.Any())
                {
                    _log.Debug("param list is null");
                    CloseForm();
                    return;
                }

                var parkingFee = paymentData.paramList.FirstOrDefault(x => x.param_name == "parkingFee")?.param_value ?? "";
                var convFee = paymentData.paramList.FirstOrDefault(x => x.param_name == "convFee")?.param_value ?? "";
                var entryDate = paymentData.paramList.FirstOrDefault(x => x.param_name == "entryDate")?.param_value ?? "";
                var entryTime = paymentData.paramList.FirstOrDefault(x => x.param_name == "entryTime")?.param_value ?? "";
                var convFeePct = paymentData.paramList.FirstOrDefault(x => x.param_name == "convFeePercent")?.param_value ?? "";

                var showTime = int.Parse(paymentData.paramList.FirstOrDefault(x => x.param_name == "showTime")?.param_value ?? "40");

                var textToDisplay = $"Entry Date: {entryDate}\r\n" +
                    $"Entry Time: {entryTime}\r\n\r\n" +
                    $"Please Pay:\r\n" +
                    $"Amount: ${parkingFee}\r\n" +
                    $"{convFeePct}% Transaction Fee: ${convFee}\r\n" +
                    $"License Plate: {paymentData.plate}\r\n\r\n" +
                    $"Total Amount: ${paymentData.amount.Value.ToString("#0.00", CultureInfo.InvariantCulture)}";

                this.lblInstructions.Text = textToDisplay;
                this.btnPrintReceipt.Text = "PRINT RECEIPT";

                FormV2.Controller.TerminalState = TerminalState.VehiclePresent;

                PaymentService.UpsertPaymentAttempt(new Model.PaymentAttempt
                {
                    AmountToCharge = paymentData.amount.Value,
                    Currency = "USD",
                    InstallationId = paymentData.installationID,
                    TerminalId = paymentData.terminalId,
                    Date = DateTime.Now,
                    Plate = paymentData.plate,
                    Processed = false,
                    TransientId = paymentData.transientId,
                    EntryDate = entryDate,
                    EntryTime = entryTime,
                    ParkingFee = parkingFee,
                    ShowTime = showTime,
                    TransactionFee = convFee,
                    TransactionPercentage = convFeePct
                });
            }
            catch (Exception e)
            {
                _log.Warn(e);
                CloseForm();
            }

        }

        public FormV2ShowReceipt(ChargePlateResponse paymentData)
        {
            InitializeComponent();

            if (paymentData == null)
            {
                _log.Warn("No data was received");
                CloseForm();
                return;
            }

            if (!paymentData.amount.HasValue)
            {
                _log.Warn("No amount was received");
                CloseForm();
                return;
            }

            if (!paymentData.paramList.Any())
            {
                _log.Warn("No params were received from the api");
                CloseForm();
                return;
            }

            var parkingFee = paymentData.paramList.FirstOrDefault(x => x.param_name == "parkingFee")?.param_value ?? "";
            var convFee = paymentData.paramList.FirstOrDefault(x => x.param_name == "convFee")?.param_value ?? "";
            var entryDate = paymentData.paramList.FirstOrDefault(x => x.param_name == "entryDate")?.param_value ?? "";
            var entryTime = paymentData.paramList.FirstOrDefault(x => x.param_name == "entryTime")?.param_value ?? "";
            var convFeePct = paymentData.paramList.FirstOrDefault(x => x.param_name == "convFeePercent")?.param_value ?? "";

            var showTime = int.Parse(paymentData.paramList.FirstOrDefault(x => x.param_name == "showTime")?.param_value ?? "40");

            var textToDisplay = $"Entry Date: {entryDate}\r\n" +
                $"Entry Time: {entryTime}\r\n\r\n" +
                $"Please Pay:\r\n" +
                $"Amount: ${parkingFee}\r\n" +
                $"16% Transaction Fee: ${convFee}\r\n" +
                $"License Plate: {paymentData.plate}\r\n\r\n" +
                $"Total Amount: ${paymentData.amount.Value.ToString("#0.00", CultureInfo.InvariantCulture)}";

            this.lblInstructions.Text = textToDisplay;

            FormV2.Controller.TerminalState = TerminalState.VehiclePresent;

            PaymentService.UpsertPaymentAttempt(new Model.PaymentAttempt
            {
                AmountToCharge = paymentData.amount.Value,
                Currency = "USD",
                InstallationId = paymentData.installationId,
                TerminalId = paymentData.terminalId,
                Date = DateTime.Now,
                Plate = paymentData.plate,
                Processed = false,
                TransientId = paymentData.transientId,
                EntryDate = entryDate,
                EntryTime = entryTime,
                ParkingFee = parkingFee,
                ShowTime = showTime,
                TransactionFee = convFee,
                TransactionPercentage = convFeePct
            });

        }

        private void InitWorker()
        {
            _log.Debug("Starting Worker...");

            Worker = new Thread(new ThreadStart((() =>
            {

                try
                {

                    var startDate = DateTime.Now;
                    var lastControllerState = FormV2.Controller.TerminalState;

                    while (true)
                    {
                        if (this.IsDisposed)
                        {
                            _log.Debug("exiting worker thread...");
                            break;
                        }

                        lastControllerState = FormV2.Controller.TerminalState;

                        if (!statusStrip1.IsDisposed)
                        {
                            if (statusStrip1.InvokeRequired)
                            {
                                statusStrip1.Invoke((MethodInvoker)(() =>
                                {
                                    statusStrip1.Items["tlblDate"].Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                                }));
                            }
                            else
                            {
                                statusStrip1.Items["tlblDate"].Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                            }
                        }

                        if (lblCurrentDate.InvokeRequired)
                        {
                            lblCurrentDate.Invoke((MethodInvoker)(() =>
                            {
                                lblCurrentDate.Text = DateTime.Now.ToString("dddd, MMMM dd, yyyy HH:mm:ss", deC);
                            }));
                        }
                        else
                        {
                            lblCurrentDate.Text = DateTime.Now.ToString("dddd, MMMM dd, yyyy HH:mm:ss", deC);
                        }

                        #region Controller Actions

                        if (FormV2.Controller != null)
                        {

                            _log.Debug("Reading controller state: " + FormV2.Controller.TerminalState.ToString());

                            if (FormV2.Controller.TerminalState == TerminalState.StandBy)
                            {

                                lblPrintedReceipt.Invoke((MethodInvoker)(() =>
                                {
                                    this.lblPrintedReceipt.Visible = false;
                                }));

                                btnPrintReceipt.Invoke((MethodInvoker)(() =>
                                {
                                    this.btnPrintReceipt.Visible = false;
                                }));

                            }
                            else if ((FormV2.Controller.TerminalState == TerminalState.VehiclePresent))
                            {
                                lblPrintedReceipt.Invoke((MethodInvoker)(() =>
                                {
                                    this.lblPrintedReceipt.Visible = false;
                                }));
                            }
                            else if ((FormV2.Controller.TerminalState == TerminalState.PaymentInProgress))
                            {

                                _log.Debug("CurrentReaderStatus: " + TerminalController.PaymentProcessorService.CurrentReaderStatus.ToString());

                                lblPrintedReceipt.Invoke((MethodInvoker)(() =>
                                {

                                    if (TerminalController.PaymentProcessorService.CurrentReaderStatus ==
                                        PangoApp.Payments.Interface.CurrentReaderStatus.WaitingForCard)
                                    {

                                        this.lblPrintedReceipt.Visible = true;
                                        lblPrintedReceipt.Text = FormV2.Controller.TemplatesDictionary["PaymentInProgressInsertCard"].Value;

                                    }
                                    else if (TerminalController.PaymentProcessorService.CurrentReaderStatus ==
                                        PangoApp.Payments.Interface.CurrentReaderStatus.CardInserted)
                                    {
                                        this.lblPrintedReceipt.Visible = true;
                                        lblPrintedReceipt.Text = FormV2.Controller.TemplatesDictionary["PaymentInProgressReadingCard"].Value;
                                    }
                                    else if (TerminalController.PaymentProcessorService.CurrentReaderStatus ==
                                        PangoApp.Payments.Interface.CurrentReaderStatus.RemoveCard)
                                    {
                                        this.lblPrintedReceipt.Visible = true;
                                        lblPrintedReceipt.Text = FormV2.Controller.TemplatesDictionary["PaymentInProgressRemoveCard"].Value;
                                    }
                                    else
                                    {
                                        this.lblPrintedReceipt.Visible = true;
                                        lblPrintedReceipt.Text = FormV2.Controller.TemplatesDictionary["PaymentInProgress"].Value;
                                    }

                                }));
                            }
                            else if ((FormV2.Controller.TerminalState == TerminalState.PaymentApproved))
                            {
                                lblInstructions.Invoke((MethodInvoker)(() =>
                                {
                                    lblInstructions.Visible = true;
                                    lblInstructions.Text = FormV2.Controller.TemplatesDictionary["PaymentApproved"].Value;
                                }));

                                btnPrintReceipt.Invoke((MethodInvoker)(() =>
                                {
                                    btnPrintReceipt.Visible = true;
                                }));

                                lblPrintedReceipt.Invoke((MethodInvoker)(() =>
                                {
                                    lblPrintedReceipt.Visible = true;
                                    if (FormV2.Controller.PrintReceipt)
                                    {
                                        lblPrintedReceipt.Text = FormV2.Controller.TemplatesDictionary["TakeReceipt"].Value;
                                    }
                                    else
                                    {
                                        lblPrintedReceipt.Text = "";
                                    }

                                }));

                            }
                            else if ((FormV2.Controller.TerminalState == TerminalState.PaymentDenied))
                            {
                                lblInstructions.Invoke((MethodInvoker)(() =>
                                {
                                    lblInstructions.Visible = true;
                                    lblInstructions.Text = FormV2.Controller.TemplatesDictionary["PaymentDenied"].Value;
                                }));

                                lblPrintedReceipt.Invoke((MethodInvoker)(() =>
                                {
                                    lblPrintedReceipt.Visible = false;
                                }));

                            }
                            else if ((FormV2.Controller.TerminalState == TerminalState.PaymentFailed))
                            {
                                lblInstructions.Invoke((MethodInvoker)(() =>
                                {
                                    lblInstructions.Visible = true;
                                    lblInstructions.Text = FormV2.Controller.TemplatesDictionary["PaymentFailed"].Value + " - " + FormV2.Controller.TextToShowOnUI;
                                }));

                                lblPrintedReceipt.Invoke((MethodInvoker)(() =>
                                {
                                    lblPrintedReceipt.Visible = false;
                                }));

                            }
                            else if (FormV2.Controller.TerminalState == TerminalState.PaymentFinalized)
                            {
                                FormV2.Controller.TextToShowOnUI = null;

                                _log.Debug("Closing form...");
                                CloseForm();
                            }

                            if (!statusStrip1.IsDisposed)
                            {
                                if (statusStrip1.InvokeRequired)
                                {
                                    statusStrip1.Invoke((MethodInvoker)(() =>
                                    {
                                        statusStrip1.Items["tlblStatusTerminal"].Text = FormV2.Controller.TerminalState.ToString();
                                    }));
                                }
                                else
                                {
                                    statusStrip1.Items["tlblStatusTerminal"].Text = FormV2.Controller.TerminalState.ToString();
                                }
                            }

                        }

                        #endregion Controller Actions

                        Thread.Sleep(500);
                    }


                }
                catch (Exception e)
                {
                    _log.Error("Exception occured in worker thread", e);
                }
            })));
            Worker.SetApartmentState(ApartmentState.STA);
            Worker.Start();
        }

        private void Init()
        {
            _log.Debug("Initializing Form...");
            InitSettings();
            SetVersion();
            InitWorker();
        }

        private void SetVersion()
        {

            _log.Debug("Setting Version Number...");

            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            string driverVersion = fvi.FileVersion;

            var version = "Versión: " + driverVersion;

            statusStrip1.Items["tlblVersion"].Text = version;
        }

        private void InitSettings()
        {
            _log.Debug("init settings...");

            LanguageLocalizationParser.LoadLocalizations();

            bool showTestInput = bool.Parse(ConfigurationManager.AppSettings["ShowTestInput"] ?? "false");
            int terminalId = int.Parse(ConfigurationManager.AppSettings["TerminalId"]);
            var eventNotificationTypeToSend = ConfigurationManager.AppSettings["EventNotificationTypeToSend"] ?? "Entry";


            var descTerminalType = eventNotificationTypeToSend == "Entry" ?
                LanguageLocalizationParser.GetCurrentLanguageTags["EntryTerminalDescription"] :
                LanguageLocalizationParser.GetCurrentLanguageTags["ExitTerminalDescription"];

            #region Style Edits

            var backgroundColor = LanguageLocalizationParser.GetCurrentLanguageTags["BackgroundColor"] ?? "Cyan";
            var statusFontSize = float.Parse(LanguageLocalizationParser.GetCurrentLanguageTags["FontSize"] ?? "16");
            var fontFamily = LanguageLocalizationParser.GetCurrentLanguageTags["FontFamily"] ?? "Arial";
            var fontColor = LanguageLocalizationParser.GetCurrentLanguageTags["FontColor"] ?? "Black";

            Color color = System.Drawing.ColorTranslator.FromHtml(backgroundColor);
            this.BackColor = color;

            #endregion Style Edits

            this.tlblTerminalId.Text = "Terminal: " + terminalId + ". " + descTerminalType;
            this.tlblDate.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            this.lblCurrentDate.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

            this.tlblStatusTerminal.Text = "";
            var readerComPort = ConfigurationManager.AppSettings["ReaderComPort"];
            var readerBauds = int.Parse(ConfigurationManager.AppSettings["ReaderBauds"] ?? "9600");

            //_log.Debug("Starting Controller...");

            //Controller = PaymentTerminalController.GetInstance(terminalId);
            //Controller.Start();

        }


        private void pictureBox1_Click(object sender, EventArgs e)
        {
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Init();
        }

        private void btnGoBack_Click(object sender, EventArgs e)
        {
            CloseForm();
        }

        private void CloseForm()
        {
            try
            {
                if (this.IsDisposed)
                {
                    return;
                }

                if (this.InvokeRequired)
                {
                    this.Invoke((MethodInvoker)(() =>
                    {
                        this.Close();
                        this.Dispose();
                    }));
                }
                else
                {
                    this.Close();
                    this.Dispose();
                }
            }
            catch (Exception e)
            {
                _log.Warn(e);
            }
        }

        private void btnPrintReceipt_Click(object sender, EventArgs e)
        {
            FormV2.Controller.PrintReceipt = true;
            this.btnPrintReceipt.Enabled = false;
            this.btnPrintReceipt.Text = "PRINTING RECEIPT...";
        }
    }
}