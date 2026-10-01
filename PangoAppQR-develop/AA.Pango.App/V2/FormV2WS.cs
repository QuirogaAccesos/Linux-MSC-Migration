using AA.Pango.ServiceLayer.Controllers;
using AA.Pango.TestWinApp.Utils;
using log4net;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace AA.Pango.App
{
    public partial class FormV2WS : Form
    {
        protected ILog _log = LogManager.GetLogger("FormV2WS");
        CultureInfo deC = new CultureInfo("en-US");
        public static volatile TerminalController EntryController;
        private Thread Worker { get; set; }

        private static string _terminalId = ConfigurationManager.AppSettings["TerminalId"];
        private static string _installationId = ConfigurationManager.AppSettings["InstallationId"];

        private static bool showServiceButtons = bool.Parse(ConfigurationManager.AppSettings["ShowServiceButtons"] ?? "false");

        private FormV2ShowQrCode FormV2ShowQrCode { get; set; }

        public FormV2WS()
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

            int terminalId = int.Parse(ConfigurationManager.AppSettings["TerminalId"]);
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
                    _log.Warn("Form background image not set", e);
                }
            }

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

            this.tlblTerminalId.Text = "Terminal: " + terminalId + ". " + descTerminalType;
            this.tlblDate.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            this.lblCurrentDate.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

            this.tlblStatusTerminal.Text = "";
            var readerComPort = ConfigurationManager.AppSettings["ReaderComPort"];
            var readerBauds = int.Parse(ConfigurationManager.AppSettings["ReaderBauds"] ?? "9600");

            EntryController = TerminalController.GetInstance(terminalId, readerComPort, readerBauds);
            EntryController.Start();

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

                        if (
                            (
                            EntryController?.TerminalState == TerminalState.ReadingBarcode ||
                            EntryController?.TerminalState == TerminalState.TicketApproved ||
                            EntryController?.TerminalState == TerminalState.TicketDenied ||
                            EntryController?.TerminalState == TerminalState.TicketNotFound
                                )
                                && !CheckOpened("FormV2ShowQrCode"))
                        {

                            //if (FormV2ShowQrCode?.Hidden == false)
                            // {
                            _log.Debug("Showing FormV2ShowQrCode form");

                            ActivateShowQrForm();
                            //}



                        }

                    }
                    catch (Exception e)
                    {
                        _log.Error("Unexpected error in Worker thread", e);
                    }

                }
            })));
            Worker.SetApartmentState(ApartmentState.STA);
            Worker.Start();
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

        private void pictureBox1_Click(object sender, EventArgs e)
        {
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Init();
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
            var form = new FormV2PullTicket();
            form.ShowDialog(this);
        }

        private void btnOption1_Click(object sender, EventArgs e)
        {
            ActivateShowQrForm();
        }

        private void ActivateShowQrForm()
        {
            FormV2ShowQrCode = new FormV2ShowQrCode();

            var result = FormV2ShowQrCode.ShowDialog(this);

            _log.Debug("Result FormV2ShowQrCode: " + result.ToString());

            if (result == DialogResult.OK)
            {
                ActivateMainForm();
            }
        }

        private void ActivateMainForm()
        {
            try
            {

                //FormV2ShowQrCode.Hidden = false;

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

        private void button3_Click(object sender, EventArgs e)
        {
            var form = new FormV2PayByPlate();
            form.ShowDialog(this);
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            var form = new FormV2PayByPlate();
            form.ShowDialog(this);
        }
    }
}