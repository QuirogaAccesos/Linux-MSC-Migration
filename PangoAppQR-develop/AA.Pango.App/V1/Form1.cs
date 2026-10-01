using AA.Pango.ServiceLayer;
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
    public partial class Form1 : Form
    {
        protected ILog _log = LogManager.GetLogger("Form1");
        CultureInfo deC = new CultureInfo("en-US");
        public TerminalController EntryController { get; set; }
        private Thread Worker { get; set; }

        public Form1()
        {
            InitializeComponent();
        }

        private void Init()
        {
            InitSettings();
            SetVersion();
            InitWorker();
            SignalVehiclePresenceToApi();
        }

        private void SignalVehiclePresenceToApi()
        {
            new Thread(new ThreadStart(() =>
            {

                var (responseSucceeded, checkCCResponse) = PangoService.CheckCCTransient(
                   new ServiceLayer.ApiDtos.CheckCCTransientRequest
                   {
                       installationID = ConfigurationManager.AppSettings["InstallationId"],
                       terminalId = ConfigurationManager.AppSettings["TerminalId"],
                   });

                if (responseSucceeded)
                {
                    _log.Debug("Check CC request sent to API successfully");
                }

            })).Start();

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

            if (showTestInput)
            {
                this.txtInputCode.BringToFront();
            }
            else
            {
                this.txtInputCode.SendToBack();
            }

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
            this.lblStatus.Font = new System.Drawing.Font(fontFamily, statusFontSize);

            this.lblStatus.ForeColor = System.Drawing.ColorTranslator.FromHtml(fontColor);

            #endregion Style Edits

            this.tlblTerminalId.Text = "Terminal: " + terminalId + ". " + descTerminalType;
            this.tlblDate.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            this.dateBlue.Text = DateTime.Now.ToString("dd/MM/yyyy");
            this.hourBlue.Text = DateTime.Now.ToString("HH:mm:ss");
            this.tlblStatusTerminal.Text = "";
            var readerComPort = ConfigurationManager.AppSettings["ReaderComPort"];
            var readerBauds = int.Parse(ConfigurationManager.AppSettings["ReaderBauds"] ?? "9600");

            _log.Debug("Starting Controller...");

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

                    if (statusStrip1.InvokeRequired)
                    {
                        statusStrip1.Invoke((MethodInvoker)(() =>
                        {
                            statusStrip1.Items["tlblDate"].Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                            dateBlue.Text = DateTime.Now.ToString("dddd, MMMM dd, yyyy", deC);
                            hourBlue.Text = DateTime.Now.ToString("HH:mm:ss");
                        }));
                    }

                    #region Entry Controller Actions

                    if (EntryController != null)
                    {
                        if ((EntryController.TerminalState == TerminalState.ReadingBarcode))
                        {
                            lblStatus.Invoke((MethodInvoker)(() =>
                            {
                                lblStatus.Visible = true;
                                lblStatus.Text = LanguageLocalizationParser.GetCurrentLanguageTags["Entry-IssuingVisitorCard"];
                            }));
                        }
                        else if ((EntryController.TerminalState == TerminalState.VehiclePresent))
                        {
                            lblStatus.Invoke((MethodInvoker)(() =>
                            {
                                lblStatus.Visible = true;
                                lblStatus.Text = LanguageLocalizationParser.GetCurrentLanguageTags["Entry-IssuingVisitorCard"];
                            }));
                        }
                        else if ((EntryController.TerminalState == TerminalState.TicketDenied || EntryController.TerminalState == TerminalState.TicketNotFound))
                        {
                            lblStatus.Invoke((MethodInvoker)(() =>
                            {
                                lblStatus.Visible = true;
                                lblStatus.Text = LanguageLocalizationParser.GetCurrentLanguageTags["Entry-RecurrentClientCardDenied"];
                            }));
                        }
                        else if (EntryController.TerminalState == TerminalState.StandBy)
                        {
                            lblStatus.Invoke((MethodInvoker)(() =>
                            {
                                lblStatus.Visible = true;
                                DateTime timeOfDayGreeting = DateTime.Now;
                                lblStatus.Text = LanguageLocalizationParser.GetCurrentLanguageTags["Hello"];

                                if (timeOfDayGreeting.Hour >= 5 && timeOfDayGreeting.Hour < 12)
                                {
                                    lblStatus.Text = LanguageLocalizationParser.GetCurrentLanguageTags["Morning"]; //Console.WriteLine("Good morning!");
                                }
                                else if (timeOfDayGreeting.Hour >= 12 && timeOfDayGreeting.Hour < 17)
                                {
                                    lblStatus.Text = LanguageLocalizationParser.GetCurrentLanguageTags["Afternoon"]; //Console.WriteLine("Good afternoon!");
                                }
                                else
                                {
                                    lblStatus.Text = LanguageLocalizationParser.GetCurrentLanguageTags["Night"]; //Console.WriteLine("Good night!");
                                }


                            }));
                        }
                        else if (EntryController.TerminalState == TerminalState.TicketApproved)
                        {
                            lblStatus.Invoke((MethodInvoker)(() =>
                            {
                                lblStatus.Visible = true;
                                lblStatus.Text = LanguageLocalizationParser.GetCurrentLanguageTags["Entry-EntryGranted"];
                            }));
                        }
                        else if (EntryController.TerminalState == TerminalState.OutOfService)
                        {
                            lblStatus.Invoke((MethodInvoker)(() =>
                            {
                                lblStatus.Visible = true;
                                lblStatus.Text = LanguageLocalizationParser.GetCurrentLanguageTags["Entry-OutOfService"];
                            }));
                        }

                        statusStrip1.Invoke((MethodInvoker)(() =>
                        {
                            statusStrip1.Items["tlblStatusTerminal"].Text = EntryController.TerminalState.ToString();
                        }));
                    }

                    #endregion Entry Controller Actions

                    Thread.Sleep(500);
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
            var msgBoxResult = MessageBox.Show("ADVERTENCIA! Al salir se desactivará el servicio de la terminal, por lo que NO SERÁ POSIBLE la correcta operación del mismo. ¿Desea continuar?", "Terminal", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

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

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            bool showTestInput = bool.Parse(ConfigurationManager.AppSettings["ShowTestInput"] ?? "false");
            if (e.KeyChar == 13)
            {
                this.EntryController.ReadInputValue = true;
                if (showTestInput)
                {
                    this.EntryController.InputValue = this.txtInputCode.Text;
                }
                else
                {
                    _log.Debug(this.EntryController.InputValue + " ready for querying");
                }
            }
            else
            {
                if (this.EntryController.InputValue == null)
                {
                    this.EntryController.InputValue = "";
                }

                this.EntryController.InputValue += e.KeyChar;
            }
        }

        private void txtInputCode_Leave(object sender, EventArgs e)
        {
            this.txtInputCode.Focus();
        }

        private void pictureBox1_Click_1(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {

        }

        private void statusStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void tlblVersion_Click(object sender, EventArgs e)
        {

        }

        private void tlblTerminalId_Click(object sender, EventArgs e)
        {

        }

        private void tlblDate_Click(object sender, EventArgs e)
        {

        }

        private void tlblStatusTerminal_Click(object sender, EventArgs e)
        {

        }

        private void txtInputCode_TextChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void dateBlue_Click(object sender, EventArgs e)
        {

        }

        private void lblStatus_Click(object sender, EventArgs e)
        {

        }

        private void label10_Click(object sender, EventArgs e)
        {

        }

        private void hourBlue_Click(object sender, EventArgs e)
        {

        }

        private void Fondo_Click(object sender, EventArgs e)
        {

        }

        private void btnAction_Click(object sender, EventArgs e)
        {

        }

        #region Labels management
        //private bool SetTemplates()
        //{

        //}

        //private bool SetTemplate(string id, string text)
        //{

        //}


        #endregion
    }
}