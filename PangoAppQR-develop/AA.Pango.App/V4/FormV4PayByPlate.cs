using AA.Pango.App.Properties;
using AA.Pango.App.V4;
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
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace AA.Pango.App
{
    public partial class FormV4PayByPlate : Form
    {
        protected ILog _log = LogManager.GetLogger("FormV4PayByPlate");
        CultureInfo deC = new CultureInfo("en-US");
        public TerminalController EntryController { get; set; }
        private Thread Worker { get; set; }
        private System.Resources.ResourceManager RM = new System.Resources.ResourceManager("AA.Pango.App.Properties.Resources", typeof(Resources).Assembly);

        private static string dateTimeFormat = ConfigurationManager.AppSettings["DateTimeFormat"];
        private static string numOpt = ConfigurationManager.AppSettings["PlateOrPhoneOptions"];
        private static string footVersion = ConfigurationManager.AppSettings["FootVersion"];
        private static string ticketType = ConfigurationManager.AppSettings["TicketType"];
        private static bool showBtnPrintTicket = bool.Parse(ConfigurationManager.AppSettings["ShowBtnPrintTicketPango"]);

        public FormV4PayByPlate()
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

            int terminalId = int.Parse(ConfigurationManager.AppSettings["TerminalId"]);
            var eventNotificationTypeToSend = ConfigurationManager.AppSettings["EventNotificationTypeToSend"] ?? "Entry";

            var descTerminalType = eventNotificationTypeToSend == "Entry" ?
                LanguageLocalizationParser.GetCurrentLanguageTags["EntryTerminalDescription"] :
                LanguageLocalizationParser.GetCurrentLanguageTags["ExitTerminalDescription"];

            #region Style Edits
            this.BackColor = ColorTranslator.FromHtml(DataFileParser.Styles["BackgroundColor"]);

            var fontColor = ColorTranslator.FromHtml(DataFileParser.Styles["FontColor"]);
            this.ForeColor = fontColor;
            this.btnPrintTicket.ForeColor = fontColor;

            var headerBackColor = ColorTranslator.FromHtml(DataFileParser.Styles["HeaderBackgroundColor"]);
            this.lblHeader.BackColor = headerBackColor;
            this.lblStatusKO.BackColor = headerBackColor;
            
            var headerFontColor = ColorTranslator.FromHtml(DataFileParser.Styles["HeaderFontColor"]);
            this.lblHeader.ForeColor = headerFontColor;
            this.lblStatusKO.ForeColor = headerFontColor;

            this.btnGoBack.BackColor = ColorTranslator.FromHtml(DataFileParser.Styles["BtnBackBackgroundColor"]);
            this.btnGoBack.ForeColor = ColorTranslator.FromHtml(DataFileParser.Styles["BtnBackFontColor"]);
            this.btnGoBack.FlatAppearance.BorderColor = ColorTranslator.FromHtml(DataFileParser.Styles["BtnBackBorderColor"]);

            this.btnPrintTicket.BackColor = ColorTranslator.FromHtml(DataFileParser.Styles["BtnPrintTicketBackgroundColor"]);
            this.btnPrintTicket.ForeColor = ColorTranslator.FromHtml(DataFileParser.Styles["BtnPrintTicketFontColor"]);

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
            this.labelReading.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4PayByPlate_Reading"];
            this.lblStatusOK.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4PayByPlate_StatusOK"];
            this.lblInstructionsOK.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4PayByPlate_InstructionsOK"];
            //this.lblStatusOKPorSiAcaso.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4PayByPlate_StatusOK"];
            //this.lblInstructionsOKPorSiAcaso.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4PayByPlate_InstructionsOK"];
            //this.lblInstructionsOK_2.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4PayByPlate_InstructionsOK_2"];
            if (numOpt == "1")
                this.lblStatusKO.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4PayByPlate_StatusKO_1opt"];
            else if (numOpt == "2")
                this.lblStatusKO.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4PayByPlate_StatusKO_2opt"];
            else if (numOpt == "3")
                this.lblStatusKO.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4PayByPlate_StatusKO_3opt"];
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
                            else if(checkCCResponse?.templateId == "blackList")
                            {
                                var form = new FormV4BlackList(timeout);
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

                                if (ticketType == "PANGO" && showBtnPrintTicket)
                                {
                                    if (!btnPrintTicket.IsDisposed)
                                    {
                                        if (this.btnPrintTicket.InvokeRequired)
                                        {
                                            this.btnPrintTicket.Invoke((MethodInvoker)(() =>
                                            {
                                                this.btnPrintTicket.Visible = true;
                                            }));
                                        }
                                        else
                                            this.btnPrintTicket.Visible = true;
                                    }
                                }
                                
                                if (!txtTransientId.IsDisposed)
                                {
                                    if (this.txtTransientId.InvokeRequired)
                                    {
                                        this.txtTransientId.Invoke((MethodInvoker)(() =>
                                        {
                                            this.txtTransientId.Text = checkCCResponse?.transientId;
                                        }));
                                    }
                                    else
                                        this.txtTransientId.Text = checkCCResponse?.transientId;
                                }

                                if (!txtTicketQR.IsDisposed)
                                {
                                    if (this.txtTicketQR.InvokeRequired)
                                    {
                                        this.txtTicketQR.Invoke((MethodInvoker)(() =>
                                        {
                                            this.txtTicketQR.Text = checkCCResponse?.ticketQR;
                                        }));
                                    }
                                    else
                                        this.txtTicketQR.Text = checkCCResponse?.ticketQR;
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
            _log.Debug("The " + (e.Type == 1 ? "Plate" : e.Type == 2 ? "Phone" : "Ticket") + " was registered: " + e.Status);

            if(e.Status == "OK")
            {
                this.lblInstructionsOK.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4PayByPlate_InstructionsOK"];

                if (e.Type == 1)
                {
                    this.imgEntryPlate.BringToFront();
                    if (ticketType == "PANGO"  && showBtnPrintTicket)
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
                    this.lblInstructionsOK.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4PayByPlate_InstructionsOK_Ticket"];
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

        private void btnPrintTicket_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(this.txtTransientId.Text) && !string.IsNullOrEmpty(this.txtTicketQR.Text))
            {
                PangoService.PrintTicketQR(ConfigurationManager.AppSettings["InstallationId"], ConfigurationManager.AppSettings["TerminalId"], this.txtTransientId.Text, this.txtTicketQR.Text, true, false);

                this.btnPrintTicket.Enabled = false;
                //this.btnPrintReceipt.Text = "PRINTING RECEIPT...";
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