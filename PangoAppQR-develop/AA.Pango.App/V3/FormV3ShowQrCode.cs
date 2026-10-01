using AA.Pango.App.Properties;
using AA.Pango.ServiceLayer.Controllers;
using AA.Pango.TestWinApp.Utils;
using AA.Pango.TestWinApp;
using log4net;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using AA.Pango.ServiceLayer.ApiDtos;
using AA.Pango.ServiceLayer;

namespace AA.Pango.App
{
    public partial class FormV3ShowQrCode : Form
    {
        protected ILog _log = LogManager.GetLogger("FormV3ShowQrCode");
        CultureInfo deC = new CultureInfo("en-US");
        private int showQrCodeFormTimeout = int.Parse(ConfigurationManager.AppSettings["ShowQrCodeFormTimeout"] ?? "10000");
        private int showQrCodeKOTimeout = int.Parse(ConfigurationManager.AppSettings["ShowQrCodeKOTimeout"] ?? "30000");
        private int showQrCodeOKTimeout = int.Parse(ConfigurationManager.AppSettings["ShowQrCodeOKTimeout"] ?? "2000");
        private static string dateTimeFormat = ConfigurationManager.AppSettings["DateTimeFormat"];
        private static string screenForm = ConfigurationManager.AppSettings["ScreenForm"];
        private static bool showPlateOrPhone = bool.Parse(ConfigurationManager.AppSettings["ShowPlateOrPhone"]);
        private static string footVersion = ConfigurationManager.AppSettings["FootVersion"];

        public bool Hidden { get; set; }

        private Thread Worker { get; set; }
        private System.Resources.ResourceManager RM = new System.Resources.ResourceManager("AA.Pango.App.Properties.Resources", typeof(Resources).Assembly);

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
                this.Close();
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
            var eventNotificationTypeToSend = ConfigurationManager.AppSettings["EventNotificationTypeToSend"] ?? "Entry";

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

            this.imgEntryQR.BackgroundImage = (Image)RM.GetObject(DataFileParser.Styles["ImgQR"]);
            this.imgEntryPlate.BackgroundImage = (Image)RM.GetObject(DataFileParser.Styles["ImgPlate"]);
            this.imgEntryPhone.BackgroundImage = (Image)RM.GetObject(DataFileParser.Styles["ImgPhone"]);
            this.pictureBox2.BackgroundImage = (Image)RM.GetObject(DataFileParser.Styles["ImgArrow"]);
            this.pictureBox3.BackgroundImage = (Image)RM.GetObject(DataFileParser.Styles["ImgCar"]);
            this.pictureBox4.BackgroundImage = (Image)RM.GetObject(DataFileParser.Styles["ImgArrow"]);
            this.pictureBox5.BackgroundImage = (Image)RM.GetObject(DataFileParser.Styles["ImgBarrier"]);

            if (footVersion == "1")
            {
                this.tableLayoutFoot.Visible = true;
                this.imgFootLogoPango.BackgroundImage = (Image)RM.GetObject(DataFileParser.Styles["FootLogoPango"]);
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
            }
            #endregion Style Edits

            this.lblHeader.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV3_Welcome"];
            this.lblInstructions.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV3ShowQRCode_ShowQR"];
            this.lblInstructionsQR.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV3ShowQRCode_InstructionsQR"];
            this.lblStatusOK.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV3ShowQRCode_CodeOK"];
            this.lblInstructionsOK.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV3ShowQRCode_InstructionsOK"];
            this.lblStatusKO.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV3ShowQRCode_CodeKO"];
            this.lblInstructionsKO.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV3ShowQRCode_InstructionsKO"];
            //this.plateOrPhone1..btnPlate.Text = LanguageLocalizationParser.GetCurrentLanguageTags["PlateOrPhone_Plate"];
            //this.plateOrPhone1.btnPhone.Text = LanguageLocalizationParser.GetCurrentLanguageTags["PlateOrPhone_Phone"];
            this.lblFootDownload.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV3_DownloadApp"];
            this.lblFootPrice.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV3_Price"];
            this.lblFootPrice3.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV3_Price"];
            this.btnGoBack.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV3_btnBack"];

            this.tlblTerminalId.Text = "Terminal: " + terminalId + ". " + descTerminalType;
            this.tlblDate.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            this.lblCurrentDate.Text = DateTime.Now.ToString(dateTimeFormat);
            this.lblFootDate2.Text = DateTime.Now.ToString(dateTimeFormat);
            this.lblFootDate3.Text = DateTime.Now.ToString(dateTimeFormat);
            this.tlblStatusTerminal.Text = "";

            _log.Debug("Starting Controller...");
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
                            if (FormV3.EntryController != null)
                            {
                                terminalState = FormV3.EntryController.TerminalState;
                                setTerminalState = true;
                            }
                        }
                        else if (screenForm == "V3_QRTicket")
                        {
                            if (FormV3_QRTicket.EntryController != null)
                            {
                                terminalState = FormV3_QRTicket.EntryController.TerminalState;
                                setTerminalState = true;
                            }
                        }
                        else if (screenForm == "V3_3opt")
                        {
                            if (FormV3_3opt.EntryController != null)
                            {
                                terminalState = FormV3_3opt.EntryController.TerminalState;
                                setTerminalState = true;
                            }
                        }
                        else if (screenForm == "V3_1opt")
                        {
                            if (FormV3_1opt.EntryController != null)
                            {
                                terminalState = FormV3_1opt.EntryController.TerminalState;
                                setTerminalState = true;
                            }
                        }
                        if (setTerminalState && lblStatus.InvokeRequired && !lblStatus.IsDisposed)
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
                                        if (showPlateOrPhone)
                                        {
                                            panelKO.Visible = true;
                                            panelKO.BringToFront();
                                        }
                                        else
                                        {
                                            openPhoneForm();
                                        }
                                    }
                                }));
                                Thread.Sleep(showQrCodeKOTimeout);
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
                                Thread.Sleep(showQrCodeOKTimeout);
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
                        CloseForm();
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
            {
                this.lblStatusOK.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV3ShowQRCode_StatusKO_PlateOrPhone"];
                if (!panelOK.IsDisposed)
                {
                    if (this.panelOK.InvokeRequired)
                    {
                        this.panelOK.Invoke((MethodInvoker)(() =>
                        {
                            this.panelOK.Visible = true;
                            this.panelOK.BringToFront();
                        }));
                    }
                    else
                    {
                        this.panelOK.Visible = true;
                        this.panelOK.BringToFront();
                    }
                }
                if (e.Type == 1)
                    this.imgEntryPlate.BringToFront();
                else
                    this.imgEntryPhone.BringToFront();
            }
            else
            {
                if (!panelOK.IsDisposed)
                {
                    if (this.panelOK.InvokeRequired)
                    {
                        this.panelOK.Invoke((MethodInvoker)(() =>
                        {
                            this.panelOK.Visible = false;
                            this.panelOK.SendToBack();
                        }));
                    }
                    else
                    {
                        this.panelOK.Visible = false;
                        this.panelOK.SendToBack();
                    }
                }
            }
        }

        private void openPhoneForm()
        {
            var form = new ScreenDigitKeyboardV3(1, true, LanguageLocalizationParser.GetCurrentLanguageTags["FormV3ShowQRCode_CodeKOPhone"]);
            var dialogResult = form.ShowDialog(this);

            if (dialogResult == DialogResult.OK)
            {
                if (string.IsNullOrEmpty(form.Phone))
                    return;

                if (form.Phone.Length != 10)
                {
                    _log.Debug("Invalid Phone");
                    //labelError.Text = "Invalid Phone";
                    return;
                }

                var (phoneRequestSent, phoneResponse) = PangoService.TransientPlateOrPhone(new TransientPlateOrPhoneRequest
                {
                    installationID = ConfigurationManager.AppSettings["InstallationId"],
                    terminalId = ConfigurationManager.AppSettings["TerminalId"],
                    phone = form.Phone
                });

                _log.Debug("Phone request sent: " + (phoneRequestSent ? "OK" : "NOT OK"));

                if (phoneRequestSent)
                {
                    _log.Debug("status received: " + phoneResponse.status);

                    if (phoneResponse.status == 1)
                    {
                        c_Registered(this, new StatusEventArgs
                        {
                            Type = 2,
                            Status = "OK"
                        });
                    }
                    else
                    {
                        //labelError.Text = "Error Try Again...";
                        _log.Error("error message: " + phoneResponse.message);
                        c_Registered(this, new StatusEventArgs
                        {
                            Type = 2,
                            Status = "KO"
                        });
                    }
                }
            }
            else
            {
                _log.Warn("No phone was read from the user.");
            }
        }

        private void Form1_FormClosing_1(object sender, FormClosingEventArgs e)
        {
            this.Hide();
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            bool showTestInput = bool.Parse(ConfigurationManager.AppSettings["ShowTestInput"] ?? "false");
            if (e.KeyChar == 13)
            {
                if (screenForm == "V3")
                {
                    FormV3.EntryController.ReadInputValue = true;
                    if (showTestInput)
                    {
                        FormV3.EntryController.InputValue = this.txtInputCode.Text;
                    }
                    else
                    {
                        _log.Debug(FormV3.EntryController.InputValue + " ready for querying");
                    }
                }  
                else if (screenForm == "V3_QRTicket")
                {
                    FormV3_QRTicket.EntryController.ReadInputValue = true;
                    if (showTestInput)
                        FormV3_QRTicket.EntryController.InputValue = this.txtInputCode.Text;
                    else
                        _log.Debug(FormV3_QRTicket.EntryController.InputValue + " ready for querying");
                }
                else if (screenForm == "V3_3opt")
                {
                    FormV3_3opt.EntryController.ReadInputValue = true;
                    if (showTestInput)
                        FormV3_3opt.EntryController.InputValue = this.txtInputCode.Text;
                    else
                        _log.Debug(FormV3_3opt.EntryController.InputValue + " ready for querying");
                }
                else if (screenForm == "V3_1opt")
                {
                    FormV3_1opt.EntryController.ReadInputValue = true;
                    if (showTestInput)
                        FormV3_1opt.EntryController.InputValue = this.txtInputCode.Text;
                    else
                        _log.Debug(FormV3_1opt.EntryController.InputValue + " ready for querying");
                }
            }
            else
            {
                if (screenForm == "V3")
                {
                    if (FormV3.EntryController.InputValue == null)
                    {
                        FormV3.EntryController.InputValue = "";
                    }

                    FormV3.EntryController.InputValue += e.KeyChar;
                }
                else if (screenForm == "V3_QRTicket")
                {
                    if (FormV3_QRTicket.EntryController.InputValue == null)
                        FormV3_QRTicket.EntryController.InputValue = "";

                    FormV3_QRTicket.EntryController.InputValue += e.KeyChar;
                }
                else if (screenForm == "V3_3opt")
                {
                    if (FormV3_3opt.EntryController.InputValue == null)
                        FormV3_3opt.EntryController.InputValue = "";

                    FormV3_3opt.EntryController.InputValue += e.KeyChar;
                }
                else if (screenForm == "V3_1opt")
                {
                    if (FormV3_1opt.EntryController.InputValue == null)
                        FormV3_1opt.EntryController.InputValue = "";

                    FormV3_1opt.EntryController.InputValue += e.KeyChar;
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
            this.Hidden = true;
            this.Hide();
        }
    }
}