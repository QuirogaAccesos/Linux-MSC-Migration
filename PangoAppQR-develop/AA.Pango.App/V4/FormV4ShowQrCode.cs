using AA.Pango.App.Properties;
using AA.Pango.ServiceLayer;
using AA.Pango.ServiceLayer.Controllers;
using AA.Pango.TestWinApp;
using AA.Pango.TestWinApp.Utils;
using log4net;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;

namespace AA.Pango.App
{
    public partial class FormV4ShowQrCode : Form
    {
        protected ILog _log = LogManager.GetLogger("FormV4ShowQrCode");
        CultureInfo deC = new CultureInfo("en-US");
        private int showQrCodeKOTimeout = int.Parse(ConfigurationManager.AppSettings["ShowQrCodeKOTimeout"] ?? "30000");
        private int showQrCodeOKTimeout = int.Parse(ConfigurationManager.AppSettings["ShowQrCodeOKTimeout"] ?? "2000");
        private int showQrCodePangoPassTimeout = int.Parse(ConfigurationManager.AppSettings["ShowQrCodePangoPassKOTimeout"] ?? "2000");
        private static string dateTimeFormat = ConfigurationManager.AppSettings["DateTimeFormat"];
        private static string screenForm = ConfigurationManager.AppSettings["ScreenForm"];
        private static string numOpt = ConfigurationManager.AppSettings["PlateOrPhoneOptions"];
        private static string footVersion = ConfigurationManager.AppSettings["FootVersion"];
        private static string ticketType = ConfigurationManager.AppSettings["TicketType"];
        private static bool showBtnPrintTicket = bool.Parse(ConfigurationManager.AppSettings["ShowBtnPrintTicketPango"]);

        public bool Hidden { get; set; }

        private Thread Worker { get; set; }

        private System.Resources.ResourceManager RM = new System.Resources.ResourceManager("AA.Pango.App.Properties.Resources", typeof(Resources).Assembly);

        public FormV4ShowQrCode()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.plateOrPhone1.Registered += c_Registered;
            this.Init();
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
            this.BackColor = ColorTranslator.FromHtml(DataFileParser.Styles["BackgroundColor"]);

            var fontColor = ColorTranslator.FromHtml(DataFileParser.Styles["FontColor"]);
            this.ForeColor = fontColor;
            this.btnPrintTicket.ForeColor = fontColor;

            Color headerBackColor = ColorTranslator.FromHtml(DataFileParser.Styles["HeaderBackgroundColor"]);
            this.lblHeader.BackColor = headerBackColor;
            this.lblInstructions.BackColor = headerBackColor;

            Color fontHeaderColor = ColorTranslator.FromHtml(DataFileParser.Styles["HeaderFontColor"]);
            this.lblHeader.ForeColor = fontHeaderColor;
            this.lblInstructions.ForeColor = fontHeaderColor;

            this.btnGoBack.BackColor = ColorTranslator.FromHtml(DataFileParser.Styles["BtnBackBackgroundColor"]);
            this.btnGoBack.ForeColor = ColorTranslator.FromHtml(DataFileParser.Styles["BtnBackFontColor"]);
            this.btnGoBack.FlatAppearance.BorderColor = ColorTranslator.FromHtml(DataFileParser.Styles["BtnBackBorderColor"]);

            this.btnPrintTicket.BackColor = ColorTranslator.FromHtml(DataFileParser.Styles["BtnPrintTicketBackgroundColor"]);
            this.btnPrintTicket.ForeColor = ColorTranslator.FromHtml(DataFileParser.Styles["BtnPrintTicketFontColor"]);

            this.panelFoot.BackColor = ColorTranslator.FromHtml(DataFileParser.Styles["FootBackgroundColor"]);
            this.panelFoot.ForeColor = ColorTranslator.FromHtml(DataFileParser.Styles["FootFontColor"]);

            this.imgShowQR.BackgroundImage = (Image)RM.GetObject(DataFileParser.Styles["ShowQRCode"]);

            this.imgEntryQR.BackgroundImage = (Image)RM.GetObject(DataFileParser.Styles["ImgQR"]);
            this.imgEntryPlate.BackgroundImage = (Image)RM.GetObject(DataFileParser.Styles["ImgPlate"]);
            this.imgEntryPhone.BackgroundImage = (Image)RM.GetObject(DataFileParser.Styles["ImgPhone"]);
            this.imgEntryTicket.BackgroundImage = (Image)RM.GetObject(DataFileParser.Styles["ImgTicket"]);
            this.pictureBox2.BackgroundImage = (Image)RM.GetObject(DataFileParser.Styles["ImgArrow"]);
            this.pictureBox3.BackgroundImage = (Image)RM.GetObject(DataFileParser.Styles["ImgCar"]);
            this.pictureBox4.BackgroundImage = (Image)RM.GetObject(DataFileParser.Styles["ImgArrow"]);
            this.pictureBox5.BackgroundImage = (Image)RM.GetObject(DataFileParser.Styles["ImgBarrier"]);

            //this.btnPrintTicketPorSiAcaso.BackgroundImage = (Image)RM.GetObject(DataFileParser.Styles["BtnPrintTicket"]);

            if (footVersion == "1")
            {
                this.tableLayoutFoot.Visible = true;
                this.imgFootLogoPango.BackgroundImage = (Image)RM.GetObject(DataFileParser.Styles["FootLogoPango"]);
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

            if (ticketType != "PANGO" || !showBtnPrintTicket)
            {
                this.btnPrintTicket.Visible = false;
                this.btnPrintTicket.Enabled = false;
                //this.btnPrintTicketPorSiAcaso.Visible = false;
                //this.btnPrintTicketPorSiAcaso.Enabled = false;
            }
            #endregion Style Edits

            this.lblHeader.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4_Welcome"];
            this.lblInstructions.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4ShowQRCode_ShowQR"];
            this.lblInstructionsQR.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4ShowQRCode_InstructionsQR"];
            this.lblStatusOK.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4ShowQRCode_CodeOK"];
            this.lblInstructionsOK.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4ShowQRCode_InstructionsOK"];
            //this.lblStatusOKPorSiAcaso.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4ShowQRCode_CodeOK"];
            //this.lblInstructionsOKPorSiAcaso.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4ShowQRCode_InstructionsOK"];
            this.lblStatusKO.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4ShowQRCode_CodeKO"];
            if (numOpt == "1" || numOpt == "1B")
                this.lblInstructionsKO.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4ShowQRCode_InstructionsKO_1opt"];
            else if (numOpt == "2")
                this.lblInstructionsKO.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4ShowQRCode_InstructionsKO_2opt"];
            else if (numOpt == "3")
                this.lblInstructionsKO.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4ShowQRCode_InstructionsKO_3opt"];
            //this.plateOrPhone1..btnPlate.Text = LanguageLocalizationParser.GetCurrentLanguageTags["PlateOrPhone_Plate"];
            //this.plateOrPhone1.btnPhone.Text = LanguageLocalizationParser.GetCurrentLanguageTags["PlateOrPhone_Phone"];
            this.lblStatusPangoPassError.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4ShowQRCode_PangoPassError"];
            this.lblFootDownload.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4_DownloadApp"];
            this.lblFootPrice.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4_Price"];
            this.btnGoBack.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4_btnBack"];
            this.btnPrintTicket.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4_btnPrintTicket"];

            this.tlblTerminalId.Text = "Terminal: " + terminalId + ". " + descTerminalType;
            this.tlblDate.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            this.lblCurrentDate.Text = DateTime.Now.ToString(dateTimeFormat);
            this.lblFootDate2.Text = DateTime.Now.ToString(dateTimeFormat);
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

                        #region Entry Controller Actions

                        TerminalState terminalState = TerminalState.StandBy;
                        string pangoPassError = "";
                        bool setTerminalState = false;
                        if (screenForm == "V4")
                        {
                            if (FormV4.EntryController != null)
                            {
                                terminalState = FormV4.EntryController.TerminalState;
                                setTerminalState = true;
                                pangoPassError = FormV4.EntryController.PangoPassError;
                            }
                        }
                        else if (screenForm == "V4_QRTicket")
                        {
                            if (FormV4_QRTicket.EntryController != null)
                            {
                                terminalState = FormV4_QRTicket.EntryController.TerminalState;
                                setTerminalState = true;
                                pangoPassError = FormV4_QRTicket.EntryController.PangoPassError;
                            }
                        }
                        else if (screenForm == "V4_3opt")
                        {
                            if (FormV4_3opt.EntryController != null)
                            {
                                terminalState = FormV4_3opt.EntryController.TerminalState;
                                setTerminalState = true;
                                pangoPassError = FormV4_3opt.EntryController.PangoPassError;
                            }
                        }
                        else if (screenForm == "V4_1opt")
                        {
                            if (FormV4_1opt.EntryController != null)
                            {
                                terminalState = FormV4_1opt.EntryController.TerminalState;
                                setTerminalState = true;
                                pangoPassError = FormV4_1opt.EntryController.PangoPassError;
                            }
                        }
                        else if (screenForm == "V4_Ticket_PH")
                        {
                            if (FormV4_Ticket_PH.EntryController != null)
                            {
                                terminalState = FormV4_Ticket_PH.EntryController.TerminalState;
                                setTerminalState = true;
                                pangoPassError = FormV4_Ticket_PH.EntryController.PangoPassError;
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
                                    if (screenForm != "V4_QRTicket")
                                    {
                                        panelKO.Visible = true;
                                        panelKO.BringToFront();
                                    }
                                }));

                                Thread _thread = new Thread(() =>
                                {
                                    Thread.Sleep(showQrCodeKOTimeout);

                                    if (this.IsDisposed)
                                        return;

                                    CloseForm();
                                });
                                _thread.SetApartmentState(ApartmentState.STA);
                                _thread.Start();

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

                                Thread _thread = new Thread(() =>
                                {
                                    Thread.Sleep(showQrCodeOKTimeout);

                                    if (this.IsDisposed)
                                        return;

                                    CloseForm();
                                });
                                _thread.SetApartmentState(ApartmentState.STA);
                                _thread.Start();

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
                            else if (terminalState == TerminalState.PangoPassError)
                            {
                                panelPangoPassError.Invoke((MethodInvoker)(() =>
                                {
                                    this.lblPangoPassErrorMsg.Text = pangoPassError;
                                    panelPangoPassError.Visible = true;
                                    panelPangoPassError.BringToFront();
                                }));

                                Thread _thread = new Thread(() =>
                                {
                                    Thread.Sleep(showQrCodePangoPassTimeout);

                                    if (this.IsDisposed)
                                        return;

                                    CloseForm();
                                });
                                _thread.SetApartmentState(ApartmentState.STA);
                                _thread.Start();

                                Thread.Sleep(showQrCodePangoPassTimeout);
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
            _log.Debug("The " + (e.Type == 1 ? "Plate" : e.Type == 2 ? "Phone" : "Ticket") + " was registered: " + e.Status);

            if (e.Status == "OK")
            {
                this.lblStatusOK.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4ShowQRCode_StatusKO_PlateOrPhone"];
                this.lblInstructionsOK.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4ShowQRCode_InstructionsOK"];
                
                if (e.Type == 1)
                {
                    this.imgEntryPlate.BringToFront();
                    if (ticketType == "PANGO" && showBtnPrintTicket)
                        this.btnPrintTicket.Visible = true;
                    this.txtTransientId.Text = e.TransientId.ToString();
                    this.txtTicketQR.Text = e.TicketQR;
                } 
                else if (e.Type == 2)
                {
                    this.imgEntryPhone.BringToFront();
                    if (ticketType == "PANGO" && showBtnPrintTicket)
                        this.btnPrintTicket.Visible = true;
                    this.txtTransientId.Text = e.TransientId.ToString();
                    this.txtTicketQR.Text = e.TicketQR;
                }
                else if (e.Type == 3)
                {
                    this.imgEntryTicket.BringToFront();
                    this.lblInstructionsOK.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4ShowQRCode_InstructionsOK_Ticket"];
                }

                this.panelOK.Visible = true;
                this.panelOK.BringToFront();
            }
            else
            {
                this.btnPrintTicket.Visible = false;
                this.txtTransientId.Text = "";
                this.txtTicketQR.Text = "";
                this.panelOK.Visible = false;
                this.panelOK.SendToBack();
            }
        }

        private void Form1_FormClosing_1(object sender, FormClosingEventArgs e)
        {
            this.btnPrintTicket.Visible = false;
            this.txtTransientId.Text = "";
            this.txtTicketQR.Text = "";
            this.Hide();
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            bool showTestInput = bool.Parse(ConfigurationManager.AppSettings["ShowTestInput"] ?? "false");
            if (e.KeyChar == 13)
            {
                if (screenForm == "V4")
                {
                    FormV4.EntryController.ReadInputValue = true;
                    if (showTestInput)
                    {
                        FormV4.EntryController.InputValue = this.txtInputCode.Text;
                    }
                    else
                    {
                        _log.Debug(FormV4.EntryController.InputValue + " ready for querying");
                    }
                }  
                else if (screenForm == "V4_QRTicket")
                {
                    FormV4_QRTicket.EntryController.ReadInputValue = true;
                    if (showTestInput)
                        FormV4_QRTicket.EntryController.InputValue = this.txtInputCode.Text;
                    else
                        _log.Debug(FormV4_QRTicket.EntryController.InputValue + " ready for querying");
                }
                else if (screenForm == "V4_3opt")
                {
                    FormV4_3opt.EntryController.ReadInputValue = true;
                    if (showTestInput)
                        FormV4_3opt.EntryController.InputValue = this.txtInputCode.Text;
                    else
                        _log.Debug(FormV4_3opt.EntryController.InputValue + " ready for querying");
                }
                else if (screenForm == "V4_1opt")
                {
                    FormV4_1opt.EntryController.ReadInputValue = true;
                    if (showTestInput)
                        FormV4_1opt.EntryController.InputValue = this.txtInputCode.Text;
                    else
                        _log.Debug(FormV4_1opt.EntryController.InputValue + " ready for querying");
                }
                else if (screenForm == "V4_Ticket_PH")
                {
                    FormV4_Ticket_PH.EntryController.ReadInputValue = true;
                    if (showTestInput)
                        FormV4_Ticket_PH.EntryController.InputValue = this.txtInputCode.Text;
                    else
                        _log.Debug(FormV4_Ticket_PH.EntryController.InputValue + " ready for querying");
                }
            }
            else
            {
                if (screenForm == "V4")
                {
                    if (FormV4.EntryController.InputValue == null)
                    {
                        FormV4.EntryController.InputValue = "";
                    }

                    FormV4.EntryController.InputValue += e.KeyChar;
                }
                else if (screenForm == "V4_QRTicket")
                {
                    if (FormV4_QRTicket.EntryController.InputValue == null)
                        FormV4_QRTicket.EntryController.InputValue = "";

                    FormV4_QRTicket.EntryController.InputValue += e.KeyChar;
                }
                else if (screenForm == "V4_3opt")
                {
                    if (FormV4_3opt.EntryController.InputValue == null)
                        FormV4_3opt.EntryController.InputValue = "";

                    FormV4_3opt.EntryController.InputValue += e.KeyChar;
                }
                else if (screenForm == "V4_1opt")
                {
                    if (FormV4_1opt.EntryController.InputValue == null)
                        FormV4_1opt.EntryController.InputValue = "";

                    FormV4_1opt.EntryController.InputValue += e.KeyChar;
                }
                else if (screenForm == "V4_Ticket_PH")
                {
                    if (FormV4_Ticket_PH.EntryController.InputValue == null)
                        FormV4_Ticket_PH.EntryController.InputValue = "";

                    FormV4_Ticket_PH.EntryController.InputValue += e.KeyChar;
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
            this.btnPrintTicket.Visible = false;
            this.txtTransientId.Text = "";
            this.txtTicketQR.Text = "";
            this.Hidden = true;
            this.Hide();
        }

        private void btnPrintTicket_Click(object sender, EventArgs e)
        {
            if(!string.IsNullOrEmpty(this.txtTransientId.Text) && !string.IsNullOrEmpty(this.txtTicketQR.Text))
            {
                PangoService.PrintTicketQR(ConfigurationManager.AppSettings["InstallationId"], ConfigurationManager.AppSettings["TerminalId"], this.txtTransientId.Text, this.txtTicketQR.Text, true, false);

                this.btnPrintTicket.Enabled = false;
                //this.btnPrintReceipt.Text = "PRINTING RECEIPT...";
            }
        }
    }
}