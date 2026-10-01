using AA.Pango.ServiceLayer;
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
    public partial class FormV3_CheckPlate : Form
    {
        protected ILog _log = LogManager.GetLogger("FormV3_CheckPlate");
        CultureInfo deC = new CultureInfo("en-US");
        private Thread Worker { get; set; }
        private static string _installationId = ConfigurationManager.AppSettings["InstallationId"];
        private static string _terminalId = ConfigurationManager.AppSettings["TerminalId"];
        private static string dateTimeFormat = ConfigurationManager.AppSettings["DateTimeFormat"];

        public FormV3_CheckPlate()
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
            CheckEventPlate();
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
            _log.Debug("Init settings...");

            LanguageLocalizationParser.LoadLocalizations();

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

            this.lblHeader.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV3_Welcome"];
            this.labelReading.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV3PayByPlate_Reading"];
            this.lblStatusOK.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV3PayByPlate_StatusOK"];
            this.lblInstructionsOK.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV3PayByPlate_InstructionsOK"];
            this.lblInstructionsOK_2.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV3PayByPlate_InstructionsOK_2"];
            this.lblStatusKO.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV3PayByPlate_StatusKO"];
            this.lblFootDownload.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV3_DownloadApp"];
            this.lblFootPrice.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV3_Price"];
            this.btnGoBack.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV3_btnBack"];

            this.tlblTerminalId.Text = "Terminal: " + int.Parse(_terminalId) + ". " + descTerminalType;
            this.tlblDate.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            this.lblCurrentDate.Text = DateTime.Now.ToString(dateTimeFormat);

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
                    catch (Exception e)
                    {
                        _log.Error("Unexpected error in Worker thread", e);
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

        private void CheckEventPlate()
        {
            var thread = new Thread(new ThreadStart(() =>
            {
                var (responseSucceeded, checkEventResponse) = PangoService.CheckEventPlate(
                   new ServiceLayer.ApiDtos.CheckEventPlateRequest
                   {
                       installationID = _installationId,
                       terminalId = _terminalId,
                   });

                if (responseSucceeded)
                {
                    try
                    {
                        _log.Debug("Check Event Plate request sent to API successfully");

                        var showTimeParam = checkEventResponse?.paramList.FirstOrDefault(x => x.param_name == "showTime");

                        var timeout = 30;

                        if (showTimeParam != null)
                        {
                            int.TryParse(showTimeParam.param_value, out timeout);
                        }

                        _log.Debug("Show Time in seconds: " + timeout);

                        if (checkEventResponse?.plateNotReadByLPR == true)
                        {                         //license cannot be read, please pull a ticket
                            if (checkEventResponse?.templateId == "plateNotReadByLPRIn")
                            {
                                if (!panelKO.IsDisposed)
                                {
                                    if (this.panelKO.InvokeRequired)
                                    {
                                        this.panelKO.Invoke((MethodInvoker)(() =>
                                        {
                                            this.panelKO.Visible = true;
                                            this.panelKO.BringToFront();
                                        }));
                                    }
                                    else
                                    {
                                        this.panelKO.Visible = true;
                                        this.panelKO.BringToFront();
                                    }
                                }

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
                            else
                            {
                                _log.Error("Invalid template ID: " + checkEventResponse?.templateId);
                            }
                        }
                        else
                        {                         //you will pay at the exit with cc
                            if (checkEventResponse?.templateId == "payByPlateIn")
                            {
                                if (!panelKO.IsDisposed)
                                {
                                    if (this.panelKO.InvokeRequired)
                                    {
                                        this.panelKO.Invoke((MethodInvoker)(() =>
                                        {
                                            this.panelKO.Visible = false;
                                            this.panelKO.SendToBack();
                                        }));
                                    }
                                    else
                                    {
                                        this.panelKO.Visible = false;
                                        this.panelKO.SendToBack();
                                    }
                                }

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
                            }
                            else
                            {
                                _log.Error("Invalid template ID: " + checkEventResponse?.templateId);
                            }
                        }

                        Thread.Sleep(timeout * 1000);
                    }
                    catch (Exception e)
                    {
                        _log.Error("Unexpected error in CheckEventPlate thread", e);
                    }
                    finally
                    {
                        this.CloseForm();
                    }
                }
            }));

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        public void c_Registered(object sender, StatusEventArgs e)
        {
            _log.Debug("The Phone was registered: " + e.Status);

            if (e.Status == "OK")
            {
                this.panelOK.Visible = true;
                this.panelOK.BringToFront();
                this.imgEntryPhone.BringToFront();
            }
            else
            {
                this.panelOK.Visible = false;
                this.panelOK.SendToBack();
            }
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
