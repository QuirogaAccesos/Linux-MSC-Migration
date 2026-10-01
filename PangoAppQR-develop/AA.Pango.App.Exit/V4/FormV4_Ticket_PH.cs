using AA.Pango.App.Exit.Properties;
using AA.Pango.App.Exit.V4;
using AA.Pango.ServiceLayer;
using AA.Pango.ServiceLayer.ApiDtos;
using AA.Pango.ServiceLayer.Controllers;
using AA.Pango.ServiceLayer.Helpers;
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
    public partial class FormV4_Ticket_PH : Form
    {
        protected ILog _log = LogManager.GetLogger("FormV4_Ticket_PH");
        CultureInfo deC = new CultureInfo("en-US");
        public static volatile TerminalController Controller;
        private Thread Worker { get; set; }
        private System.Resources.ResourceManager RM = new System.Resources.ResourceManager("AA.Pango.App.Exit.Properties.Resources", typeof(Resources).Assembly);

        private static string _terminalId = ConfigurationManager.AppSettings["TerminalId"];
        private static string _installationId = ConfigurationManager.AppSettings["InstallationId"];
        private static string dateTimeFormat = ConfigurationManager.AppSettings["DateTimeFormat"];

        private int showNotFoundTimeout = int.Parse(ConfigurationManager.AppSettings["showNotFoundTimeout"] ?? "15000");
        private int showPaymOptionsTimeout = int.Parse(ConfigurationManager.AppSettings["showPaymOptionsTimeout"] ?? "15000");
        private static string headerVersion = ConfigurationManager.AppSettings["HeaderVersion"];
        private static string footVersion = ConfigurationManager.AppSettings["FootVersion"];
        private static bool notificarFreezed = false;
        private static string btnPayByPlateClickText;

        private FormV4ShowQrCode FormV4ShowQrCode { get; set; }

        public FormV4_Ticket_PH()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.plateOrPhone1.Registered += c_Registered;
            this.paymentOptions.Registered += c_PaymOptRegistered;
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

            this.Activate();
            this.WindowState = FormWindowState.Maximized;
            MinimumSize = this.Size;
            MaximumSize = this.Size;

            LanguageLocalizationParser.LoadLocalizations();
            DataFileParser.LoadStyles("styles.dat");

            var eventNotificationTypeToSend = ConfigurationManager.AppSettings["EventNotificationTypeToSend"] ?? "Exit";

            var descTerminalType = eventNotificationTypeToSend == "Entry" ?
                LanguageLocalizationParser.GetCurrentLanguageTags["EntryTerminalDescription"] :
                LanguageLocalizationParser.GetCurrentLanguageTags["ExitTerminalDescription"];

            bool showTestInput = bool.Parse(ConfigurationManager.AppSettings["ShowTestInput"] ?? "false");
            if (showTestInput)
                this.txtInputCode.BringToFront();
            else
                this.txtInputCode.SendToBack();

            #region Style Edits
            this.BackColor = ColorTranslator.FromHtml(DataFileParser.Styles["BackgroundColor"]);

            var fontColor = ColorTranslator.FromHtml(DataFileParser.Styles["FontColor"]);
            this.ForeColor = fontColor;
            this.btnPH.ForeColor = fontColor;

            Color headerBackColor = ColorTranslator.FromHtml(DataFileParser.Styles["HeaderBackgroundColor"]);
            this.lblHeader.BackColor = headerBackColor;
            this.lblInstructions.BackColor = headerBackColor;
            this.lblHeader2.BackColor = headerBackColor;
            this.lblChargingHours.BackColor = headerBackColor;
            this.lblInstructions2.BackColor = headerBackColor;
            this.lblInstructions3.BackColor = headerBackColor;
            this.lblStatusKO.BackColor = headerBackColor;

            Color fontHeaderColor = ColorTranslator.FromHtml(DataFileParser.Styles["HeaderFontColor"]);
            this.lblHeader.ForeColor = fontHeaderColor;
            this.lblInstructions.ForeColor = fontHeaderColor;
            this.lblHeader2.ForeColor = fontHeaderColor;
            this.lblChargingHours.ForeColor = fontHeaderColor;
            this.lblInstructions2.ForeColor = fontHeaderColor;
            this.lblInstructions3.ForeColor = fontHeaderColor;
            this.lblStatusKO.ForeColor = fontHeaderColor;
            //this.lblStatusRetry.ForeColor = fontHeaderColor;
            //this.lblStatusNotFound.ForeColor = fontHeaderColor;
            
            this.btnGoBack.BackColor = ColorTranslator.FromHtml(DataFileParser.Styles["BtnBackBackgroundColor"]);
            this.btnGoBack2.BackColor = ColorTranslator.FromHtml(DataFileParser.Styles["BtnBackBackgroundColor"]);
            this.btnGoBack3.BackColor = ColorTranslator.FromHtml(DataFileParser.Styles["BtnBackBackgroundColor"]);
            this.btnGoBack4.BackColor = ColorTranslator.FromHtml(DataFileParser.Styles["BtnBackBackgroundColor"]);
            this.btnGoBack.ForeColor = ColorTranslator.FromHtml(DataFileParser.Styles["BtnBackFontColor"]);
            this.btnGoBack2.ForeColor = ColorTranslator.FromHtml(DataFileParser.Styles["BtnBackFontColor"]);
            this.btnGoBack3.ForeColor = ColorTranslator.FromHtml(DataFileParser.Styles["BtnBackFontColor"]);
            this.btnGoBack4.ForeColor = ColorTranslator.FromHtml(DataFileParser.Styles["BtnBackFontColor"]);
            this.btnGoBack5.ForeColor = ColorTranslator.FromHtml(DataFileParser.Styles["BtnBackFontColor"]);
            this.btnGoBack.FlatAppearance.BorderColor = ColorTranslator.FromHtml(DataFileParser.Styles["BtnBackBorderColor"]);
            this.btnGoBack2.FlatAppearance.BorderColor = ColorTranslator.FromHtml(DataFileParser.Styles["BtnBackBorderColor"]);
            this.btnGoBack3.FlatAppearance.BorderColor = ColorTranslator.FromHtml(DataFileParser.Styles["BtnBackBorderColor"]);
            this.btnGoBack4.FlatAppearance.BorderColor = ColorTranslator.FromHtml(DataFileParser.Styles["BtnBackBorderColor"]);
            this.btnGoBack5.FlatAppearance.BorderColor = ColorTranslator.FromHtml(DataFileParser.Styles["BtnBackBorderColor"]);

            this.panelFoot.BackColor = ColorTranslator.FromHtml(DataFileParser.Styles["FootBackgroundColor"]);
            this.panelFoot.ForeColor = ColorTranslator.FromHtml(DataFileParser.Styles["FootFontColor"]);

            this.btnPH.BackgroundImage = (Image)RM.GetObject(DataFileParser.Styles["BtnPermitHolder"]);
            this.btnPH.Image = (Image)RM.GetObject(DataFileParser.Styles["BtnPermitHolder_Image"]);
            this.imgShowQRCode.BackgroundImage = (Image)RM.GetObject(DataFileParser.Styles["InitImgShowQR"]);
            this.imgBtnHelp.BackgroundImage = (Image)RM.GetObject(DataFileParser.Styles["ImgBtnHelp"]);

            if (headerVersion == "1")
            {
                this.tableHeader.Visible = true;
                this.tableHeader2.Visible = false;
            }
            else
            {
                this.tableHeader.Visible = false;
                this.tableHeader2.Visible = true;
            }

            if (footVersion == "1")
            {
                this.tableLayoutFoot.Visible = true;
                this.imgFootLogoPango.BackgroundImage = (Image)RM.GetObject(DataFileParser.Styles["FootLogoPango"]);
                this.imgFootYellowBtn.BackgroundImage = (Image)RM.GetObject(DataFileParser.Styles["FootBtn24h"]);
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
            this.lblInstructions.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4_Instructions"];
            this.lblHeader2.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4_Welcome"];
            this.lblChargingHours.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4_ChargingHours"];
            this.lblInstructions2.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4_Instructions"];
            this.lblInstructions3.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4_Instructions2"];
            this.btnPH.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4_Ticket_PH_BtnPermitHolder"];
            this.lblStatusKO.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4_EnterData"];
            this.lblStatusRetry.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4_Ticket_PH_NotFound"];
            this.lblRetryMsg.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4_Ticket_PH_RetryMsg"];
            this.lblPlate.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4_Ticket_PH_RetryPlate"];
            this.lblPhone.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4_Ticket_PH_RetryPhone"];
            this.lblStatusNotFound.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4_Ticket_PH_NotFound"];
            this.lblNotFoundMsg.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4_Ticket_PH_NotFoundMsg"];
            this.lblStatusPaymMethod.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4_Ticket_PH_PaymMethod"];
            this.lblStatusPangoPaymFail.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4_Ticket_PH_PangoPaymFail"];
            this.lblPangoPaymFailMsg.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4_Ticket_PH_PangoPaymFailMsg"];
            this.lblFootDownload.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4_DownloadApp"];
            this.btnGoBack.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4_btnBack"];
            this.btnGoBack2.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4_btnBack"];
            this.btnGoBack3.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4_btnBack"];
            this.btnGoBack4.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4_btnBack"];
            this.btnGoBack5.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4_btnBack"];
            btnPayByPlateClickText = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4_BtnPayPlateClick"];

            this.tlblTerminalId.Text = "Terminal: " + _terminalId + ". " + descTerminalType;
            this.tlblDate.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            this.lblCurrentDate.Text = DateTime.Now.ToString(dateTimeFormat);
            this.lblFootDate2.Text = DateTime.Now.ToString(dateTimeFormat);
            this.tlblStatusTerminal.Text = "";

            var readerComPort = ConfigurationManager.AppSettings["ReaderComPort"];
            var readerBauds = int.Parse(ConfigurationManager.AppSettings["ReaderBauds"] ?? "9600");
            Controller = TerminalController.GetInstance(int.Parse(_terminalId), readerComPort, readerBauds);

            #region Init event callbacks
            Controller.CodeWasReadEvent += Controller_CodeWasReadEvent;
            #endregion

            Controller.Start();
        }

        private void Controller_CodeWasReadEvent(object sender, BarcodeReadEventArgs e)
        {
            _log.Debug("Incoming barcode detected event.");

            if (
                   //(Controller.TerminalState == TerminalState.ReadingBarcode ||
                   //Controller.TerminalState == TerminalState.TicketApproved ||
                   //Controller.TerminalState == TerminalState.TicketDenied ||
                   //Controller.TerminalState == TerminalState.TicketNotFound ||
                   //Controller.TerminalState == TerminalState.PaymentApproved ||
                   //Controller.TerminalState == TerminalState.PaymentDenied ||
                   //Controller.TerminalState == TerminalState.PaymentFailed ||
                   //Controller.TerminalState == TerminalState.TicketEntry) &&
                    !CheckOpened("FormV4ShowQrCode") &&
                    !CheckOpened("FormV4ShowReceipt"))
            {
                _log.Debug("Showing FormV4ShowQrCode form");

                ActivateShowQrForm();
            }
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
                        else
                        {
                            _log.Error("SE QUEDA TOSTADO LG1");
                            if (!notificarFreezed)
                            {
                                notificarFreezed = true;
                                Utils.sendMail("CRITICAL ERROR LG1 FREEZED", "ERROR CRITICO, Máquina LG1 congelada, reinicio inmediato");
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

            Worker.Start();

            Thread _thread = new Thread(() =>
            {
                var interval = int.Parse(ConfigurationManager.AppSettings["PendingPaymentsInterval"] ?? "3000");
                while (true)
                {
                    var paymentAttempts = PaymentService.GetPendingPaymentAttemptsWithinTime(_terminalId);

                    if (paymentAttempts?.Any() == true)
                    {
                        var paymentAttempt = paymentAttempts.FirstOrDefault();

                        _log.Debug("There are pending payments detected and no transactions in progress, opening show receipt form...");
                        _log.Debug("IS VEHICLE PRESENT??? isVehiclePresent=" + Controller.IsVehiclePresent);
                        var form = new FormV4ShowReceipt(paymentAttempt);
                        form.FormClosed += FormVShowReceipt_FormClosed;
                        form.TopMost = true;

                        _log.Debug("Showing receipt form");
                        Application.Run(form);
                        _log.Debug("Showed receipt form");
                    }

                    Thread.Sleep(interval);
                }
            });

            _thread.SetApartmentState(ApartmentState.STA);
            _thread.Start();

            Thread _thread2 = new Thread(() =>
            {
                var interval = int.Parse(ConfigurationManager.AppSettings["PendingChangeScreenInterval"] ?? "3000");
                while (true)
                {
                    var pendingChanges = ChangeScreenService.GetPendingTemplateChangeScreen(_terminalId);

                    if (pendingChanges?.Any() == true)
                    {
                        var change = pendingChanges.FirstOrDefault();

                        _log.Debug("There are pending screen changes detected, opening form " + change.TemplateId + "...");
                        if (change.TemplateId == "pangoPassAccepted")
                        {
                            var form = new FormV4PangoPassOK(change);
                            form.FormClosed += FormVShowReceipt_FormClosed;
                            form.TopMost = true;

                            _log.Debug("Showing form");
                            Application.Run(form);
                        }
                        if (change.TemplateId == "freeOfCharge")
                        {
                            var form = new FormV4ShowReceipt(change);
                            form.FormClosed += FormVShowReceipt_FormClosed;
                            form.TopMost = true;

                            _log.Debug("Showing form");
                            Application.Run(form);
                        }
                    }

                    Thread.Sleep(interval);
                }
            });

            _thread2.SetApartmentState(ApartmentState.STA);
            _thread2.Start();
        }

        private bool CheckOpened(string name)
        {
            try
            {
                FormCollection fc = Application.OpenForms;

                foreach (Form frm in fc)
                {
                    _log.Debug("CheckOpened - " + name + ", openFormName:" + frm.Name + ", openFormText:" + frm.Text);
                    if (frm.Name == name)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                return false;
            }
        }

        private void FormVShowReceipt_FormClosed(object sender, FormClosedEventArgs e)
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

        private void ActivateShowQrForm()
        {
            try
            {
                if (this.InvokeRequired)
                {
                    this.Invoke((MethodInvoker)(() =>
                    {
                        openQRForm();
                    }));
                }
                else
                {
                    openQRForm();
                }
            }
            catch (Exception e)
            {
                _log.Error(e);
            }
        }

        private void openQRForm()
        {
            FormV4ShowQrCode = new FormV4ShowQrCode();

            var result = FormV4ShowQrCode.ShowDialog(this);

            _log.Debug("Result FormV4ShowQrCode: " + result.ToString());

            //FormV4ShowQrCode.Dispose();

            if (result == DialogResult.OK)
            {
                ActivateMainForm();
            }
        }

        private void ActivateMainForm()
        {
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)(() =>
                {
                    this.BringToFront();
                    this.Activate();
                    //this.TopMost = true;
                    this.WindowState = FormWindowState.Maximized;
                    MinimumSize = this.Size;
                    MaximumSize = this.Size;
                }));
            }
            else
            {
                this.BringToFront();
                this.Activate();
                //this.TopMost = true;
                this.WindowState = FormWindowState.Maximized;
                MinimumSize = this.Size;
                MaximumSize = this.Size;
            }
        }

        /// <summary>
        /// Shutdowns all devices and exit the terminal app
        /// </summary>
        private void Shutdown()
        {
            Program.Exit();
        }

        private void Form1_FormClosing_1(object sender, FormClosingEventArgs e)
        {
            var msgBoxResult = MessageBox.Show("ATTENTION. By closing this program, the terminal functionality will be shutdown. Are you sure to proceed?", "Terminal", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (msgBoxResult == DialogResult.Yes)
            {
                try
                {
                    // Quit app.
                    Shutdown();
                }
                catch (Exception ex)
                {
                    _log.Error(ex);
                }
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void btnPH_Click(object sender, EventArgs e)
        {
            var tempBtnText = this.btnPH.Text;
            this.btnPH.Text = btnPayByPlateClickText;
            this.btnPH.Enabled = false;

            Thread _thread0 = new Thread(() =>
            {
                var form = new FormLoading();
                form.FormClosed += FormVShowReceipt_FormClosed;
                form.TopMost = true;
                Application.Run(form);
            });
            _thread0.SetApartmentState(ApartmentState.STA);
            _thread0.Start();

            OpenKeyboard();

            this.btnPH.Enabled = true;
            this.btnPH.Text = tempBtnText;
        }

        private void btnGoBack_Click(object sender, EventArgs e)
        {
            _log.Debug("GoBack");
            panelInit.BringToFront();
            panelKO.Visible = false;
            panelRetry.Visible = false;
            panelNotFound.Visible = false;
            panelPaymMethod.Visible = false;
            panelPangoPaymFail.Visible = false;
            lblPlateValue.Text = "";
            lblPhoneValue.Text = "";
        }

        public void c_Registered(object sender, StatusEventArgs e)
        {
            _log.Debug("The " + (e.Type == 1 ? "Plate" : "Phone") + " was registered: " + e.Status);

            if (e.Status == "OK")
                processChargePlateResponse(e.chargeResponse);
        }

        public void c_PaymOptRegistered(object sender, StatusEventArgs e)
        {
            _log.Debug("The charge on " + (e.Type == 1 ? "PANGO" : "LG1") + " was registered: " + e.Status);

            if (e.Type == 1)
            {
                if (e.Status == "OK")
                {
                    var formV4 = new FormV4ShowReceipt(true);
                    var dialogResult = formV4.ShowDialog(this);

                    //if (dialogResult == DialogResult.Cancel)
                    //{
                    //    panelKO.Visible = false;
                    //    panelKO.SendToBack();
                    //}
                }
                else
                {
                    this.panelPangoPaymFail.Visible = true;
                    this.panelPangoPaymFail.BringToFront();

                    Thread _thread = new Thread(() =>
                    {
                        Thread.Sleep(showNotFoundTimeout);

                        if (this.IsDisposed)
                            return;

                        if (panelPangoPaymFail.InvokeRequired)
                        {
                            panelPangoPaymFail.Invoke((MethodInvoker)(() =>
                            {
                                this.panelPangoPaymFail.Visible = false;
                                this.panelPangoPaymFail.SendToBack();
                            }));
                        }
                    });
                    _thread.SetApartmentState(ApartmentState.STA);
                    _thread.Start();
                }
            }
            else
            {
                if (e.Status == "OK")
                {
                    var formV4 = new FormV4ShowReceipt(true);
                    var dialogResult = formV4.ShowDialog(this);
                }
                else
                {
                    Thread _thread = new Thread(() =>
                    {
                        var form = new FormLoading(showNotFoundTimeout);
                        form.FormClosed += FormVShowReceipt_FormClosed;
                        form.TopMost = true;
                        Application.Run(form);
                    });
                    _thread.SetApartmentState(ApartmentState.STA);
                    _thread.Start();
                }
            }

            if (!panelPaymMethod.IsDisposed)
            {
                //if (panelPaymMethod.InvokeRequired)
                //{
                    panelPaymMethod.Invoke((MethodInvoker)(() =>
                    {
                        this.panelPaymMethod.Visible = false;
                        this.panelPaymMethod.SendToBack();
                    }));
                //}
            }
        }

        private void OpenKeyboard()
        {
            var form = new FrmOnScreenKeyboardV4(2);
            var dialogResult = form.ShowDialog(this);

            if (dialogResult == DialogResult.OK)
            {
                if (string.IsNullOrEmpty(form.Plate))
                    return;

                Thread _thread0 = new Thread(() =>
                {
                    var formL = new FormLoading();
                    formL.FormClosed += FormVShowReceipt_FormClosed;
                    formL.TopMost = true;
                    Application.Run(formL);
                });
                _thread0.SetApartmentState(ApartmentState.STA);
                _thread0.Start();

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
                        var form2 = new FormV4PangoPassOK(true);
                        form2.ShowDialog(this);
                    }
                    else if (plateResponse.resultCode == 4 && plateResponse.whiteListId > 0)
                    {
                        this.paymentOptions.setWhiteList(plateResponse.whiteListId);
                        this.panelPaymMethod.Visible = true;
                        this.panelPaymMethod.BringToFront();

                        Thread _thread = new Thread(() =>
                        {
                            Thread.Sleep(showPaymOptionsTimeout);

                            if (this.IsDisposed)
                                return;

                            if (panelPaymMethod.InvokeRequired)
                            {
                                panelPaymMethod.Invoke((MethodInvoker)(() =>
                                {
                                    this.panelPaymMethod.Visible = false;
                                    this.panelPaymMethod.SendToBack();
                                }));
                            }
                        });
                        _thread.SetApartmentState(ApartmentState.STA);
                        _thread.Start();
                    }
                    else
                    {
                        _log.Error("error message: " + plateResponse.description);
                        OpenDigitKeyboard(form.Plate);
                    }
                }
                else
                    return;
            }
            else
            {
                _log.Warn("No plate was read from the user.");
                return;
            }
        }

        private void OpenDigitKeyboard(string plate)
        {
            var form = new ScreenDigitKeyboardV4(2);
            var dialogResult = form.ShowDialog(this);

            if (dialogResult == DialogResult.OK)
            {
                if (string.IsNullOrEmpty(form.Phone))
                    return;

                if (form.Phone.Length != 10)
                {
                    //labelError.Text = "Invalid Phone";
                    return;
                }

                Thread _thread0 = new Thread(() =>
                {
                    var formL = new FormLoading();
                    formL.FormClosed += FormVShowReceipt_FormClosed;
                    formL.TopMost = true;
                    Application.Run(formL);
                });
                _thread0.SetApartmentState(ApartmentState.STA);
                _thread0.Start();

                var (platePhoneRequestSent, platePhoneResponse) = PangoService.ChargePlatePhone(new ChargePlatePhoneRequest
                {
                    installationID = ConfigurationManager.AppSettings["InstallationId"],
                    terminalId = ConfigurationManager.AppSettings["TerminalId"],
                    plate = plate,
                    phone = form.Phone
                });

                _log.Debug("Phone request sent: " + (platePhoneRequestSent ? "OK" : "NOT OK"));

                if (platePhoneRequestSent)
                {
                    if (platePhoneResponse.templateId == "payAtExitMaxAmount" || platePhoneResponse.templateId == "payAtExit")
                        processChargePlateResponse(platePhoneResponse);
                    else if (platePhoneResponse.templateId == "transientNotFoundRetry")
                    {
                        lblPlateValue.Text = plate;
                        lblPhoneValue.Text = form.Phone;
                        panelRetry.BringToFront();
                        panelRetry.Visible = true;
                        panelNotFound.Visible = false;
                        panelKO.Visible = false;

                        Thread _thread = new Thread(() =>
                        {
                            Thread.Sleep(showNotFoundTimeout);

                            if (this.IsDisposed)
                                return;

                            if (panelRetry.InvokeRequired)
                            {
                                panelRetry.Invoke((MethodInvoker)(() =>
                                {
                                    this.panelRetry.Visible = false;
                                    this.panelRetry.SendToBack();
                                }));
                            }
                        });
                        _thread.SetApartmentState(ApartmentState.STA);
                        _thread.Start();
                    }
                    else if (platePhoneResponse.templateId == "transientNotFound")
                    {
                        panelNotFound.BringToFront();
                        panelNotFound.Visible = true;
                        panelRetry.Visible = false;
                        panelKO.Visible = false;
                        lblPlateValue.Text = "";
                        lblPhoneValue.Text = "";

                        Thread _thread = new Thread(() =>
                        {
                            Thread.Sleep(showNotFoundTimeout);

                            if (this.IsDisposed)
                                return;

                            if (panelNotFound.InvokeRequired)
                            {
                                panelNotFound.Invoke((MethodInvoker)(() =>
                                {
                                    this.panelNotFound.Visible = false;
                                    this.panelNotFound.SendToBack();
                                }));
                            }
                        });
                        _thread.SetApartmentState(ApartmentState.STA);
                        _thread.Start();
                    }
                    else if (platePhoneResponse.templateId == "payAtExitByPangoPass")
                    {
                        var form2 = new FormV4PangoPassOK(false);
                        form2.ShowDialog(this);
                    }
                    else
                    {
                        _log.Error("Invalid template ID: " + platePhoneResponse.templateId);
                        return;
                    }
                }
                else
                    return;
            }
            else
            {
                _log.Warn("No phone was read from the user.");
                return;
            }
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            bool showTestInput = bool.Parse(ConfigurationManager.AppSettings["ShowTestInput"] ?? "false");
            if (e.KeyChar == 13)
            {
                FormV4_Ticket_PH.Controller.ReadInputValue = true;
                if (showTestInput)
                    FormV4_Ticket_PH.Controller.InputValue = this.txtInputCode.Text;
                else
                    _log.Debug(FormV4_Ticket_PH.Controller.InputValue + " ready for querying");
            }
            else
            {
                if (FormV4_Ticket_PH.Controller.InputValue == null)
                    FormV4_Ticket_PH.Controller.InputValue = "";

                FormV4_Ticket_PH.Controller.InputValue += e.KeyChar;
            }
        }

        private void txtInputCode_Leave(object sender, EventArgs e)
        {
            this.txtInputCode.Focus();
        }

        private void processChargePlateResponse(ChargePlateResponse paymentData)
        {
            if (paymentData != null && paymentData.paramList.Any() && paymentData.amount.HasValue)
            {
                if (paymentData.amount.Value > 0M)
                {
                    var parkingFee = paymentData.paramList.FirstOrDefault(x => x.param_name == "parkingFee")?.param_value ?? "";
                    var convFee = paymentData.paramList.FirstOrDefault(x => x.param_name == "convFee")?.param_value ?? "";
                    var entryDate = paymentData.paramList.FirstOrDefault(x => x.param_name == "entryDate")?.param_value ?? "";
                    var entryTime = paymentData.paramList.FirstOrDefault(x => x.param_name == "entryTime")?.param_value ?? "";
                    var convFeePct = paymentData.paramList.FirstOrDefault(x => x.param_name == "convFeePercent")?.param_value ?? "";
                    var showTime = int.Parse(paymentData.paramList.FirstOrDefault(x => x.param_name == "showTime")?.param_value ?? "40");
                    var priceDetail = paymentData.paramList.FirstOrDefault(x => x.param_name == "priceDetail")?.param_value ?? "";

                    PaymentService.UpsertPaymentAttempt(new Model.PaymentAttempt
                    {
                        AmountToCharge = paymentData.amount.Value,
                        Currency = "USD",
                        InstallationId = paymentData.installationId,
                        TerminalId = paymentData.terminalId,
                        Date = DateTime.Now,
                        Plate = paymentData.plate,
                        Phone = paymentData.phone,
                        Processed = false,
                        TransientId = paymentData.transientId,
                        EntryDate = entryDate,
                        EntryTime = entryTime,
                        ParkingFee = parkingFee,
                        ShowTime = showTime,
                        TransactionFee = convFee,
                        TransactionPercentage = convFeePct,
                        PriceDetail = priceDetail
                    });
                }
                else
                {
                    var formV4 = new FormV4ShowReceipt(paymentData);
                    var dialogResult = formV4.ShowDialog(this);

                    if (dialogResult == DialogResult.Cancel)
                    {
                        panelKO.Visible = false;
                        panelKO.SendToBack();
                    }
                }
            }
        }
    }
}