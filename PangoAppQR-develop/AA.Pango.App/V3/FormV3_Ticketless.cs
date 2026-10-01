using AA.Pango.App.V3;
using AA.Pango.ServiceLayer;
using AA.Pango.ServiceLayer.Controllers;
using AA.Pango.ServiceLayer.Helpers;
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
    public partial class FormV3_Ticketless : Form
    {
        protected ILog _log = LogManager.GetLogger("FormV3_Ticketless");
        CultureInfo deC = new CultureInfo("en-US");
        public static volatile TerminalController EntryController;
        private Thread Worker { get; set; }

        private static string _terminalId = ConfigurationManager.AppSettings["TerminalId"];
        private static string _installationId = ConfigurationManager.AppSettings["InstallationId"];
        private static string dateTimeFormat = ConfigurationManager.AppSettings["DateTimeFormat"];

        private static bool showServiceButtons = bool.Parse(ConfigurationManager.AppSettings["ShowServiceButtons"] ?? "false");

        private static bool notificarFreezed = false;

        private FormV3ShowQrCode FormV3ShowQrCode { get; set; }

        public FormV3_Ticketless()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
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
            _log.Debug("Init settings...");

            this.Activate();
            this.WindowState = FormWindowState.Maximized;
            MinimumSize = this.Size;
            MaximumSize = this.Size;

            bool showTestInput = bool.Parse(ConfigurationManager.AppSettings["ShowTestInput"] ?? "false");
            if (showTestInput)
            {
                this.txtInputCode.BringToFront();
            }
            else
            {
                this.txtInputCode.SendToBack();
            }

            LanguageLocalizationParser.LoadLocalizations();

            var eventNotificationTypeToSend = ConfigurationManager.AppSettings["EventNotificationTypeToSend"] ?? "Entry";

            var descTerminalType = eventNotificationTypeToSend == "Entry" ?
                LanguageLocalizationParser.GetCurrentLanguageTags["EntryTerminalDescription"] :
                LanguageLocalizationParser.GetCurrentLanguageTags["ExitTerminalDescription"];

            this.toolStripDropDownButton1.Visible = showServiceButtons;

            #region Style Edits

            var backgroundColor = LanguageLocalizationParser.GetCurrentLanguageTags["BackgroundColor"] ?? "Cyan";
            var statusFontSize = float.Parse(LanguageLocalizationParser.GetCurrentLanguageTags["FontSize"] ?? "16");
            var fontFamily = LanguageLocalizationParser.GetCurrentLanguageTags["FontFamily"] ?? "Arial";
            var fontColor = LanguageLocalizationParser.GetCurrentLanguageTags["FontColor"] ?? "Black";

            Color color = System.Drawing.ColorTranslator.FromHtml(backgroundColor);
            this.BackColor = color;

            #endregion Style Edits

            this.lblHeader.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV3_Welcome"];
            this.lblInstructions.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV3_Instructions"];
            //this.lblFootDownload.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV3_DownloadApp"];
            //this.lblFootPrice.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV3_Price"];

            this.lblTicketless.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV3_Ticketless"];
            this.lblStart.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV3_Ticketless_Start"];
            this.lblPayAtExit.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV3_Ticketless_PayAtExit"];

            this.tlblTerminalId.Text = "Terminal: " + _terminalId + ". " + descTerminalType;
            this.tlblDate.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            //this.lblCurrentDate.Text = DateTime.Now.ToString(dateTimeFormat);
            this.tlblStatusTerminal.Text = "";

            var readerComPort = ConfigurationManager.AppSettings["ReaderComPort"];
            var readerBauds = int.Parse(ConfigurationManager.AppSettings["ReaderBauds"] ?? "9600");
            EntryController = TerminalController.GetInstance(int.Parse(_terminalId), readerComPort, readerBauds);

            #region Init event callbacks
            EntryController.CodeWasReadEvent += Controller_CodeWasReadEvent;
            #endregion

            EntryController.Start();
        }

        private void Controller_CodeWasReadEvent(object sender, BarcodeReadEventArgs e)
        {
            _log.Debug("Incoming barcode detected event.");

            if (
                //(EntryController?.TerminalState == TerminalState.ReadingBarcode ||
                //         EntryController?.TerminalState == TerminalState.TicketApproved ||
                //         EntryController?.TerminalState == TerminalState.TicketDenied ||
                //         EntryController?.TerminalState == TerminalState.TicketNotFound) &&
                !CheckOpened("FormV3ShowQrCode"))
            {
                _log.Debug("Showing FormV3ShowQrCode form");

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
                            _log.Error("SE QUEDA TOSTADO N42");
                            if (!notificarFreezed)
                            {
                                notificarFreezed = true;
                                Utils.sendMail("CRITICAL ERROR N42 FREEZED", "ERROR CRITICO, Máquina N42 congelada, reinicio inmediato");
                            }
                        }

                        //if (!lblCurrentDate.IsDisposed)
                        //{
                        //    if (lblCurrentDate.InvokeRequired)
                        //    {
                        //        lblCurrentDate.Invoke((MethodInvoker)(() =>
                        //        {
                        //            lblCurrentDate.Text = DateTime.Now.ToString(dateTimeFormat, deC);
                        //        }));
                        //    }
                        //    else
                        //    {
                        //        lblCurrentDate.Text = DateTime.Now.ToString(dateTimeFormat, deC);
                        //    }
                        //}
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

            Thread _thread = new Thread(() =>
            {
                var interval = int.Parse(ConfigurationManager.AppSettings["PendingChangeScreenInterval"] ?? "3000");
                while (true)
                {
                    var pendingChanges = ChangeScreenService.GetPendingTemplateChangeScreen(_terminalId);

                    if (pendingChanges?.Any() == true)
                    {
                        var change = pendingChanges.FirstOrDefault();

                        _log.Debug("There are pending screen changes detected, opening form " + change?.TemplateId + "...");
                        if (change?.TemplateId == "pangoPassAccepted")
                        {
                            var form = new FormV3PangoPassOK(change);
                            form.FormClosed += NewForm_FormClosed;
                            form.TopMost = true;

                            _log.Debug("Showing form");
                            Application.Run(form);
                        }
                    }

                    Thread.Sleep(interval);
                }
            });

            _thread.SetApartmentState(ApartmentState.STA);
            _thread.Start();
        }

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

        private void buttonStart_Click(object sender, EventArgs e)
        {
            var form = new FormV3();

            form.ShowDialog(this);
        }

        private bool CheckOpened(string name)
        {
            try
            {
                FormCollection fc = Application.OpenForms;

                foreach (Form frm in fc)
                {
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
            FormV3ShowQrCode = new FormV3ShowQrCode();

            var result = FormV3ShowQrCode.ShowDialog(this);

            _log.Debug("Result FormV3ShowQrCode: " + result.ToString());

            //FormV3ShowQrCode.Dispose();

            if (result == DialogResult.OK)
            {
                ActivateMainForm();
            }
        }

        private void ActivateMainForm()
        {
            try
            {
                //FormV3ShowQrCode.Hidden = false;
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
            catch (Exception e)
            {
                _log.Error(e);
            }
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            bool showTestInput = bool.Parse(ConfigurationManager.AppSettings["ShowTestInput"] ?? "false");
            if (e.KeyChar == 13)
            {
                EntryController.ReadInputValue = true;
                if (showTestInput)
                {
                    EntryController.InputValue = this.txtInputCode.Text;
                }
                else
                {
                    _log.Debug(EntryController.InputValue + " ready for querying");
                }
            }
            else
            {
                if (EntryController.InputValue == null)
                {
                    EntryController.InputValue = "";
                }

                EntryController.InputValue += e.KeyChar;
            }
        }

        private void txtInputCode_Leave(object sender, EventArgs e)
        {
            this.txtInputCode.Focus();
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            _log.Debug("IS VEHICLE PRESENT??? isVehiclePresent=" + EntryController.IsVehiclePresent);
            var form = new FormV3PayByPlate();
            form.ShowDialog(this);
        }
    }
}
