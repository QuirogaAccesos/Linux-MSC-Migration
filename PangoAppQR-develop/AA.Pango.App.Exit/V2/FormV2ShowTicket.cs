using AA.Pango.ServiceLayer.Controllers;
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
    public partial class FormV2ShowTicket : Form
    {
        protected ILog _log = LogManager.GetLogger("FormV2ShowTicket");
        CultureInfo deC = new CultureInfo("en-US");

        private int showTicketFormTimeout = int.Parse(ConfigurationManager.AppSettings["ShowTicketFormTimeout"] ?? "10000");
        private Thread Worker { get; set; }

        public FormV2ShowTicket()
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
            this.lblCurrentDate.Text = DateTime.Now.ToString("dddd, MMMM dd, yyyy HH:mm:ss");
            this.tlblStatusTerminal.Text = "";
            var readerComPort = ConfigurationManager.AppSettings["ReaderComPort"];
            var readerBauds = int.Parse(ConfigurationManager.AppSettings["ReaderBauds"] ?? "9600");

        }

        private void InitWorker()
        {
            _log.Debug("Starting Worker...");

            Worker = new Thread(new ThreadStart((() =>
            {
                while (true)
                {
                    try
                    {
                        if (this.IsDisposed)
                        {
                            _log.Debug("exiting thread...");
                            break;
                        }

                        if (!this.IsHandleCreated)
                        {
                            _log.Warn("handle not created");
                            break;
                        }

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

                        #region Entry Controller Actions

                        if (FormV2.Controller != null && lblStatus.InvokeRequired)
                        {
                            if ((FormV2.Controller.TerminalState == TerminalState.ReadingBarcode))
                            {
                                lblStatus.Invoke((MethodInvoker)(() =>
                                {
                                    lblStatus.Visible = true;
                                    lblStatus.Text = LanguageLocalizationParser.GetCurrentLanguageTags["Entry-IssuingVisitorCard"];
                                }));
                            }
                            else if ((FormV2.Controller.TerminalState == TerminalState.VehiclePresent))
                            {
                                lblStatus.Invoke((MethodInvoker)(() =>
                                {
                                    lblStatus.Visible = true;
                                    lblStatus.Text = LanguageLocalizationParser.GetCurrentLanguageTags["Entry-IssuingVisitorCard"];
                                }));
                            }
                            else if ((FormV2.Controller.TerminalState == TerminalState.TicketDenied || FormV2.Controller.TerminalState == TerminalState.TicketNotFound))
                            {
                                lblStatus.Invoke((MethodInvoker)(() =>
                                {
                                    lblStatus.Visible = true;
                                    lblStatus.Text = LanguageLocalizationParser.GetCurrentLanguageTags["Entry-RecurrentClientCardDenied"];
                                }));
                            }
                            else if (FormV2.Controller.TerminalState == TerminalState.StandBy)
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
                            else if (FormV2.Controller.TerminalState == TerminalState.TicketApproved)
                            {
                                lblStatus.Invoke((MethodInvoker)(() =>
                                {
                                    lblStatus.Visible = true;
                                    lblStatus.Text = LanguageLocalizationParser.GetCurrentLanguageTags["Entry-EntryGranted"];
                                }));
                            }
                            else if (FormV2.Controller.TerminalState == TerminalState.OutOfService)
                            {
                                lblStatus.Invoke((MethodInvoker)(() =>
                                {
                                    lblStatus.Visible = true;
                                    lblStatus.Text = LanguageLocalizationParser.GetCurrentLanguageTags["Entry-OutOfService"];
                                }));
                            }

                            if (!statusStrip1.IsDisposed)
                            {
                                if (statusStrip1.InvokeRequired)
                                {
                                    statusStrip1.Invoke((MethodInvoker)(() =>
                                    {
                                        statusStrip1.Items["tlblStatusTerminal"].Text = FormV2.Controller.TerminalState.ToString();
                                    }));
                                }
                                else
                                {
                                    statusStrip1.Items["tlblStatusTerminal"].Text = FormV2.Controller.TerminalState.ToString();
                                }
                            }
                        }

                    }
                    catch (Exception e)
                    {
                        _log.Error("Unexpected error in Worker thread", e);
                    }

                    Thread.Sleep(500);

                    #endregion Entry Controller Actions


                }
            })));

            Worker.Start();
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            this.Init();

            Thread _thread = new Thread(() =>
            {

                Thread.Sleep(showTicketFormTimeout);

                if (this.IsDisposed)
                {
                    return;
                }

                CloseForm();

            });
            _thread.SetApartmentState(ApartmentState.STA);
            _thread.Start();

        }

        private void Form1_FormClosing_1(object sender, FormClosingEventArgs e)
        {
            this.Hide();
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            bool showTestInput = bool.Parse(ConfigurationManager.AppSettings["ShowTestInput"] ?? "false");
            if (e.KeyChar == 13)
            {
                FormV2.Controller.ReadInputValue = true;
                if (showTestInput)
                {
                    FormV2.Controller.InputValue = this.txtInputCode.Text;
                }
                else
                {
                    _log.Debug(FormV2.Controller.InputValue + " ready for querying");
                }
            }
            else
            {
                if (FormV2.Controller.InputValue == null)
                {
                    FormV2.Controller.InputValue = "";
                }

                FormV2.Controller.InputValue += e.KeyChar;
            }
        }

        private void txtInputCode_Leave(object sender, EventArgs e)
        {
            this.txtInputCode.Focus();
        }


        #region Labels management
        //private bool SetTemplates()
        //{

        //}

        //private bool SetTemplate(string id, string text)
        //{

        //}


        #endregion

        private void btnGoBack_Click(object sender, EventArgs e)
        {
            this.Hide();
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