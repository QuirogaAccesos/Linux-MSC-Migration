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
    public partial class FormV4CheckPermitPlate : Form
    {
        protected ILog _log = LogManager.GetLogger("FormV4CheckPermitPlate");
        CultureInfo deC = new CultureInfo("en-US");
        public TerminalController EntryController { get; set; }
        private Thread Worker { get; set; }
        private System.Resources.ResourceManager RM = new System.Resources.ResourceManager("AA.Pango.App.Properties.Resources", typeof(Resources).Assembly);

        private int checkPermitFormTimeout = int.Parse(ConfigurationManager.AppSettings["CheckPermitFormTimeout"] ?? "10000");
        private static string dateTimeFormat = ConfigurationManager.AppSettings["DateTimeFormat"];
        private static string footVersion = ConfigurationManager.AppSettings["FootVersion"];

        public FormV4CheckPermitPlate()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Init();
            this.OpenKeyboard();

            Thread _thread = new Thread(() =>
            {
                Thread.Sleep(checkPermitFormTimeout);

                if (this.IsDisposed)
                    return;

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

            LanguageLocalizationParser.LoadLocalizations();

            int terminalId = int.Parse(ConfigurationManager.AppSettings["TerminalId"]);
            var eventNotificationTypeToSend = ConfigurationManager.AppSettings["EventNotificationTypeToSend"] ?? "Entry";

            var descTerminalType = eventNotificationTypeToSend == "Entry" ?
                LanguageLocalizationParser.GetCurrentLanguageTags["EntryTerminalDescription"] :
                LanguageLocalizationParser.GetCurrentLanguageTags["ExitTerminalDescription"];

            #region Style Edits
            this.BackColor = ColorTranslator.FromHtml(DataFileParser.Styles["BackgroundColor"]);

            var fontColor = ColorTranslator.FromHtml(DataFileParser.Styles["FontColor"]);
            this.ForeColor = fontColor;

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
            this.imgEntryTicket.BackgroundImage = (Image)RM.GetObject(DataFileParser.Styles["ImgTicket"]);
            this.pictureBox2.BackgroundImage = (Image)RM.GetObject(DataFileParser.Styles["ImgArrow"]);
            this.pictureBox3.BackgroundImage = (Image)RM.GetObject(DataFileParser.Styles["ImgCar"]);
            this.pictureBox4.BackgroundImage = (Image)RM.GetObject(DataFileParser.Styles["ImgArrow"]);
            this.pictureBox5.BackgroundImage = (Image)RM.GetObject(DataFileParser.Styles["ImgBarrier"]);

            this.imgEntryPlate.BringToFront();

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
            this.labelReading.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4CheckPermitPlate_Reading"];
            this.lblStatusOK.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4CheckPermitPlate_StatusOK"];
            this.lblInstructionsOK.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4CheckPermitPlate_InstructionsOK"];
            //this.lblStatusOKPorSiAcaso.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4CheckPermitPlate_StatusOK"];
            //this.lblInstructionsOKPorSiAcaso.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4CheckPermitPlate_InstructionsOK"];
            //this.lblInstructionsOK_2.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4CheckPermitPlate_InstructionsOK_2"];
            this.lblStatusKO.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4CheckPermitPlate_StatusKO"];
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

        private void OpenKeyboard()
        {
            var form = new FrmOnScreenKeyboardV4(1);
            var dialogResult = form.ShowDialog(this);

            if (dialogResult == DialogResult.OK)
            {
                if (string.IsNullOrEmpty(form.Plate))
                    this.CloseForm();

                var (plateRequestSent, plateResponse) = PangoService.CheckPermitPlate(new CheckPermitPlateRequest
                {
                    installationID = ConfigurationManager.AppSettings["InstallationId"],
                    terminalId = ConfigurationManager.AppSettings["TerminalId"],
                    plate = form.Plate
                });

                _log.Debug("Plate request sent: " + (plateRequestSent ? "OK" : "NOT OK"));

                if (plateRequestSent)
                {
                    _log.Debug("status received: " + plateResponse.resultCode);

                    if (plateResponse.allowAccess)
                    {
                        this.imgEntryPlate.BringToFront();
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
                        _log.Error("error message: " + plateResponse.description);
                        OpenDigitKeyboard(form.Plate);
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

        private void OpenDigitKeyboard(string plate)
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
                    plate = plate,
                    phone = form.Phone
                });

                _log.Debug("Plate request sent: " + (phoneRequestSent ? "OK" : "NOT OK"));

                if (phoneRequestSent)
                {
                    _log.Debug("status received: " + phoneResponse.status);

                    if (phoneResponse.status == 1)
                    {
                        this.imgEntryPhone.BringToFront();
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