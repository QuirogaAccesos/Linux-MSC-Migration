using AA.Pango.App.Exit.Properties;
using AA.Pango.ServiceLayer;
using AA.Pango.ServiceLayer.ApiDtos;
using AA.Pango.ServiceLayer.Controllers;
using AA.Pango.TestWinApp;
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
    public partial class FormV3ShowQrCode : Form
    {
        protected ILog _log = LogManager.GetLogger("FormV3ShowQrCode");
        CultureInfo deC = new CultureInfo("en-US");
        private int showQrCodeFormTimeout = int.Parse(ConfigurationManager.AppSettings["ShowQrCodeFormTimeout"] ?? "10000");
        private int showQrCodeKOFormTimeout = int.Parse(ConfigurationManager.AppSettings["ShowQrCodeKOTimeout"] ?? "30000");
        private int showQrResultStatusTimeout = int.Parse(ConfigurationManager.AppSettings["ShowQrCodeOKTimeout"] ?? "2000");
        private static string dateTimeFormat = ConfigurationManager.AppSettings["DateTimeFormat"];
        private static string screenForm = ConfigurationManager.AppSettings["ScreenForm"];
        private static string footVersion = ConfigurationManager.AppSettings["FootVersion"];

        private Thread Worker { get; set; }
        private System.Resources.ResourceManager RM = new System.Resources.ResourceManager("AA.Pango.App.Exit.Properties.Resources", typeof(Resources).Assembly);

        public FormV3ShowQrCode()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.plateOrPhone1.Registered += c_Registered;
            this.Init();

            Thread _thread = new Thread(() =>
            {
                Thread.Sleep(showQrCodeFormTimeout);

                if (this.IsDisposed)
                {
                    return;
                }

                this.DialogResult = DialogResult.OK;
                this.CloseForm();
                //this.Dispose();
            });
            _thread.SetApartmentState(ApartmentState.STA);
            _thread.Start();
        }

        private void Init()
        {
            InitSettings();
            SetVersion();
            InitWorker();
        }

        private void SetVersion()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            string driverVersion = fvi.FileVersion;

            var version = "Versión: " + driverVersion;

            statusStrip1.Items["tlblVersion"].Text = version;
        }

        private void InitSettings()
        {
            _log.Debug("init settings...");

            //this.TopMost = true;
            this.Activate();

            this.txtInputCode.Focus();

            LanguageLocalizationParser.LoadLocalizations();
            DataFileParser.LoadStyles("styles.dat");

            bool showTestInput = bool.Parse(ConfigurationManager.AppSettings["ShowTestInput"] ?? "false");
            int terminalId = int.Parse(ConfigurationManager.AppSettings["TerminalId"]);
            var eventNotificationTypeToSend = ConfigurationManager.AppSettings["EventNotificationTypeToSend"] ?? "Exit";

            if (showTestInput)
            {
                this.txtInputCode.BringToFront();
            }
            else
            {
                this.txtInputCode.SendToBack();
            }

            var descTerminalType = eventNotificationTypeToSend == "Entry" ?
                LanguageLocalizationParser.GetCurrentLanguageTags["EntryTerminalDescription"] :
                LanguageLocalizationParser.GetCurrentLanguageTags["ExitTerminalDescription"];

            #region Style Edits
            var backColor = ColorTranslator.FromHtml(DataFileParser.Styles["BackgroundColor"]);
            this.BackColor = backColor;
            this.plateOrPhone1.BackColor = backColor;

            var fontColor = ColorTranslator.FromHtml(DataFileParser.Styles["FontColor"]);
            this.ForeColor = fontColor;
            this.lblInstructionsQR.ForeColor = fontColor;

            Color headerBackColor = ColorTranslator.FromHtml(DataFileParser.Styles["HeaderBackgroundColor"]);
            this.lblHeader.BackColor = headerBackColor;
            this.lblInstructions.BackColor = headerBackColor;

            Color fontHeaderColor = ColorTranslator.FromHtml(DataFileParser.Styles["HeaderFontColor"]);
            this.lblHeader.ForeColor = fontHeaderColor;
            this.lblInstructions.ForeColor = fontHeaderColor;

            this.btnGoBack.BackColor = ColorTranslator.FromHtml(DataFileParser.Styles["BtnBackBackgroundColor"]);
            this.btnGoBack.ForeColor = ColorTranslator.FromHtml(DataFileParser.Styles["BtnBackFontColor"]);
            this.btnGoBack.FlatAppearance.BorderColor = ColorTranslator.FromHtml(DataFileParser.Styles["BtnBackBorderColor"]);

            this.panelFoot.BackColor = ColorTranslator.FromHtml(DataFileParser.Styles["FootBackgroundColor"]);
            this.panelFoot.ForeColor = ColorTranslator.FromHtml(DataFileParser.Styles["FootFontColor"]);

            this.imgShowQR.BackgroundImage = (Image)RM.GetObject(DataFileParser.Styles["ShowQRCode"]);

            this.pictureBox1.BackgroundImage = (Image)RM.GetObject(DataFileParser.Styles["ImgQR"]);
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

            this.lblHeader.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV3_Welcome"];
            this.lblInstructions.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV3ShowQRCode_ShowQR"];
            this.lblInstructionsQR.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV3ShowQRCode_InstructionsQR"];
            this.lblStatusOK.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV3ShowQRCode_CodeOK"];
            this.lblInstructionsOK.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV3ShowQRCode_InstructionsOK"];
            this.lblStatusKO.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV3ShowQRCode_CodeKO"];
            this.lblInstructionsKO.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV3ShowQRCode_InstructionsKO"];
            this.lblFootDownload.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV3_DownloadApp"];
            this.btnGoBack.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV3_btnBack"];
            //this.plateOrPhone1..btnPlate.Text = LanguageLocalizationParser.GetCurrentLanguageTags["PlateOrPhone_Plate"];
            //this.plateOrPhone1.btnPhone.Text = LanguageLocalizationParser.GetCurrentLanguageTags["PlateOrPhone_Phone"];

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
                while (true)
                {
                    if (this.IsDisposed)
                    {
                        _log.Debug("exiting thread...");

                        break;
                    }
                    if (!this.IsHandleCreated)
                    {
                        break;
                    }

                    try
                    {
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

                        #region Entry Controller Actions
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
                        else if (screenForm == "V3_QRTicket")
                        {
                            if (FormV3_QRTicket.Controller != null)
                            {
                                terminalState = FormV3_QRTicket.Controller.TerminalState;
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
                        if (setTerminalState && lblStatus.InvokeRequired)
                        {
                            if (terminalState == TerminalState.ReadingBarcode)
                            {
                                lblStatus.Invoke((MethodInvoker)(() =>
                                {
                                    lblStatus.Visible = true;
                                    lblStatus.Text = LanguageLocalizationParser.GetCurrentLanguageTags["Entry-IssuingVisitorCard"];
                                }));
                            }
                            else if (terminalState == TerminalState.VehiclePresent)
                            {
                                lblStatus.Invoke((MethodInvoker)(() =>
                                {
                                    lblStatus.Visible = true;
                                    lblStatus.Text = LanguageLocalizationParser.GetCurrentLanguageTags["Entry-IssuingVisitorCard"];
                                }));
                            }
                            else if (terminalState == TerminalState.TicketDenied || terminalState == TerminalState.TicketNotFound)
                            {
                                lblStatus.Invoke((MethodInvoker)(() =>
                                {
                                    lblStatus.Visible = true;
                                    lblStatus.Text = LanguageLocalizationParser.GetCurrentLanguageTags["Entry-RecurrentClientCardDenied"];
                                    if (screenForm != "V3_QRTicket")
                                    {
                                        panelKO.Visible = true;
                                        panelKO.BringToFront();
                                    }
                                }));
                                Thread.Sleep(showQrCodeKOFormTimeout);
                            }
                            else if (terminalState == TerminalState.StandBy)
                            {
                                lblStatus.Invoke((MethodInvoker)(() =>
                                {
                                    lblStatus.Visible = true;
                                    DateTime timeOfDayGreeting = DateTime.Now;
                                    lblStatus.Text = LanguageLocalizationParser.GetCurrentLanguageTags["Hello"];

                                    if (timeOfDayGreeting.Hour >= 5 && timeOfDayGreeting.Hour < 12)
                                    {
                                        lblStatus.Text = LanguageLocalizationParser.GetCurrentLanguageTags["Morning"]; //Console.WriteLine("Good morning!");
                                    }
                                    else if (timeOfDayGreeting.Hour >= 12 && timeOfDayGreeting.Hour < 17)
                                    {
                                        lblStatus.Text = LanguageLocalizationParser.GetCurrentLanguageTags["Afternoon"]; //Console.WriteLine("Good afternoon!");
                                    }
                                    else
                                    {
                                        lblStatus.Text = LanguageLocalizationParser.GetCurrentLanguageTags["Night"]; //Console.WriteLine("Good night!");
                                    }
                                }));
                            }
                            else if (terminalState == TerminalState.TicketApproved)
                            {
                                lblStatus.Invoke((MethodInvoker)(() =>
                                {
                                    lblStatus.Visible = true;
                                    lblStatus.Text = LanguageLocalizationParser.GetCurrentLanguageTags["Entry-EntryGranted"];
                                    panelOK.Visible = true;
                                    panelOK.BringToFront();
                                }));

                                Thread.Sleep(showQrResultStatusTimeout);
                            }
                            else if (terminalState == TerminalState.OutOfService)
                            {
                                lblStatus.Invoke((MethodInvoker)(() =>
                                {
                                    lblStatus.Visible = true;
                                    lblStatus.Text = LanguageLocalizationParser.GetCurrentLanguageTags["Entry-OutOfService"];
                                }));
                            }

                            if (!statusStrip1.IsDisposed)
                            {
                                statusStrip1.Items["tlblStatusTerminal"].Text = terminalState.ToString();
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
                    }
                    catch (Exception e)
                    {
                        _log.Error("Unexpected error in Worker thread", e);
                        this.CloseForm();
                    }
                    finally
                    {
                        Thread.Sleep(200);
                    }
                    #endregion Entry Controller Actions
                }
            })));

            Worker.Start();
        }

        public void c_Registered(object sender, StatusEventArgs e)
        {
            _log.Debug("The " + (e.Type == 1 ? "Plate" : "Phone") + " was registered: " + e.Status);
            if (e.Status == "OK")
                processChargePlateResponse(e.chargeResponse);
        }

        private void Form1_FormClosing_1(object sender, FormClosingEventArgs e)
        {
            _log.Debug("Hiding form...");
            this.Hide();
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            bool showTestInput = bool.Parse(ConfigurationManager.AppSettings["ShowTestInput"] ?? "false");
            if (e.KeyChar == 13)
            {
                if (screenForm == "V3")
                {
                    FormV3.Controller.ReadInputValue = true;
                    if (showTestInput)
                    {
                        FormV3.Controller.InputValue = this.txtInputCode.Text;
                    }
                    else
                    {
                        _log.Debug(FormV3.Controller.InputValue + " ready for querying");
                    }
                }
                else if (screenForm == "V3_QRTicket")
                {
                    FormV3_QRTicket.Controller.ReadInputValue = true;
                    if (showTestInput)
                        FormV3_QRTicket.Controller.InputValue = this.txtInputCode.Text;
                    else
                        _log.Debug(FormV3_QRTicket.Controller.InputValue + " ready for querying");
                }
                else if (screenForm == "V3_3opt")
                {
                    FormV3_3opt.Controller.ReadInputValue = true;
                    if (showTestInput)
                        FormV3_3opt.Controller.InputValue = this.txtInputCode.Text;
                    else
                        _log.Debug(FormV3_3opt.Controller.InputValue + " ready for querying");
                }
                else if (screenForm == "V3_1opt")
                {
                    FormV3_1opt.Controller.ReadInputValue = true;
                    if (showTestInput)
                        FormV3_1opt.Controller.InputValue = this.txtInputCode.Text;
                    else
                        _log.Debug(FormV3_1opt.Controller.InputValue + " ready for querying");
                }
            }
            else
            {
                if (screenForm == "V3")
                {
                    if (FormV3.Controller.InputValue == null)
                    {
                        FormV3.Controller.InputValue = "";
                    }

                    FormV3.Controller.InputValue += e.KeyChar;
                }
                else if (screenForm == "V3_QRTicket")
                {
                    if (FormV3_QRTicket.Controller.InputValue == null)
                        FormV3_QRTicket.Controller.InputValue = "";

                    FormV3_QRTicket.Controller.InputValue += e.KeyChar;
                }
                else if (screenForm == "V3_3opt")
                {
                    if (FormV3_3opt.Controller.InputValue == null)
                        FormV3_3opt.Controller.InputValue = "";

                    FormV3_3opt.Controller.InputValue += e.KeyChar;
                }
                else if (screenForm == "V3_1opt")
                {
                    if (FormV3_1opt.Controller.InputValue == null)
                        FormV3_1opt.Controller.InputValue = "";

                    FormV3_1opt.Controller.InputValue += e.KeyChar;
                }
            }
        }

        private void txtInputCode_Leave(object sender, EventArgs e)
        {
            this.txtInputCode.Focus();
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

        private void btnGoBack_Click(object sender, EventArgs e)
        {
            _log.Debug("GoBack");
            this.Hide();
        }

        private void processChargePlateResponse(ChargePlateResponse paymentData)
        {
            if (paymentData != null && paymentData.paramList.Any() && paymentData.amount.HasValue)
            {
                if (paymentData.amount.Value > 0M)
                {
                    var parkingFee = paymentData.paramList.FirstOrDefault(x => x.param_name == "parkingFee")?.param_value ?? "";
                    var convFee = paymentData.paramList.FirstOrDefault(x => x.param_name == "convFee")?.param_value ?? "";
                    var entryDate = paymentData.paramList.FirstOrDefault(x => x.param_name == "entryDate")?.param_value ?? "";
                    var entryTime = paymentData.paramList.FirstOrDefault(x => x.param_name == "entryTime")?.param_value ?? "";
                    var convFeePct = paymentData.paramList.FirstOrDefault(x => x.param_name == "convFeePercent")?.param_value ?? "";
                    var showTime = int.Parse(paymentData.paramList.FirstOrDefault(x => x.param_name == "showTime")?.param_value ?? "40");

                    PaymentService.UpsertPaymentAttempt(new Model.PaymentAttempt
                    {
                        AmountToCharge = paymentData.amount.Value,
                        Currency = "USD",
                        InstallationId = paymentData.installationId,
                        TerminalId = paymentData.terminalId,
                        Date = DateTime.Now,
                        Plate = paymentData.plate,
                        Phone = paymentData.phone,
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
                else
                {
                    var formV3 = new FormV3ShowReceipt(paymentData);
                    var dialogResult = formV3.ShowDialog(this);

                    if (dialogResult == DialogResult.Cancel)
                    {
                        panelKO.Visible = false;
                        panelKO.SendToBack();
                    }
                }
            }
        }
    }
}