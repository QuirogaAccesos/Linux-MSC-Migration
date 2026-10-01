using AA.Pango.ServiceLayer;
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
    public partial class FormV2PayByPlate : Form
    {
        protected ILog _log = LogManager.GetLogger("FormV2PayByPlate");
        CultureInfo deC = new CultureInfo("en-US");
        public TerminalController EntryController { get; set; }
        private Thread Worker { get; set; }

        public FormV2PayByPlate()
        {
            InitializeComponent();
        }

        private void Init()
        {
            InitSettings();
            SetVersion();
            SignalVehiclePresenceToApi();
            InitWorker();
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
                                    statusStrip1.Items["tlblDate"].Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                                }));
                            }
                            else
                            {
                                statusStrip1.Items["tlblDate"].Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                            }
                        }

                        if (!lblCurrentDate.IsDisposed)
                        {
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
                        }


                    }
                    catch (Exception e)
                    {
                        _log.Error("Unexpected error in Worker thread", e);
                    }

                    Thread.Sleep(500);

                }
            })));

            Worker.SetApartmentState(ApartmentState.STA);
            Worker.Start();
        }


        private void SignalVehiclePresenceToApi()
        {
            var thread = new Thread(new ThreadStart(() =>
            {

                var (responseSucceeded, checkCCResponse) = PangoService.CheckCCTransient(
                   new ServiceLayer.ApiDtos.CheckCCTransientRequest
                   {
                       installationID = ConfigurationManager.AppSettings["InstallationId"],
                       terminalId = ConfigurationManager.AppSettings["TerminalId"],
                   });

                if (responseSucceeded)
                {
                    try
                    {
                        _log.Debug("Check CC request sent to API successfully");

                        var showTimeParam = checkCCResponse?.paramList.FirstOrDefault(x => x.param_name == "showTime");

                        var timeout = 30;

                        if (showTimeParam != null)
                        {
                            int.TryParse(showTimeParam.param_value, out timeout);
                        }

                        _log.Debug("Show Time in seconds: " + timeout);

                        if (checkCCResponse?.plateNotReadByLPR == true)
                        {                         //license cannot be read, please pull a ticket
                            if (checkCCResponse.templateId == "plateNotReadByLPRIn")
                            {

                                if (!lblPlateNotRead.IsDisposed)
                                {
                                    if (this.lblPlateNotRead.InvokeRequired)
                                    {
                                        this.lblPlateNotRead.Invoke((MethodInvoker)(() =>
                                        {
                                            this.lblPlateNotRead.Visible = true;
                                            this.lblPlateNotRead.BringToFront();
                                        }));
                                    }
                                    else
                                    {
                                        this.lblPlateNotRead.Visible = true;
                                        this.lblPlateNotRead.BringToFront();
                                    }

                                }

                                if (!lblPlateRead.IsDisposed)
                                {
                                    if (this.lblPlateRead.InvokeRequired)
                                    {
                                        this.lblPlateRead.Invoke((MethodInvoker)(() =>
                                        {
                                            this.lblPlateRead.Visible = false;
                                            this.lblPlateRead.SendToBack();
                                        }));
                                    }
                                    else
                                    {
                                        this.lblPlateRead.Visible = false;
                                        this.lblPlateRead.SendToBack();
                                    }

                                }


                            }
                            else
                            {
                                _log.Error("Invalid template ID: " + checkCCResponse.templateId);
                            }
                        }
                        else
                        {                         //you will pay at the exit with cc
                            if (checkCCResponse?.templateId == "payByPlateIn")
                            {

                                if (!lblPlateNotRead.IsDisposed)
                                {
                                    if (this.lblPlateNotRead.InvokeRequired)
                                    {
                                        this.lblPlateNotRead.Invoke((MethodInvoker)(() =>
                                        {
                                            this.lblPlateNotRead.Visible = false;
                                            this.lblPlateNotRead.SendToBack();
                                        }));
                                    }
                                    else
                                    {
                                        this.lblPlateNotRead.Visible = false;
                                        this.lblPlateNotRead.SendToBack();
                                    }
                                }

                                if (!lblPlateRead.IsDisposed)
                                {
                                    if (this.lblPlateRead.InvokeRequired)
                                    {
                                        this.lblPlateRead.Invoke((MethodInvoker)(() =>
                                        {
                                            this.lblPlateRead.Visible = true;
                                            this.lblPlateRead.BringToFront();
                                        }));
                                    }
                                    else
                                    {
                                        this.lblPlateRead.Visible = true;
                                        this.lblPlateRead.BringToFront();
                                    }
                                }

                            }
                            else
                            {
                                _log.Error("Invalid template ID: " + checkCCResponse?.templateId);
                            }
                        }

                        Thread.Sleep(timeout * 1000);

                        this.CloseForm();
                    }
                    catch (Exception e)
                    {
                        _log.Error("Unexpected error in SignalVehiclePresenceToApi thread", e);
                    }

                }

            }));

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

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

            this.txtInputCode.Focus();

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

        }


        private void pictureBox1_Click(object sender, EventArgs e)
        {
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Init();
        }


        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {

        }


        private void btnGoBack_Click(object sender, EventArgs e)
        {
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