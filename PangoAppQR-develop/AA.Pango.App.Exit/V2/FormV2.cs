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
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace AA.Pango.App
{
    public partial class FormV2 : Form
    {
        protected ILog _log = LogManager.GetLogger("FormV2");
        CultureInfo deC = new CultureInfo("en-US");
        public static volatile TerminalController Controller;
        private Thread Worker { get; set; }

        private static string _terminalId = ConfigurationManager.AppSettings["TerminalId"];
        private static string _installationId = ConfigurationManager.AppSettings["InstallationId"];

        private FormV2ShowQrCode FormV2ShowQrCode { get; set; }

        public FormV2()
        {
            InitializeComponent();

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

            var eventNotificationTypeToSend = ConfigurationManager.AppSettings["EventNotificationTypeToSend"] ?? "Entry";

            var imagePath = ConfigurationManager.AppSettings["BackgroundImage"];

            if (!string.IsNullOrEmpty(imagePath))
            {
                try
                {
                    this.pictureBox1.Image = Image.FromStream(new StreamReader(imagePath).BaseStream);
                }
                catch (Exception e)
                {
                    _log.Error("Error when setting the form background image", e);
                }
            }

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

            this.tlblTerminalId.Text = "Terminal: " + _terminalId + ". " + descTerminalType;
            this.tlblDate.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            this.lblCurrentDate.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

            this.tlblStatusTerminal.Text = "";
            var readerComPort = ConfigurationManager.AppSettings["ReaderComPort"];
            var readerBauds = int.Parse(ConfigurationManager.AppSettings["ReaderBauds"] ?? "9600");

            int terminalId = int.Parse(ConfigurationManager.AppSettings["TerminalId"]);
            Controller = TerminalController.GetInstance(terminalId, readerComPort, readerBauds);
            Controller.Start();

        }

        private void InitWorker()
        {
            _log.Debug("Starting Worker...");
            var terminalId = ConfigurationManager.AppSettings["TerminalId"];

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

                        Thread.Sleep(500);
                    }
                    catch (Exception e)
                    {
                        _log.Error("Unexpected error in Worker thread", e);
                    }

                }
            })));

            Worker.Start();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Init();

            Thread _thread = new Thread(() =>
            {
                while (true)
                {

                    var paymentAttempts = PaymentService.GetPendingPaymentAttemptsWithinTime(_terminalId);

                    if (paymentAttempts?.Any() == true)
                    {

                        var paymentAttempt = paymentAttempts.FirstOrDefault();

                        _log.Debug("There are pending payments detected and no transactions in progress, opening show receipt form...");
                        var form = new FormV2ShowReceipt(paymentAttempt);
                        form.FormClosed += FormVShowReceipt_FormClosed;
                        form.TopMost = true;

                        _log.Debug("Showing receipt form");
                        Application.Run(form);
                        _log.Debug("Showed receipt form");
                    }

                    Thread.Sleep(1500);

                }
            });

            _thread.SetApartmentState(ApartmentState.STA);
            _thread.Start();


            Thread _thread2 = new Thread(() =>
            {
                while (true)
                {

                    //Thread.Sleep(800);
                    //_log.Debug("Controller State: " + Controller.TerminalState.ToString());
                    if (
                        (
                        Controller.TerminalState == TerminalState.ReadingBarcode ||
                        Controller.TerminalState == TerminalState.TicketApproved ||
                        Controller.TerminalState == TerminalState.TicketDenied ||
                        Controller.TerminalState == TerminalState.TicketNotFound
                            )
                            && !CheckOpened("FormV2ShowQrCode"))
                    {

                        _log.Debug("Showing FormV2ShowQrCode form");

                        ActivateShowQrForm();

                    }

                }
            });
            _thread2.SetApartmentState(ApartmentState.STA);
            _thread2.Start();

        }

        private bool CheckOpened(string name)
        {
            FormCollection fc = Application.OpenForms;

            foreach (Form frm in fc)
            {
                if (frm.Text == name)
                {
                    return true;
                }
            }
            return false;
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
            FormV2ShowQrCode = new FormV2ShowQrCode();

            var result = FormV2ShowQrCode.ShowDialog(this);

            _log.Debug("Result FormV2ShowQrCode: " + result.ToString());

            //FormV2ShowQrCode.Dispose();

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

        private void FormVShowQRCode_FormClosed(object sender, FormClosedEventArgs e)
        {

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

        private void button2_Click(object sender, EventArgs e)
        {
            var form = new FormV2ShowTicket();
            form.ShowDialog(this);
        }

        private void btnOption1_Click(object sender, EventArgs e)
        {
            ActivateShowQrForm();
        }


        private void button3_Click(object sender, EventArgs e)
        {

            var tempBtnText = this.btnPayByPlate.Text;
            this.btnPayByPlate.Text = "PLEASE WAIT...";
            this.btnPayByPlate.Enabled = false;

            var (responseSucceeded, checkCCResponse) = PangoService.CheckCCTransient(
                    new ServiceLayer.ApiDtos.CheckCCTransientRequest
                    {
                        installationID = ConfigurationManager.AppSettings["InstallationId"],
                        terminalId = ConfigurationManager.AppSettings["TerminalId"],
                    });

            if (responseSucceeded)
            {
                _log.Debug("Check CC response received successfully");

                try
                {
                    if (checkCCResponse?.plateNotReadByLPR == true)
                    {

                        if (checkCCResponse.templateId == "plateNotReadByLPROut")
                        {
                            var form = new FrmOnScreenKeyboard();

                            var dialogResult = form.ShowDialog(this);

                            if (dialogResult == DialogResult.OK)
                            {
                                var plate = form.Plate;

                                if (string.IsNullOrEmpty(plate))
                                {
                                    return;
                                }

                                var (plateRequestSent, plateResponse) = PangoService.ChargePlate(new ChargePlateRequest
                                {
                                    Amount = 0M,
                                    Currency = "USD",
                                    InstallationID = _installationId,
                                    TerminalId = _terminalId,
                                    Plate = plate,
                                });

                                _log.Debug("Plate request sent: " + (plateRequestSent ? "OK" : "NOT OK"));

                                _log.Debug("Template ID received: " + plateResponse.templateId);

                                if (plateResponse.templateId == "payAtExitMaxAmount" || plateResponse.templateId == "payAtExit")
                                {
                                    var formV2 = new FormV2ShowReceipt(plateResponse);
                                    formV2.ShowDialog(this);
                                }
                                else
                                {
                                    _log.Error("Invalid template ID: " + checkCCResponse.templateId);
                                }

                            }
                            else
                            {
                                _log.Warn("No plate was read from the user.");
                            }

                        }
                        else if (checkCCResponse.templateId == "payAtExitMaxAmount")
                        {
                            var form = new FormV2ShowReceipt(checkCCResponse);
                            form.ShowDialog(this);
                        }
                        else
                        {
                            _log.Error("Invalid template ID: " + checkCCResponse.templateId);
                        }


                    }
                    else
                    {

                        if (checkCCResponse?.templateId == "payAtExit")
                        {
                            var form = new FormV2ShowReceipt(checkCCResponse);
                            form.ShowDialog(this);
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

        private void btnShowQrCode2_Click(object sender, EventArgs e)
        {
            var form = new FormV2ShowQrCode();
            form.ShowDialog(this);
        }
    }
}