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
    public partial class FormV4_3opt : Form
    {
        protected ILog _log = LogManager.GetLogger("FormV4_3opt");
        CultureInfo deC = new CultureInfo("en-US");
        public static volatile TerminalController Controller;
        private Thread Worker { get; set; }
        private System.Resources.ResourceManager RM = new System.Resources.ResourceManager("AA.Pango.App.Exit.Properties.Resources", typeof(Resources).Assembly);

        private static string _terminalId = ConfigurationManager.AppSettings["TerminalId"];
        private static string _installationId = ConfigurationManager.AppSettings["InstallationId"];
        private static string dateTimeFormat = ConfigurationManager.AppSettings["DateTimeFormat"];

        private int showPlatePhoneTimeout = int.Parse(ConfigurationManager.AppSettings["showPlatePhoneTimeout"] ?? "20000");
        private static string headerVersion = ConfigurationManager.AppSettings["HeaderVersion"];
        private static string footVersion = ConfigurationManager.AppSettings["FootVersion"];
        private static bool notificarFreezed = false;
        private static string btnPayByPlateClickText;

        private FormV4ShowQrCode FormV4ShowQrCode { get; set; }

        public FormV4_3opt()
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
            this.lblInitMsg.ForeColor = fontColor;
            this.btnShowQrCode.ForeColor = fontColor;
            this.btnScanTicket.ForeColor = fontColor;
            this.btnPayByPlate.ForeColor = fontColor;

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

            this.btnGoBack.BackColor = ColorTranslator.FromHtml(DataFileParser.Styles["BtnBackBackgroundColor"]);
            this.btnGoBack.ForeColor = ColorTranslator.FromHtml(DataFileParser.Styles["BtnBackFontColor"]);
            this.btnGoBack.FlatAppearance.BorderColor = ColorTranslator.FromHtml(DataFileParser.Styles["BtnBackBorderColor"]);

            this.panelFoot.BackColor = ColorTranslator.FromHtml(DataFileParser.Styles["FootBackgroundColor"]);
            this.panelFoot.ForeColor = ColorTranslator.FromHtml(DataFileParser.Styles["FootFontColor"]);

            this.btnShowQrCode.BackgroundImage = (Image)RM.GetObject(DataFileParser.Styles["BtnScanQR"]);
            this.btnPayByPlate.BackgroundImage = (Image)RM.GetObject(DataFileParser.Styles["BtnPayPlate"]);
            this.btnPayByPlate.Image = (Image)RM.GetObject(DataFileParser.Styles["BtnPayPlate_Image"]);
            this.btnScanTicket.BackgroundImage = (Image)RM.GetObject(DataFileParser.Styles["BtnScantTicket"]);
            this.pictureBox1.BackgroundImage = (Image)RM.GetObject(DataFileParser.Styles["InitOrImage"]);
            this.pictureBox2.BackgroundImage = (Image)RM.GetObject(DataFileParser.Styles["InitOrImage"]);

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
            this.lblInitMsg.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4_InitMsg"];
            this.btnShowQrCode.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4_3opt_BtnShowQR"];
            this.btnScanTicket.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4_3opt_BtnScanTicket"]; 
            this.btnPayByPlate.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4_3opt_BtnPayPlate"];
            this.lblStatusKO.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4_EnterData"];
            this.lblFootDownload.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4_DownloadApp"];
            this.btnGoBack.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4_btnBack"];
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

        private void btnScanTicket_Click(object sender, EventArgs e)
        {
            var form = new FormV4ShowTicket();
            form.ShowDialog(this);
        }

        private void btnPayByPlate_Click(object sender, EventArgs e)
        {
            var tempBtnText = this.btnPayByPlate.Text;
            this.btnPayByPlate.Text = btnPayByPlateClickText;
            this.btnPayByPlate.Enabled = false;

            var (responseSucceeded, checkCCResponse) = PangoService.CheckCCTransient(
                new CheckCCTransientRequest
                {
                    installationID = _installationId,
                    terminalId = _terminalId,
                });

            if (responseSucceeded)
            {
                _log.Debug("Check CC response received successfully");

                try
                {
                    if (checkCCResponse?.plateNotReadByLPR == true)
                    {
                        if (checkCCResponse?.templateId == "plateNotReadByLPROut")
                        {
                            this.panelKO.Visible = true;
                            this.panelKO.BringToFront();

                            Thread _thread = new Thread(() =>
                            {
                                Thread.Sleep(showPlatePhoneTimeout);

                                if (this.IsDisposed)
                                    return;

                                if (panelKO.InvokeRequired)
                                {
                                    panelKO.Invoke((MethodInvoker)(() =>
                                    {
                                        this.panelKO.Visible = false;
                                        this.panelKO.SendToBack();
                                    }));
                                }
                            });
                            _thread.SetApartmentState(ApartmentState.STA);
                            _thread.Start();
                        }
                        else if (checkCCResponse?.templateId == "payAtExitMaxAmount")
                        {
                            _log.Debug("IS VEHICLE PRESENT??? isVehiclePresent=" + Controller.IsVehiclePresent);
                            processCheckCCTransientResponse(checkCCResponse);
                        }
                        else
                        {
                            _log.Error("Invalid template ID: " + checkCCResponse?.templateId);
                        }
                    }
                    else
                    {
                        if (checkCCResponse?.templateId == "payAtExit")
                        {
                            _log.Debug("IS VEHICLE PRESENT??? isVehiclePresent=" + Controller.IsVehiclePresent);
                            processCheckCCTransientResponse(checkCCResponse);
                        }
                        else
                        {
                            _log.Error("Invalid template ID: " + checkCCResponse?.templateId);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _log.Error(ex);
                }
            }

            this.btnPayByPlate.Enabled = true;
            this.btnPayByPlate.Text = tempBtnText;
        }

        private void btnShowQrCode_Click(object sender, EventArgs e)
        {
            ActivateShowQrForm();
        }

        private void btnGoBack_Click(object sender, EventArgs e)
        {
            _log.Debug("GoBack");
            panelInit.BringToFront();
            panelKO.Visible = false;
        }

        public void c_Registered(object sender, StatusEventArgs e)
        {
            _log.Debug("The " + (e.Type == 1 ? "Plate" : "Phone") + " was registered: " + e.Status);

            if (e.Status == "OK")
                processChargePlateResponse(e.chargeResponse);
        }
        
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            bool showTestInput = bool.Parse(ConfigurationManager.AppSettings["ShowTestInput"] ?? "false");
            if (e.KeyChar == 13)
            {
                FormV4_3opt.Controller.ReadInputValue = true;
                if (showTestInput)
                    FormV4_3opt.Controller.InputValue = this.txtInputCode.Text;
                else
                    _log.Debug(FormV4_3opt.Controller.InputValue + " ready for querying");
            }
            else
            {
                if (FormV4_3opt.Controller.InputValue == null)
                    FormV4_3opt.Controller.InputValue = "";

                FormV4_3opt.Controller.InputValue += e.KeyChar;
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

        private void processCheckCCTransientResponse(CheckCCTransientResponse paymentData)
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
                        InstallationId = paymentData.installationID,
                        TerminalId = paymentData.terminalId,
                        Date = DateTime.Now,
                        Plate = paymentData.plate,
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
                }
            }
        }
    }
}