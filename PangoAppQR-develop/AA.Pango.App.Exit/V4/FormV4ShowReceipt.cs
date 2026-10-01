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
    public partial class FormV4ShowReceipt : Form
    {
        protected ILog _log = LogManager.GetLogger("FormV4ShowReceipt");
        CultureInfo deC = new CultureInfo("en-US");
        private static string _terminalId = ConfigurationManager.AppSettings["TerminalId"];
        private static string dateTimeFormat = ConfigurationManager.AppSettings["DateTimeFormat"];
        private static string screenForm = ConfigurationManager.AppSettings["ScreenForm"];
        private static string footVersion = ConfigurationManager.AppSettings["FootVersion"];
        private static bool amountZero = false;
        private static bool pangoPayment = false;

        private Thread Worker { get; set; }
        private System.Resources.ResourceManager RM = new System.Resources.ResourceManager("AA.Pango.App.Exit.Properties.Resources", typeof(Resources).Assembly);

        private CheckCCTransientResponse PaymentData { get; set; }

        public FormV4ShowReceipt()
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
                    this.lblPlate.Text = !String.IsNullOrEmpty(paymentAttempt.Plate) ? paymentAttempt.Plate : !String.IsNullOrEmpty(paymentAttempt.Phone) ? paymentAttempt.Phone : LanguageLocalizationParser.GetCurrentLanguageTags["FormV4ShowReceipt_LabelPlateDefault"];

                    if (screenForm == "V4")
                        FormV4.Controller.TerminalState = TerminalState.VehiclePresent;
                    else if (screenForm == "V4_3opt")
                        FormV4_3opt.Controller.TerminalState = TerminalState.VehiclePresent;
                    else if (screenForm == "V4_1opt")
                        FormV4_1opt.Controller.TerminalState = TerminalState.VehiclePresent;
                    else if (screenForm == "V4_Ticket_PH")
                        FormV4_Ticket_PH.Controller.TerminalState = TerminalState.VehiclePresent;
                }
            }
            else
            {
                _log.Debug("No pending payments to process.");
                CloseForm();
            }
        }

        public FormV4ShowReceipt(PaymentAttempt paymentAttempt)
        {
            InitializeComponent();

            if (paymentAttempt != null)
            {
                this.lblTotalPrice.Text = "$" + paymentAttempt.AmountToCharge.ToString("#0.00", CultureInfo.InvariantCulture);
                this.lblPlate.Text = !String.IsNullOrEmpty(paymentAttempt.Plate) ? paymentAttempt.Plate : !String.IsNullOrEmpty(paymentAttempt.Phone) ? paymentAttempt.Phone : LanguageLocalizationParser.GetCurrentLanguageTags["FormV4ShowReceipt_LabelPlateDefault"];
                if (!string.IsNullOrEmpty(paymentAttempt.PriceDetail))
                    this.lblPriceDetail.Text = LanguageLocalizationParser.GetCurrentLanguageTags[paymentAttempt.PriceDetail];

                if (screenForm == "V4")
                    FormV4.Controller.TerminalState = TerminalState.VehiclePresent;
                else if (screenForm == "V4_3opt")
                    FormV4_3opt.Controller.TerminalState = TerminalState.VehiclePresent;
                else if (screenForm == "V4_1opt")
                    FormV4_1opt.Controller.TerminalState = TerminalState.VehiclePresent;
                else if (screenForm == "V4_Ticket_PH")
                    FormV4_Ticket_PH.Controller.TerminalState = TerminalState.VehiclePresent;
            }
        }

        public FormV4ShowReceipt(CheckCCTransientResponse paymentData)
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

        public FormV4ShowReceipt(ChargePlateResponse paymentData)
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

        public FormV4ShowReceipt(TemplateChangeScreen changeScreenData)
        {
            _log.Debug("changeScreenData - id: " + changeScreenData._id + ", template: " + changeScreenData.TemplateId);

            InitializeComponent();
            _log.Debug("amount equal than zero");
            amountZero = true;

            ChangeScreenService.SetProcessedChangeScreen(changeScreenData._id);
        }

        public FormV4ShowReceipt(bool isPangoPaym)
        {
            _log.Debug("ShowReceipt - isPangoPaym: " + isPangoPaym);

            InitializeComponent();
            pangoPayment = true;
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
            //this.btnPrintReceiptPorSiAcaso.BackgroundImage = (Image)RM.GetObject(DataFileParser.Styles["ShowReceipt_PrintBtn"]);

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
            }
            else
            {
                this.tableLayoutFoot.Visible = false;
                this.tableLayoutFoot2.Visible = true;
                this.imgFootLogoPango2.BackgroundImage = (Image)RM.GetObject(DataFileParser.Styles["FootLogoPango"]);
                this.imgFootLogo2.BackgroundImage = (Image)RM.GetObject(DataFileParser.Styles["FootLogoV2"]);
                this.imgFootBtn24h.BackgroundImage = (Image)RM.GetObject(DataFileParser.Styles["FootBtn24h"]);
            }

            #endregion Style Edits

            this.lblHeader.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4ShowReceipt_Header"];
            this.lblFootDownload.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4_DownloadApp"];
            this.lblInstructionsCC.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4ShowReceipt_InstructionsCC"];
            this.lblInstructionsOK.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4ShowReceipt_InstructionsOK"];
            //this.lblInstructionsOKPorSiAcaso.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4ShowReceipt_InstructionsOK"];
            this.btnGoBack.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4_btnBack"];

            if (amountZero)
                this.lblStatusOK.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4ShowReceipt_PaymentFreeOK"];
            else
                this.lblStatusOK.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4ShowReceipt_PaymentOK"];

            //if (amountZero)
            //    this.lblStatusOKPorSiAcaso.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4ShowReceipt_PaymentFreeOK"];
            //else
            //    this.lblStatusOKPorSiAcaso.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4ShowReceipt_PaymentOK"];

            this.tlblTerminalId.Text = "Terminal: " + terminalId + ". " + descTerminalType;
            this.tlblDate.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            this.lblCurrentDate.Text = DateTime.Now.ToString(dateTimeFormat);
            this.lblFootDate2.Text = DateTime.Now.ToString(dateTimeFormat);

            this.tlblStatusTerminal.Text = "";
        }

        private void InitWorker()
        {
            _log.Debug("Starting Worker...");

            Worker = new Thread(new ThreadStart((() =>
            {
                try
                {
                    var startDate = DateTime.Now;
                    var lastControllerState = TerminalState.StandBy;
                    if (screenForm == "V4")
                        lastControllerState = FormV4.Controller.TerminalState;
                    else if (screenForm == "V4_3opt")
                        lastControllerState = FormV4_3opt.Controller.TerminalState;
                    else if (screenForm == "V4_1opt")
                        lastControllerState = FormV4_1opt.Controller.TerminalState;
                    else if (screenForm == "V4_Ticket_PH")
                        lastControllerState = FormV4_Ticket_PH.Controller.TerminalState;

                    while (true)
                    {
                        if (this.IsDisposed)
                        {
                            _log.Debug("exiting worker thread...");
                            break;
                        }

                        try
                        {
                            if (amountZero || pangoPayment)
                            {
                                amountZero = false;
                                pangoPayment = false;
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
                            else
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

                            #region Controller Actions
                            TerminalState terminalState = TerminalState.StandBy;
                            bool setTerminalState = false;
                            if (screenForm == "V4")
                            {
                                if (FormV4.Controller != null)
                                {
                                    terminalState = FormV4.Controller.TerminalState;
                                    setTerminalState = true;
                                }
                            }
                            else if (screenForm == "V4_3opt")
                            {
                                if (FormV4_3opt.Controller != null)
                                {
                                    terminalState = FormV4_3opt.Controller.TerminalState;
                                    setTerminalState = true;
                                }
                            }
                            else if (screenForm == "V4_1opt")
                            {
                                if (FormV4_1opt.Controller != null)
                                {
                                    terminalState = FormV4_1opt.Controller.TerminalState;
                                    setTerminalState = true;
                                }
                            }
                            else if (screenForm == "V4_Ticket_PH")
                            {
                                if (FormV4_Ticket_PH.Controller != null)
                                {
                                    terminalState = FormV4_Ticket_PH.Controller.TerminalState;
                                    setTerminalState = true;
                                }
                            }

                            if (setTerminalState)
                            {
                                _log.Debug("Reading controller state: " + terminalState.ToString());

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
                                            if (screenForm == "V4")
                                                paymStatusText = FormV4.Controller.TemplatesDictionary["PaymentInProgressInsertCard"].Value;
                                            else if (screenForm == "V4_3opt")
                                                paymStatusText = FormV4_3opt.Controller.TemplatesDictionary["PaymentInProgressInsertCard"].Value;
                                            else if (screenForm == "V4_1opt")
                                                paymStatusText = FormV4_1opt.Controller.TemplatesDictionary["PaymentInProgressInsertCard"].Value;
                                            else if (screenForm == "V4_Ticket_PH")
                                                paymStatusText = FormV4_Ticket_PH.Controller.TemplatesDictionary["PaymentInProgressInsertCard"].Value;
                                        }
                                        else if (TerminalController.PaymentProcessorService.CurrentReaderStatus == PangoApp.Payments.Interface.CurrentReaderStatus.CardInserted)
                                        {
                                            if (screenForm == "V4")
                                                paymStatusText = FormV4.Controller.TemplatesDictionary["PaymentInProgressReadingCard"].Value;
                                            else if (screenForm == "V4_3opt")
                                                paymStatusText = FormV4_3opt.Controller.TemplatesDictionary["PaymentInProgressReadingCard"].Value;
                                            else if (screenForm == "V4_1opt")
                                                paymStatusText = FormV4_1opt.Controller.TemplatesDictionary["PaymentInProgressReadingCard"].Value;
                                            else if (screenForm == "V4_Ticket_PH")
                                                paymStatusText = FormV4_Ticket_PH.Controller.TemplatesDictionary["PaymentInProgressReadingCard"].Value;
                                        }
                                        else if (TerminalController.PaymentProcessorService.CurrentReaderStatus == PangoApp.Payments.Interface.CurrentReaderStatus.RemoveCard)
                                        {
                                            if (screenForm == "V4")
                                                paymStatusText = FormV4.Controller.TemplatesDictionary["PaymentInProgressRemoveCard"].Value;
                                            else if (screenForm == "V4_3opt")
                                                paymStatusText = FormV4_3opt.Controller.TemplatesDictionary["PaymentInProgressRemoveCard"].Value;
                                            else if (screenForm == "V4_1opt")
                                                paymStatusText = FormV4_1opt.Controller.TemplatesDictionary["PaymentInProgressRemoveCard"].Value;
                                            else if (screenForm == "V4_Ticket_PH")
                                                paymStatusText = FormV4_Ticket_PH.Controller.TemplatesDictionary["PaymentInProgressRemoveCard"].Value;
                                        }
                                        else
                                        {
                                            if (screenForm == "V4")
                                                paymStatusText = FormV4.Controller.TemplatesDictionary["PaymentInProgress"].Value;
                                            else if (screenForm == "V4_3opt")
                                                paymStatusText = FormV4_3opt.Controller.TemplatesDictionary["PaymentInProgress"].Value;
                                            else if (screenForm == "V4_1opt")
                                                paymStatusText = FormV4_1opt.Controller.TemplatesDictionary["PaymentInProgress"].Value;
                                            else if (screenForm == "V4_Ticket_PH")
                                                paymStatusText = FormV4_Ticket_PH.Controller.TemplatesDictionary["PaymentInProgress"].Value;
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
                                        if (screenForm == "V4")
                                        {
                                            if (FormV4.Controller.PrintReceipt)
                                                lblPaymStatus.Text = FormV4.Controller.TemplatesDictionary["TakeReceipt"].Value;
                                            else
                                                lblPaymStatus.Text = "";
                                        }
                                        else if (screenForm == "V4_3opt")
                                        {
                                            if (FormV4_3opt.Controller.PrintReceipt)
                                                lblPaymStatus.Text = FormV4_3opt.Controller.TemplatesDictionary["TakeReceipt"].Value;
                                            else
                                                lblPaymStatus.Text = "";
                                        }
                                        else if (screenForm == "V4_1opt")
                                        {
                                            if (FormV4_1opt.Controller.PrintReceipt)
                                                lblPaymStatus.Text = FormV4_1opt.Controller.TemplatesDictionary["TakeReceipt"].Value;
                                            else
                                                lblPaymStatus.Text = "";
                                        }
                                        else if (screenForm == "V4_Ticket_PH")
                                        {
                                            if (FormV4_Ticket_PH.Controller.PrintReceipt)
                                                lblPaymStatus.Text = FormV4_Ticket_PH.Controller.TemplatesDictionary["TakeReceipt"].Value;
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
                                        if (screenForm == "V4")
                                            paymDeniedText = FormV4.Controller.TemplatesDictionary["PaymentDenied"].Value;
                                        else if (screenForm == "V4_3opt")
                                            paymDeniedText = FormV4_3opt.Controller.TemplatesDictionary["PaymentDenied"].Value;
                                        else if (screenForm == "V4_1opt")
                                            paymDeniedText = FormV4_1opt.Controller.TemplatesDictionary["PaymentDenied"].Value;
                                        else if (screenForm == "V4_Ticket_PH")
                                            paymDeniedText = FormV4_Ticket_PH.Controller.TemplatesDictionary["PaymentDenied"].Value;

                                        lblPaymStatus.Text = paymDeniedText;
                                    }));
                                }
                                else if (terminalState == TerminalState.PaymentFailed)
                                {
                                    lblPaymStatus.Invoke((MethodInvoker)(() =>
                                    {
                                        var paymFailText = "";
                                        if (screenForm == "V4")
                                            paymFailText = FormV4.Controller.TemplatesDictionary["PaymentFailed"].Value.ToUpper() + " - " + FormV4.Controller.TextToShowOnUI.ToUpper();
                                        else if (screenForm == "V4_3opt")
                                            paymFailText = FormV4_3opt.Controller.TemplatesDictionary["PaymentFailed"].Value.ToUpper() + " - " + FormV4_3opt.Controller.TextToShowOnUI.ToUpper();
                                        else if (screenForm == "V4_1opt")
                                            paymFailText = FormV4_1opt.Controller.TemplatesDictionary["PaymentFailed"].Value.ToUpper() + " - " + FormV4_1opt.Controller.TextToShowOnUI.ToUpper();
                                        else if (screenForm == "V4_Ticket_PH")
                                            paymFailText = FormV4_Ticket_PH.Controller.TemplatesDictionary["PaymentFailed"].Value.ToUpper() + " - " + FormV4_Ticket_PH.Controller.TextToShowOnUI.ToUpper();

                                        lblPaymStatus.Text = paymFailText;
                                    }));
                                }
                                else if (terminalState == TerminalState.PaymentFinalized)
                                {
                                    if (screenForm == "V4")
                                        FormV4.Controller.TextToShowOnUI = null;
                                    else if (screenForm == "V4_3opt")
                                        FormV4_3opt.Controller.TextToShowOnUI = null;
                                    else if (screenForm == "V4_1opt")
                                        FormV4_1opt.Controller.TextToShowOnUI = null;
                                    else if (screenForm == "V4_Ticket_PH")
                                        FormV4_Ticket_PH.Controller.TextToShowOnUI = null;

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
            if (screenForm == "V4")
                FormV4.Controller.PrintReceipt = true;
            else if (screenForm == "V4_3opt")
                FormV4_3opt.Controller.PrintReceipt = true;
            else if (screenForm == "V4_1opt")
                FormV4_1opt.Controller.PrintReceipt = true;
            else if (screenForm == "V4_Ticket_PH")
                FormV4_Ticket_PH.Controller.PrintReceipt = true;

            this.btnPrintReceipt.Enabled = false;
            this.btnPrintReceipt.BackgroundImage = (Image)RM.GetObject(DataFileParser.Styles["ShowReceipt_PrintBtnDisabled"]);
            //this.btnPrintReceipt.Text = "PRINTING RECEIPT...";
        }
    }
}