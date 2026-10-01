using AA.Pango.App.Exit.Properties;
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
    public partial class FormV3ShowReceipt : Form
    {
        protected ILog _log = LogManager.GetLogger("FormV3ShowReceipt");
        CultureInfo deC = new CultureInfo("en-US");
        private static string _terminalId = ConfigurationManager.AppSettings["TerminalId"];
        private static string dateTimeFormat = ConfigurationManager.AppSettings["DateTimeFormat"];
        private static string screenForm = ConfigurationManager.AppSettings["ScreenForm"];
        private static string footVersion = ConfigurationManager.AppSettings["FootVersion"];
        private static bool amountZero = false;

        private Thread Worker { get; set; }
        private System.Resources.ResourceManager RM = new System.Resources.ResourceManager("AA.Pango.App.Exit.Properties.Resources", typeof(Resources).Assembly);

        private CheckCCTransientResponse PaymentData { get; set; }

        public FormV3ShowReceipt()
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

                    this.lblTotalPrice.Text = "$" + paymentAttempt.AmountToCharge.ToString("#0.00", CultureInfo.InvariantCulture);
                    this.lblPlate.Text = paymentAttempt.Plate;

                    if (screenForm == "V3")
                        FormV3.Controller.TerminalState = TerminalState.VehiclePresent;
                    else if (screenForm == "V3_3opt")
                        FormV3_3opt.Controller.TerminalState = TerminalState.VehiclePresent;
                    else if (screenForm == "V3_1opt")
                        FormV3_1opt.Controller.TerminalState = TerminalState.VehiclePresent;
                }
            }
            else
            {
                _log.Debug("No pending payments to process.");
                CloseForm();
            }
        }

        public FormV3ShowReceipt(PaymentAttempt paymentAttempt)
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

                this.lblTotalPrice.Text = "$" + paymentAttempt.AmountToCharge.ToString("#0.00", CultureInfo.InvariantCulture);
                this.lblPlate.Text = !String.IsNullOrEmpty(paymentAttempt.Plate) ? paymentAttempt.Plate : paymentAttempt.Phone;

                if (screenForm == "V3")
                    FormV3.Controller.TerminalState = TerminalState.VehiclePresent;
                else if (screenForm == "V3_3opt")
                    FormV3_3opt.Controller.TerminalState = TerminalState.VehiclePresent;
                else if (screenForm == "V3_1opt")
                    FormV3_1opt.Controller.TerminalState = TerminalState.VehiclePresent;
            }
        }

        public FormV3ShowReceipt(CheckCCTransientResponse paymentData)
        {
            if (paymentData == null || !paymentData.amount.HasValue)
            {
                _log.Warn("No data was received");
                return;
            }

            if (paymentData.amount.Value <= 0M)
            {
                InitializeComponent();
                _log.Debug("amount equal or lower than zero");
                amountZero = true;
            }
        }

        public FormV3ShowReceipt(ChargePlateResponse paymentData)
        {
            if (paymentData == null || !paymentData.amount.HasValue)
            {
                _log.Warn("No data was received");
                return;
            }

            if (paymentData.amount.Value <= 0M)
            {
                InitializeComponent();
                _log.Debug("amount equal or lower than zero");
                amountZero = true;
            }
        }

        public FormV3ShowReceipt(TemplateChangeScreen changeScreenData)
        {
            _log.Debug("changeScreenData - id: " + changeScreenData._id + ", template: " + changeScreenData.TemplateId);

            InitializeComponent();
            _log.Debug("amount equal than zero");
            amountZero = true;

            ChangeScreenService.SetProcessedChangeScreen(changeScreenData._id);

            //Thread _thread = new Thread(() =>
            //{
            //    while (true)
            //    {
            //        Thread.Sleep(changeScreenData.SecondsToShowOnScreen * 1000);

            //        this.CloseForm();
            //    }
            //});

            //_thread.SetApartmentState(ApartmentState.STA);
            //_thread.Start();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Init();
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
            DataFileParser.LoadStyles("styles.dat");

            bool showTestInput = bool.Parse(ConfigurationManager.AppSettings["ShowTestInput"] ?? "false");
            int terminalId = int.Parse(ConfigurationManager.AppSettings["TerminalId"]);
            var eventNotificationTypeToSend = ConfigurationManager.AppSettings["EventNotificationTypeToSend"] ?? "Exit";

            var descTerminalType = eventNotificationTypeToSend == "Entry" ?
                LanguageLocalizationParser.GetCurrentLanguageTags["EntryTerminalDescription"] :
                LanguageLocalizationParser.GetCurrentLanguageTags["ExitTerminalDescription"];

            #region Style Edits
            this.BackColor = ColorTranslator.FromHtml(DataFileParser.Styles["BackgroundColor"]);

            var fontColor = ColorTranslator.FromHtml(DataFileParser.Styles["FontColor"]);
            this.ForeColor = fontColor;
            this.panelInit.ForeColor = fontColor;

            this.lblHeader.BackColor = ColorTranslator.FromHtml(DataFileParser.Styles["HeaderBackgroundColor"]);
            this.lblHeader.ForeColor = ColorTranslator.FromHtml(DataFileParser.Styles["HeaderFontColor"]);

            var receiptDetailFontColor = ColorTranslator.FromHtml(DataFileParser.Styles["ReceiptFontColor"]);
            this.lblTotalPrice.ForeColor = receiptDetailFontColor;
            this.lblPlate.ForeColor = receiptDetailFontColor;

            this.btnGoBack.BackColor = ColorTranslator.FromHtml(DataFileParser.Styles["BtnBackBackgroundColor"]);
            this.btnGoBack.ForeColor = ColorTranslator.FromHtml(DataFileParser.Styles["BtnBackFontColor"]);
            this.btnGoBack.FlatAppearance.BorderColor = ColorTranslator.FromHtml(DataFileParser.Styles["BtnBackBorderColor"]);

            this.panelFoot.BackColor = ColorTranslator.FromHtml(DataFileParser.Styles["FootBackgroundColor"]);
            this.panelFoot.ForeColor = ColorTranslator.FromHtml(DataFileParser.Styles["FootFontColor"]);

            this.imgCC.BackgroundImage = (Image)RM.GetObject(DataFileParser.Styles["ShowReceipt_CCImage"]);
            this.btnPrintReceipt.BackgroundImage = (Image)RM.GetObject(DataFileParser.Styles["ShowReceipt_PrintBtn"]);

            this.pictureBox1.BackgroundImage = (Image)RM.GetObject(DataFileParser.Styles["ImgDolar"]);
            this.pictureBox2.BackgroundImage = (Image)RM.GetObject(DataFileParser.Styles["ImgArrow"]);
            this.pictureBox3.BackgroundImage = (Image)RM.GetObject(DataFileParser.Styles["ImgCar"]);
            this.pictureBox4.BackgroundImage = (Image)RM.GetObject(DataFileParser.Styles["ImgArrow"]);
            this.pictureBox5.BackgroundImage = (Image)RM.GetObject(DataFileParser.Styles["ImgBarrier"]);

            if (footVersion == "1")
            {
                this.tableLayoutFoot.Visible = true;
                this.imgFootLogoPango.BackgroundImage = (Image)RM.GetObject(DataFileParser.Styles["FootLogoPango"]);
                this.imgFootYellowBtn.BackgroundImage = (Image)RM.GetObject(DataFileParser.Styles["FootBtn24h"]);
                this.tableLayoutFoot2.Visible = false;
                this.tableLayoutFoot3.Visible = false;
            }
            else if (footVersion == "2")
            {
                this.tableLayoutFoot.Visible = false;
                this.tableLayoutFoot2.Visible = true;
                this.imgFootLogoPango2.BackgroundImage = (Image)RM.GetObject(DataFileParser.Styles["FootLogoPango"]);
                this.imgFootLogo2.BackgroundImage = (Image)RM.GetObject(DataFileParser.Styles["FootLogoV2"]);
                this.imgFootBtn24h.BackgroundImage = (Image)RM.GetObject(DataFileParser.Styles["FootBtn24h"]);
                this.tableLayoutFoot3.Visible = false;
            }
            else
            {
                this.tableLayoutFoot.Visible = false;
                this.tableLayoutFoot2.Visible = false;
                this.tableLayoutFoot3.Visible = true;
                this.imgFootBtn24h3.BackgroundImage = (Image)RM.GetObject(DataFileParser.Styles["FootBtn24h"]);
            }
            #endregion Style Edits

            this.lblHeader.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV3ShowReceipt_Header"];
            this.lblFootDownload.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV3_DownloadApp"];
            this.lblInstructionsCC.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV3ShowReceipt_InstructionsCC"];
            this.lblInstructionsOK.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV3ShowReceipt_InstructionsOK"];
            this.btnGoBack.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV3_btnBack"];

            if (amountZero)
                this.lblStatusOK.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV3ShowReceipt_PaymentFreeOK"];
            else
                this.lblStatusOK.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV3ShowReceipt_PaymentOK"];

            this.tlblTerminalId.Text = "Terminal: " + terminalId + ". " + descTerminalType;
            this.tlblDate.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            this.lblCurrentDate.Text = DateTime.Now.ToString(dateTimeFormat);
            this.lblFootDate2.Text = DateTime.Now.ToString(dateTimeFormat);
            this.lblFootDate3.Text = DateTime.Now.ToString(dateTimeFormat);

            this.tlblStatusTerminal.Text = "";
        }

        private void InitWorker()
        {
            _log.Debug("Starting Worker...");

            Worker = new Thread(new ThreadStart((() =>
            {
                try
                {
                    var lastControllerState = TerminalState.StandBy;
                    if (screenForm == "V3")
                        lastControllerState = FormV3.Controller.TerminalState;
                    else if (screenForm == "V3_3opt")
                        lastControllerState = FormV3_3opt.Controller.TerminalState;
                    else if (screenForm == "V3_1opt")
                        lastControllerState = FormV3_1opt.Controller.TerminalState;

                    while (true)
                    {
                        if (this.IsDisposed)
                        {
                            _log.Debug("exiting worker thread...");
                            break;
                        }

                        try
                        {
                            if (amountZero)
                            {
                                amountZero = false;
                                panelOK.Invoke((MethodInvoker)(() =>
                                {
                                    panelOK.Visible = true;
                                    panelOK.BringToFront();
                                }));

                                Thread _thread = new Thread(() =>
                                {
                                    Thread.Sleep(5000);

                                    if (this.IsDisposed)
                                        return;

                                    CloseForm();
                                });
                                _thread.SetApartmentState(ApartmentState.STA);
                                _thread.Start();
                            }

                            if (!statusStrip1.IsDisposed)
                            {
                                if (statusStrip1.InvokeRequired)
                                {
                                    statusStrip1.Invoke((MethodInvoker)(() =>
                                    {
                                        statusStrip1.Items["tlblDate"].Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
                                    }));
                                }
                                else
                                {
                                    statusStrip1.Items["tlblDate"].Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
                                }
                            }

                            if (footVersion == "1")
                            {
                                if (!lblCurrentDate.IsDisposed)
                                {
                                    if (lblCurrentDate.InvokeRequired)
                                    {
                                        lblCurrentDate.Invoke((MethodInvoker)(() =>
                                        {
                                            lblCurrentDate.Text = DateTime.Now.ToString(dateTimeFormat, deC);
                                        }));
                                    }
                                    else
                                    {
                                        lblCurrentDate.Text = DateTime.Now.ToString(dateTimeFormat, deC);
                                    }
                                }
                            }
                            else if (footVersion == "2")
                            {
                                if (!lblFootDate2.IsDisposed)
                                {
                                    if (lblFootDate2.InvokeRequired)
                                    {
                                        lblFootDate2.Invoke((MethodInvoker)(() =>
                                        {
                                            lblFootDate2.Text = DateTime.Now.ToString(dateTimeFormat, deC);
                                        }));
                                    }
                                    else
                                        lblFootDate2.Text = DateTime.Now.ToString(dateTimeFormat, deC);
                                }
                            }
                            else
                            {
                                if (!lblFootDate3.IsDisposed)
                                {
                                    if (lblFootDate3.InvokeRequired)
                                    {
                                        lblFootDate3.Invoke((MethodInvoker)(() =>
                                        {
                                            lblFootDate3.Text = DateTime.Now.ToString(dateTimeFormat, deC);
                                        }));
                                    }
                                    else
                                        lblFootDate3.Text = DateTime.Now.ToString(dateTimeFormat, deC);
                                }
                            }

                            #region Controller Actions
                            TerminalState terminalState = TerminalState.StandBy;
                            bool setTerminalState = false;
                            if (screenForm == "V3")
                            {
                                if (FormV3.Controller != null)
                                {
                                    terminalState = FormV3.Controller.TerminalState;
                                    setTerminalState = true;
                                }
                            }
                            else if (screenForm == "V3_3opt")
                            {
                                if (FormV3_3opt.Controller != null)
                                {
                                    terminalState = FormV3_3opt.Controller.TerminalState;
                                    setTerminalState = true;
                                }
                            }
                            else if (screenForm == "V3_1opt")
                            {
                                if (FormV3_1opt.Controller != null)
                                {
                                    terminalState = FormV3_1opt.Controller.TerminalState;
                                    setTerminalState = true;
                                }
                            }

                            if (setTerminalState)
                            {
                                //_log.Debug("Reading controller state: " + terminalState.ToString());

                                if (terminalState == TerminalState.StandBy)
                                {
                                    lblPaymStatus.Invoke((MethodInvoker)(() =>
                                    {
                                        this.lblPaymStatus.Text = "";
                                    }));
                                }
                                else if (terminalState == TerminalState.VehiclePresent)
                                {
                                    lblPaymStatus.Invoke((MethodInvoker)(() =>
                                    {
                                        this.lblPaymStatus.Text = "";
                                    }));
                                }
                                else if (terminalState == TerminalState.PaymentInProgress)
                                {
                                    _log.Debug("CurrentReaderStatus: " + TerminalController.PaymentProcessorService.CurrentReaderStatus.ToString());

                                    lblPaymStatus.Invoke((MethodInvoker)(() =>
                                    {
                                        var paymStatusText = "";
                                        if (TerminalController.PaymentProcessorService.CurrentReaderStatus == PangoApp.Payments.Interface.CurrentReaderStatus.WaitingForCard)
                                        {
                                            if (screenForm == "V3")
                                                paymStatusText = FormV3.Controller.TemplatesDictionary["PaymentInProgressInsertCard"].Value;
                                            else if (screenForm == "V3_3opt")
                                                paymStatusText = FormV3_3opt.Controller.TemplatesDictionary["PaymentInProgressInsertCard"].Value;
                                            else if (screenForm == "V3_1opt")
                                                paymStatusText = FormV3_1opt.Controller.TemplatesDictionary["PaymentInProgressInsertCard"].Value;
                                        }
                                        else if (TerminalController.PaymentProcessorService.CurrentReaderStatus == PangoApp.Payments.Interface.CurrentReaderStatus.CardInserted)
                                        {
                                            if (screenForm == "V3")
                                                paymStatusText = FormV3.Controller.TemplatesDictionary["PaymentInProgressReadingCard"].Value;
                                            else if (screenForm == "V3_3opt")
                                                paymStatusText = FormV3_3opt.Controller.TemplatesDictionary["PaymentInProgressReadingCard"].Value;
                                            else if (screenForm == "V3_1opt")
                                                paymStatusText = FormV3_1opt.Controller.TemplatesDictionary["PaymentInProgressReadingCard"].Value;
                                        }
                                        else if (TerminalController.PaymentProcessorService.CurrentReaderStatus == PangoApp.Payments.Interface.CurrentReaderStatus.RemoveCard)
                                        {
                                            if (screenForm == "V3")
                                                paymStatusText = FormV3.Controller.TemplatesDictionary["PaymentInProgressRemoveCard"].Value;
                                            else if (screenForm == "V3_3opt")
                                                paymStatusText = FormV3_3opt.Controller.TemplatesDictionary["PaymentInProgressRemoveCard"].Value;
                                            else if (screenForm == "V3_1opt")
                                                paymStatusText = FormV3_1opt.Controller.TemplatesDictionary["PaymentInProgressRemoveCard"].Value;
                                        }
                                        else
                                        {
                                            if (screenForm == "V3")
                                                paymStatusText = FormV3.Controller.TemplatesDictionary["PaymentInProgress"].Value;
                                            else if (screenForm == "V3_3opt")
                                                paymStatusText = FormV3_3opt.Controller.TemplatesDictionary["PaymentInProgress"].Value;
                                            else if (screenForm == "V3_1opt")
                                                paymStatusText = FormV3_1opt.Controller.TemplatesDictionary["PaymentInProgress"].Value;
                                        }
                                    
                                        lblPaymStatus.Text = paymStatusText;
                                    }));
                                }
                                else if (terminalState == TerminalState.PaymentApproved)
                                {
                                    panelOK.Invoke((MethodInvoker)(() =>
                                    {
                                        panelOK.Visible = true;
                                        panelOK.BringToFront();
                                    }));

                                    btnPrintReceipt.Invoke((MethodInvoker)(() =>
                                    {
                                        btnPrintReceipt.Visible = true;
                                    }));

                                    lblPaymStatus.Invoke((MethodInvoker)(() =>
                                    {
                                        if (screenForm == "V3")
                                        {
                                            if (FormV3.Controller.PrintReceipt)
                                                lblPaymStatus.Text = FormV3.Controller.TemplatesDictionary["TakeReceipt"].Value;
                                            else
                                                lblPaymStatus.Text = "";
                                        }
                                        else if (screenForm == "V3_3opt")
                                        {
                                            if (FormV3_3opt.Controller.PrintReceipt)
                                                lblPaymStatus.Text = FormV3_3opt.Controller.TemplatesDictionary["TakeReceipt"].Value;
                                            else
                                                lblPaymStatus.Text = "";
                                        }
                                        else if (screenForm == "V3_1opt")
                                        {
                                            if (FormV3_1opt.Controller.PrintReceipt)
                                                lblPaymStatus.Text = FormV3_1opt.Controller.TemplatesDictionary["TakeReceipt"].Value;
                                            else
                                                lblPaymStatus.Text = "";
                                        }
                                    }));
                                }
                                else if (terminalState == TerminalState.PaymentDenied)
                                {
                                    lblPaymStatus.Invoke((MethodInvoker)(() =>
                                    {
                                        var paymDeniedText = "";
                                        if (screenForm == "V3")
                                            paymDeniedText = FormV3.Controller.TemplatesDictionary["PaymentDenied"].Value;
                                        else if (screenForm == "V3_3opt")
                                            paymDeniedText = FormV3_3opt.Controller.TemplatesDictionary["PaymentDenied"].Value;
                                        else if (screenForm == "V3_1opt")
                                            paymDeniedText = FormV3_1opt.Controller.TemplatesDictionary["PaymentDenied"].Value;

                                        lblPaymStatus.Text = paymDeniedText;
                                    }));
                                }
                                else if (terminalState == TerminalState.PaymentFailed)
                                {
                                    lblPaymStatus.Invoke((MethodInvoker)(() =>
                                    {
                                        var paymFailText = "";
                                        if (screenForm == "V3")
                                            paymFailText = FormV3.Controller.TemplatesDictionary["PaymentFailed"].Value + " - " + FormV3.Controller.TextToShowOnUI;
                                        else if (screenForm == "V3_3opt")
                                            paymFailText = FormV3_3opt.Controller.TemplatesDictionary["PaymentFailed"].Value + " - " + FormV3_3opt.Controller.TextToShowOnUI;
                                        else if (screenForm == "V3_1opt")
                                            paymFailText = FormV3_1opt.Controller.TemplatesDictionary["PaymentFailed"].Value + " - " + FormV3_1opt.Controller.TextToShowOnUI;

                                        lblPaymStatus.Text = paymFailText;
                                    }));
                                }
                                else if (terminalState == TerminalState.PaymentFinalized)
                                {
                                    if (screenForm == "V3")
                                        FormV3.Controller.TextToShowOnUI = null;
                                    else if (screenForm == "V3_3opt")
                                        FormV3_3opt.Controller.TextToShowOnUI = null;
                                    else if (screenForm == "V3_1opt")
                                        FormV3_1opt.Controller.TextToShowOnUI = null;

                                    _log.Debug("Closing form...");
                                    CloseForm();
                                }

                                if (!statusStrip1.IsDisposed)
                                {
                                    if (statusStrip1.InvokeRequired)
                                    {
                                        statusStrip1.Invoke((MethodInvoker)(() =>
                                        {
                                            statusStrip1.Items["tlblStatusTerminal"].Text = terminalState.ToString();
                                        }));
                                    }
                                    else
                                    {
                                        statusStrip1.Items["tlblStatusTerminal"].Text = terminalState.ToString();
                                    }
                                }
                            }
                            #endregion Controller Actions
                        }
                        catch (Exception e)
                        {
                            _log.Error("Exception occured in worker thread", e);
                        }
                        finally
                        {
                            Thread.Sleep(1000);
                        }
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

        private void btnGoBack_Click(object sender, EventArgs e)
        {
            _log.Debug("GoBack");
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
            if (screenForm == "V3")
                FormV3.Controller.PrintReceipt = true;
            else if (screenForm == "V3_3opt")
                FormV3_3opt.Controller.PrintReceipt = true;
            else if (screenForm == "V3_1opt")
                FormV3_1opt.Controller.PrintReceipt = true;

            this.btnPrintReceipt.Enabled = false;
            //this.btnPrintReceipt.Text = "PRINTING RECEIPT...";
        }
    }
}