using AA.Pango.App.Properties;
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
using System.Threading;
using System.Windows.Forms;

namespace AA.Pango.App
{
    public partial class FormV4PullTicket : Form
    {
        protected ILog _log = LogManager.GetLogger("FormV4PullTicket");
        CultureInfo deC = new CultureInfo("en-US");
        private static string _terminalId = ConfigurationManager.AppSettings["TerminalId"];
        private static string _installationId = ConfigurationManager.AppSettings["InstallationId"];
        private static string dateTimeFormat = ConfigurationManager.AppSettings["DateTimeFormat"];
        private static string footVersion = ConfigurationManager.AppSettings["FootVersion"];
        private static string ticketType = ConfigurationManager.AppSettings["TicketType"];
        private static string showPhone = ConfigurationManager.AppSettings["PullTicketShowPhone"];
        private int pullTicketFormTimeout = int.Parse(ConfigurationManager.AppSettings["PullTicketFormTimeout"] ?? "5000");

        public TerminalController EntryController { get; set; }
        private Thread Worker { get; set; }
        private System.Resources.ResourceManager RM = new System.Resources.ResourceManager("AA.Pango.App.Properties.Resources", typeof(Resources).Assembly);

        public FormV4PullTicket()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Init();
        }

        private void Init()
        {
            InitSettings();
            SetVersion();
            if (showPhone == "true")
                OpenDigitKeyboard();
            else
                PrintTicket();
            CloseOnTimeout();
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

            LanguageLocalizationParser.LoadLocalizations();

            bool showTestInput = bool.Parse(ConfigurationManager.AppSettings["ShowTestInput"] ?? "false");
            int terminalId = int.Parse(ConfigurationManager.AppSettings["TerminalId"]);
            var eventNotificationTypeToSend = ConfigurationManager.AppSettings["EventNotificationTypeToSend"] ?? "Entry";

            var descTerminalType = eventNotificationTypeToSend == "Entry" ?
                LanguageLocalizationParser.GetCurrentLanguageTags["EntryTerminalDescription"] :
                LanguageLocalizationParser.GetCurrentLanguageTags["ExitTerminalDescription"];

            #region Style Edits
            this.BackColor = ColorTranslator.FromHtml(DataFileParser.Styles["BackgroundColor"]);
            this.ForeColor = ColorTranslator.FromHtml(DataFileParser.Styles["FontColor"]);

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

            this.imgPullTicket.BackgroundImage = (Image)RM.GetObject(DataFileParser.Styles["ImgPullTicket"]);
            this.imgEntryTicket.BackgroundImage = (Image)RM.GetObject(DataFileParser.Styles["ImgTicket"]);
            this.pictureBox2.BackgroundImage = (Image)RM.GetObject(DataFileParser.Styles["ImgArrow"]);
            this.pictureBox3.BackgroundImage = (Image)RM.GetObject(DataFileParser.Styles["ImgCar"]);
            this.pictureBox4.BackgroundImage = (Image)RM.GetObject(DataFileParser.Styles["ImgArrow"]);
            this.pictureBox5.BackgroundImage = (Image)RM.GetObject(DataFileParser.Styles["ImgBarrier"]);

            if (!string.IsNullOrEmpty(DataFileParser.Styles["ImgPullTicket"]))
            {
                this.imgPullTicket.Visible = true;
                this.lblInstructionsPullImg.Visible = true;
                this.lblInstructionsPull.Visible = false;
            }
            else
            {
                this.imgPullTicket.Visible = false;
                this.lblInstructionsPullImg.Visible = false;
                this.lblInstructionsPull.Visible = true;
            }

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

            #endregion Style Edits

            this.lblHeader.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4_Welcome"];
            this.lblInstructions.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4PullTicket_GetTicket"];
            this.lblInstructionsPull.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4PullTicket_Instructions"];
            this.lblInstructionsPullImg.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4PullTicket_Instructions"];
            this.lblStatusOK.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4PullTicket_StatusOK"];
            this.lblInstructionsOK.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4PullTicket_InstructionsOK"];
            //this.lblStatusOKPorSiAcaso.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4PullTicket_StatusOK"];
            //this.lblInstructionsOKPorSiAcaso.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4PullTicket_InstructionsOK"];
            //this.lblInstructionsOK_2.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4PullTicket_InstructionsOK_2"];
            this.lblFootDownload.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4_DownloadApp"];
            this.lblFootPrice.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4_Price"];
            this.btnGoBack.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4_btnBack"];

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
                while (true)
                {
                    if (this.IsDisposed)
                    {
                        _log.Debug("exiting thread...");
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

                        Thread.Sleep(60000);
                    }
                    catch (Exception e)
                    {
                        _log.Error("Unexpected error in Worker thread", e);
                    }
                }
            })));
            Worker.SetApartmentState(ApartmentState.STA);
            Worker.Start();
        }

        private void CloseOnTimeout()
        {
            new Thread(new ThreadStart(() =>
            {
                Thread.Sleep(pullTicketFormTimeout);

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
            })).Start();
        }

        private void PrintTicket()
        {
            var thread = new Thread(new ThreadStart(() =>
            {
                //PangoService.PrintTicketQR(_installationId, _terminalId, "929", "BmmSaNgTGWep0kYNnfliwWv+5KgWOxnVDeuc9ofpD53xq4AbQDrarvuQmp136ldGZwoC+AKL9N22Dszuu2ODMvBuZIeSbfEXp+DCWcMA1GE=", false);
                if (ticketType == "PANGO")
                {
                    var (ticketRequestSent, ticketResponse) = PangoService.TransientPlateOrPhone(new TransientPlateOrPhoneRequest
                    {
                        installationID = _installationId,
                        terminalId = _terminalId,
                        ticket = true
                    });

                    _log.Debug("Ticket request sent: " + (ticketRequestSent ? "OK" : "NOT OK"));

                    if (ticketRequestSent)
                    {
                        _log.Debug("status received: " + ticketResponse.status);

                        if (ticketResponse.status == 1)
                        {
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
                            
                            PangoService.PrintTicketQR(_installationId, _terminalId, ticketResponse.transientId.ToString(), ticketResponse.ticketQR, false, true);
                        }
                        else
                        {
                            _log.Error("error message: " + ticketResponse.message);
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
                }
            }));

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        private void OpenDigitKeyboard()
        {
            var form = new ScreenDigitKeyboardV4(1);
            var dialogResult = form.ShowDialog(this);

            if (dialogResult == DialogResult.OK)
            {
                if (string.IsNullOrEmpty(form.Phone))
                    this.CloseForm();

                if (form.Phone.Length != 10)
                    return;

                var (phoneRequestSent, phoneResponse) = PangoService.TransientPlateOrPhone(new TransientPlateOrPhoneRequest
                {
                    installationID = ConfigurationManager.AppSettings["InstallationId"],
                    terminalId = ConfigurationManager.AppSettings["TerminalId"],
                    phone = form.Phone,
                    ticket = true
                });

                _log.Debug("Plate request sent: " + (phoneRequestSent ? "OK" : "NOT OK"));

                if (phoneRequestSent)
                {
                    _log.Debug("status received: " + phoneResponse.status);

                    if (phoneResponse.status == 1)
                    {
                        if (ticketType == "PANGO")
                            PangoService.PrintTicketQR(_installationId, _terminalId, phoneResponse.transientId.ToString(), phoneResponse.ticketQR, true, true);
                        //this.imgEntryPhone.BringToFront();
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
                    else
                    {
                        _log.Error("error message: " + phoneResponse.message);
                        this.CloseForm();
                    }
                }
                else
                    this.CloseForm();
            }
            else
            {
                _log.Warn("No plate was read from the user.");
                this.CloseForm();
            }
        }

        private void btnGoBack_Click(object sender, EventArgs e)
        {
            _log.Debug("GoBack");
            this.CloseForm();
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
    }
}