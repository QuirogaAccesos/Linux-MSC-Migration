using AA.Pango.ServiceLayer;
using AA.Pango.TestWinApp.Utils;
using AA.Pango.TestWinApp;
using log4net;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using AA.Pango.ServiceLayer.ApiDtos;
using AA.Pango.App.Properties;

namespace AA.Pango.App
{
    public partial class FormV3PayByPlate : Form
    {
        protected ILog _log = LogManager.GetLogger("FormV3PayByPlate");
        CultureInfo deC = new CultureInfo("en-US");
        private Thread Worker { get; set; }
        private System.Resources.ResourceManager RM = new System.Resources.ResourceManager("AA.Pango.App.Properties.Resources", typeof(Resources).Assembly);

        private static string dateTimeFormat = ConfigurationManager.AppSettings["DateTimeFormat"];
        private static bool showPlateOrPhone = bool.Parse(ConfigurationManager.AppSettings["ShowPlateOrPhone"]);
        private static string footVersion = ConfigurationManager.AppSettings["FootVersion"];

        public FormV3PayByPlate()
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
            SignalVehiclePresenceToApi();
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
            var eventNotificationTypeToSend = ConfigurationManager.AppSettings["EventNotificationTypeToSend"] ?? "Entry";

            var descTerminalType = eventNotificationTypeToSend == "Entry" ?
                LanguageLocalizationParser.GetCurrentLanguageTags["EntryTerminalDescription"] :
                LanguageLocalizationParser.GetCurrentLanguageTags["ExitTerminalDescription"];

            #region Style Edits
            var backColor = ColorTranslator.FromHtml(DataFileParser.Styles["BackgroundColor"]);
            this.BackColor = backColor;
            this.plateOrPhone1.BackColor = backColor;

            var fontColor = ColorTranslator.FromHtml(DataFileParser.Styles["FontColor"]);
            this.ForeColor = fontColor;
            this.panelReading.ForeColor = fontColor;
            this.tableInstructionsOK.ForeColor = fontColor;

            var headerBackColor = ColorTranslator.FromHtml(DataFileParser.Styles["HeaderBackgroundColor"]);
            this.lblHeader.BackColor = headerBackColor;
            this.lblStatusKO.BackColor = headerBackColor;

            var headerFontColor = ColorTranslator.FromHtml(DataFileParser.Styles["HeaderFontColor"]);
            this.lblHeader.ForeColor = headerFontColor;
            this.lblStatusKO.ForeColor = headerFontColor;

            this.btnGoBack.BackColor = ColorTranslator.FromHtml(DataFileParser.Styles["BtnBackBackgroundColor"]);
            this.btnGoBack.ForeColor = ColorTranslator.FromHtml(DataFileParser.Styles["BtnBackFontColor"]);
            this.btnGoBack.FlatAppearance.BorderColor = ColorTranslator.FromHtml(DataFileParser.Styles["BtnBackBorderColor"]);

            this.panelFoot.BackColor = ColorTranslator.FromHtml(DataFileParser.Styles["FootBackgroundColor"]);
            this.panelFoot.ForeColor = ColorTranslator.FromHtml(DataFileParser.Styles["FootFontColor"]);

            this.imgEntryLPR.BackgroundImage = (Image)RM.GetObject(DataFileParser.Styles["ImgCamera"]);
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
            this.labelReading.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV3PayByPlate_Reading"];
            this.lblStatusOK.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV3PayByPlate_StatusOK"];
            this.lblInstructionsOK.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV3PayByPlate_InstructionsOK"];
            this.lblInstructionsOK_2.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV3PayByPlate_InstructionsOK_2"];
            this.lblStatusKO.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV3PayByPlate_StatusKO"];
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
                            if (checkCCResponse?.templateId == "plateNotReadByLPRIn")
                            {
                                if (showPlateOrPhone)
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
                                }
                                else
                                {
                                    bool hasPhone = openPhoneForm();
                                    if(!hasPhone)
                                        timeout = 0;
                                }
                            }
                            else if (checkCCResponse?.templateId == "blackList")
                            {
                                var form = new FormV3BlackList(timeout);
                                form.FormClosed += NewForm_FormClosed;
                                form.TopMost = true;

                                _log.Debug("Showing form");
                                Application.Run(form);

                                timeout = 0;
                            }
                            else
                            {
                                _log.Error("Invalid template ID: " + checkCCResponse?.templateId);
                            }
                        }
                        else
                        {                         //you will pay at the exit with cc
                            if (checkCCResponse?.templateId == "payByPlateIn")
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
                                _log.Error("Invalid template ID: " + checkCCResponse?.templateId);
                            }
                        }

                        Thread.Sleep(timeout * 1000);
                    }
                    catch (Exception e)
                    {
                        _log.Error("Unexpected error in SignalVehiclePresenceToApi thread", e);
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
            _log.Debug("The " + (e.Type == 1 ? "Plate" : "Phone") + " was registered: " + e.Status);

            if(e.Status == "OK")
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

        private bool openPhoneForm()
        {
            bool result = false;
            var form = new ScreenDigitKeyboardV3(1, true, LanguageLocalizationParser.GetCurrentLanguageTags["FormV3PayByPlate_StatusKOPhone"]);
            var dialogResult = form.ShowDialog(this);

            if (dialogResult == DialogResult.OK)
            {
                if (string.IsNullOrEmpty(form.Phone))
                    return result;

                if (form.Phone.Length != 10)
                {
                    _log.Debug("Invalid Phone");
                    //labelError.Text = "Invalid Phone";
                    return result;
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

                        result = true;
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

            return result;
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

        private void NewForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            //this.TopMost = true;

            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)(() =>
                {
                    this.Activate();
                    this.WindowState = FormWindowState.Maximized;
                    MinimumSize = this.Size;
                    MaximumSize = this.Size;
                }));
            }
            else
            {
                this.Activate();
                this.WindowState = FormWindowState.Maximized;
                MinimumSize = this.Size;
                MaximumSize = this.Size;
            }
        }
    }
}