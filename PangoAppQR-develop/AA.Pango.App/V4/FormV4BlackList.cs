using AA.Pango.App.Properties;
using AA.Pango.Model;
using AA.Pango.ServiceLayer;
using AA.Pango.TestWinApp.Utils;
using log4net;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;

namespace AA.Pango.App.V4
{
    public partial class FormV4BlackList : Form
    {
        protected ILog _log = LogManager.GetLogger("FormV4BlackList");
        private Thread Worker { get; set; }
        private System.Resources.ResourceManager RM = new System.Resources.ResourceManager("AA.Pango.App.Properties.Resources", typeof(Resources).Assembly);

        CultureInfo deC = new CultureInfo("en-US");
        private static string dateTimeFormat = ConfigurationManager.AppSettings["DateTimeFormat"];
        private static string footVersion = ConfigurationManager.AppSettings["FootVersion"];

        public FormV4BlackList()
        {
            InitializeComponent();
        }

        public FormV4BlackList(int secondsToShow)
        {
            InitializeComponent();

            Thread _thread = new Thread(() =>
            {
                while (true)
                {
                    Thread.Sleep(secondsToShow * 1000);

                    this.CloseForm();
                }
            });

            _thread.SetApartmentState(ApartmentState.STA);
            _thread.Start();
        }

        public FormV4BlackList(TemplateChangeScreen changeScreenData)
        {
            _log.Debug("changeScreenData - id: " + changeScreenData._id + ", template: " + changeScreenData.TemplateId);

            InitializeComponent();

            ChangeScreenService.SetProcessedChangeScreen(changeScreenData._id);

            Thread _thread = new Thread(() =>
            {
                while (true)
                {
                    Thread.Sleep(changeScreenData.SecondsToShowOnScreen * 1000);

                    this.CloseForm();
                }
            });

            _thread.SetApartmentState(ApartmentState.STA);
            _thread.Start();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Init();
            this.Disposed += FormV4BlackList_Disposed;
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

            int terminalId = int.Parse(ConfigurationManager.AppSettings["TerminalId"]);
            var eventNotificationTypeToSend = ConfigurationManager.AppSettings["EventNotificationTypeToSend"] ?? "Entry";

            var descTerminalType = eventNotificationTypeToSend == "Entry" ?
                LanguageLocalizationParser.GetCurrentLanguageTags["EntryTerminalDescription"] :
                LanguageLocalizationParser.GetCurrentLanguageTags["ExitTerminalDescription"];

            #region Style Edits
            this.BackColor = ColorTranslator.FromHtml(DataFileParser.Styles["BackgroundColor"]);
            this.ForeColor = ColorTranslator.FromHtml(DataFileParser.Styles["FontColor"]);

            this.btnGoBack.BackColor = ColorTranslator.FromHtml(DataFileParser.Styles["BtnBackBackgroundColor"]);
            this.btnGoBack.ForeColor = ColorTranslator.FromHtml(DataFileParser.Styles["BtnBackFontColor"]);
            this.btnGoBack.FlatAppearance.BorderColor = ColorTranslator.FromHtml(DataFileParser.Styles["BtnBackBorderColor"]);

            this.panelFoot.BackColor = ColorTranslator.FromHtml(DataFileParser.Styles["FootBackgroundColor"]);
            this.panelFoot.ForeColor = ColorTranslator.FromHtml(DataFileParser.Styles["FootFontColor"]);

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

            this.lblStatusKO.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4BlackList_StatusKO"];
            this.lblInstructionsKO.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4BlackList_InstructionsKO"];
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
                    }
                    catch (Exception e)
                    {
                        _log.Error("Unexpected error in Worker thread", e);
                        CloseForm();
                    }
                    finally
                    {
                        Thread.Sleep(60000);
                    }
                }
            })));

            Worker.SetApartmentState(ApartmentState.STA);
            Worker.Start();
        }

        private void FormV4BlackList_Disposed(object sender, EventArgs e)
        {
            _log.Error("FORM DISPOSED!!!");
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
