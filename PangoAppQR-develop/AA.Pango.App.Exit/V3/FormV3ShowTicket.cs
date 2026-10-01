using AA.Pango.App.Exit.Properties;
using AA.Pango.ServiceLayer.Controllers;
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
    public partial class FormV3ShowTicket : Form
    {
        protected ILog _log = LogManager.GetLogger("FormV3ShowTicket");
        CultureInfo deC = new CultureInfo("en-US");

        private static string dateTimeFormat = ConfigurationManager.AppSettings["DateTimeFormat"];
        private static string screenForm = ConfigurationManager.AppSettings["ScreenForm"];
        private static string footVersion = ConfigurationManager.AppSettings["FootVersion"];

        private int showTicketFormTimeout = int.Parse(ConfigurationManager.AppSettings["ShowTicketFormTimeout"] ?? "10000");
        private Thread Worker { get; set; }
        private System.Resources.ResourceManager RM = new System.Resources.ResourceManager("AA.Pango.App.Exit.Properties.Resources", typeof(Resources).Assembly);

        public FormV3ShowTicket()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Init();

            Thread _thread = new Thread(() =>
            {
                Thread.Sleep(showTicketFormTimeout);

                if (this.IsDisposed)
                {
                    return;
                }

                CloseForm();
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

            LanguageLocalizationParser.LoadLocalizations();
            DataFileParser.LoadStyles("styles.dat");

            int terminalId = int.Parse(ConfigurationManager.AppSettings["TerminalId"]);
            var eventNotificationTypeToSend = ConfigurationManager.AppSettings["EventNotificationTypeToSend"] ?? "Exit";

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

            this.pictureBox2.BackgroundImage = (Image)RM.GetObject(DataFileParser.Styles["ShowTicket"]);

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
            this.lblInstructions.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV3ShowTicket_Instructions"];
            this.lblFootDownload.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV3_DownloadApp"];
            this.btnGoBack.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV3_btnBack"];

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
                    try
                    {
                        if (this.IsDisposed)
                        {
                            _log.Debug("exiting thread...");
                            break;
                        }

                        if (!this.IsHandleCreated)
                        {
                            _log.Warn("handle not created");
                            break;
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

                        #region Entry Controller Actions
                        TerminalState terminalState = TerminalState.StandBy;
                        bool setTerminalState = false;
                        if (screenForm == "V3_QRTicket")
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
                                }));
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
                                }));
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
                    }
                    finally
                    {
                        Thread.Sleep(1000);
                    }
                    #endregion Entry Controller Actions
                }
            })));

            Worker.Start();
        }

        private void Form1_FormClosing_1(object sender, FormClosingEventArgs e)
        {
            this.Hide();
        }

        private void btnGoBack_Click(object sender, EventArgs e)
        {
            _log.Debug("GoBack");
            this.Hide();
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