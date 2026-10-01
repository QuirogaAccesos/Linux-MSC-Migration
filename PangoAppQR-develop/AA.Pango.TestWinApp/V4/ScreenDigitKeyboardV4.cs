using AA.Pango.TestWinApp.Properties;
using AA.Pango.TestWinApp.Utils;
using log4net;
using System;
using System.Configuration;
using System.Drawing;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;

namespace AA.Pango.TestWinApp
{
    public partial class ScreenDigitKeyboardV4 : Form
    {
        protected ILog _log = LogManager.GetLogger("ScreenDigitKeyboardV4");

        CultureInfo deC = new CultureInfo("en-US");
        private int keyboardFormTimeout = int.Parse(ConfigurationManager.AppSettings["KeyboardFormTimeout"] ?? "60000");
        private static string dateTimeFormat = ConfigurationManager.AppSettings["DateTimeFormat"];
        private static string headerVersion = ConfigurationManager.AppSettings["KeyboardHeaderVersion"];
        private static string footVersion = ConfigurationManager.AppSettings["FootVersion"];

        private System.Resources.ResourceManager RM = new System.Resources.ResourceManager("AA.Pango.TestWinApp.Properties.Resources", typeof(Resources).Assembly);

        public string Phone { get; set; }

        public ScreenDigitKeyboardV4()
        {
            InitializeComponent();
        }

        public ScreenDigitKeyboardV4(int direction)
        {
            InitializeComponent();

            this.BackColor = ColorTranslator.FromHtml(DataFileParser.Styles["KeyboardBackgroundColor"]);
            this.ForeColor = ColorTranslator.FromHtml(DataFileParser.Styles["FontColor"]);

            this.lblHeader.BackColor = ColorTranslator.FromHtml(DataFileParser.Styles["DigitKeyboardHeaderBackColor"]);
            this.lblHeader.ForeColor = ColorTranslator.FromHtml(DataFileParser.Styles["DigitKeyboardHeaderFontColor"]);
            this.lblHeader1.BackColor = ColorTranslator.FromHtml(DataFileParser.Styles["DigitKeyboardHeaderBackColor"]);
            this.lblHeader1.ForeColor = ColorTranslator.FromHtml(DataFileParser.Styles["DigitKeyboardHeaderFontColor"]);
            this.lblHeader2.BackColor = ColorTranslator.FromHtml(DataFileParser.Styles["DigitKeyboardHeaderBackColor"]);
            this.lblHeader2.ForeColor = ColorTranslator.FromHtml(DataFileParser.Styles["DigitKeyboardHeaderFontColor"]);
            this.lblInstructions.BackColor = ColorTranslator.FromHtml(DataFileParser.Styles["DigitKeyboardHeaderBackColor"]);
            this.lblInstructions.ForeColor = ColorTranslator.FromHtml(DataFileParser.Styles["DigitKeyboardHeaderFontColor"]);

            this.panelFoot.BackColor = ColorTranslator.FromHtml(DataFileParser.Styles["FootBackgroundColor"]);
            this.panelFoot.ForeColor = ColorTranslator.FromHtml(DataFileParser.Styles["FootFontColor"]);

            this.lblHeader.Text = LanguageLocalizationParser.GetCurrentLanguageTags["Form_Keyboard_HeaderPhone"];
            this.lblHeader1.Text = LanguageLocalizationParser.GetCurrentLanguageTags["Form_Keyboard_HeaderPhone"];
            this.lblHeader2.Text = LanguageLocalizationParser.GetCurrentLanguageTags["Form_Keyboard_HeaderPhone"];
            this.lblInstructions.Text = LanguageLocalizationParser.GetCurrentLanguageTags["Form_Keyboard_HeaderPhoneInstructions"];

            //if (headerVersion == "1")
            //{
            //    this.lblHeader1.Visible = true;
            //    this.tableHeader2.Visible = false;
            //}
            //else
            //{
            //    this.lblHeader1.Visible = false;
            //    this.tableHeader2.Visible = true;
            //}

            if (footVersion == "1")
            {
                this.tableLayoutFoot.Visible = true;
                this.tableLayoutFoot2.Visible = false;

                this.lblFootDownload.Text = LanguageLocalizationParser.GetCurrentLanguageTags["Form_Keyboard_DownloadApp"];
                this.imgFootLogoPango.BackgroundImage = (Image)RM.GetObject(DataFileParser.Styles["FootLogoPango"]);
                this.imgFootYellowBtn.BackgroundImage = (Image)RM.GetObject(DataFileParser.Styles["FootBtn24h"]);

                if (direction == 1)
                {
                    this.lblFootPrice.Text = LanguageLocalizationParser.GetCurrentLanguageTags["Form_Keyboard_Price"];
                    lblFootPrice.BringToFront();
                }
                if (direction == 2)
                {
                    imgFootYellowBtn.BringToFront();
                }
            }
            else
            {
                this.tableLayoutFoot.Visible = false;
                this.tableLayoutFoot2.Visible = true;

                this.imgFootLogoPango2.BackgroundImage = (Image)RM.GetObject(DataFileParser.Styles["FootLogoPango"]);
                this.imgFootLogo2.BackgroundImage = (Image)RM.GetObject(DataFileParser.Styles["FootLogoV2"]);
                this.imgFootBtn24h.BackgroundImage = (Image)RM.GetObject(DataFileParser.Styles["FootBtn24h"]);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;
            this.phoneInput.PhoneEntered += c_PhoneEntered;

            InitWorker();
        }

        public void c_PhoneEntered(object sender, PhoneEnteredEventArgs e)
        {
            _log.Debug("The Phone was entered: " + e.Phone?.ToUpper());

            if (!string.IsNullOrEmpty(e.Phone))
            {
                this.Phone = e.Phone.ToUpper();
                this.DialogResult = DialogResult.OK;
            }
            else
            {
                this.Phone = null;
                this.DialogResult = DialogResult.Cancel;
            }

            this.Close();
        }

        private void phoneInput_Load(object sender, EventArgs e)
        {

        }

        private void InitWorker()
        {
            _log.Debug("Starting Worker...");

            var start = DateTime.Now;
            var Worker = new Thread(new ThreadStart((() =>
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
                            break;
                        }
                    
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

                        Thread.Sleep(500);

                        if ((DateTime.Now - start).TotalMilliseconds > keyboardFormTimeout)
                        {
                            CloseForm();
                        }
                    }
                    catch (Exception ex)
                    {
                        _log.Error("ERROR DIGIT KEYBOARD", ex);
                        CloseForm();
                    }
                }
            })));

            Worker.Start();
        }

        private void CloseForm()
        {
            if (this.IsDisposed)
                return;

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
    }
}
