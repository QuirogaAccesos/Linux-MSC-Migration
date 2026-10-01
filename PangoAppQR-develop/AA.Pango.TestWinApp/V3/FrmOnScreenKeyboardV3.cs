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
    public partial class FrmOnScreenKeyboardV3 : Form
    {
        protected ILog _log = LogManager.GetLogger("FrmOnScreenKeyboardV3");

        CultureInfo deC = new CultureInfo("en-US");
        private int keyboardFormTimeout = int.Parse(ConfigurationManager.AppSettings["KeyboardFormTimeout"] ?? "60000");
        private static string dateTimeFormat = ConfigurationManager.AppSettings["DateTimeFormat"];
        private static string footVersion = ConfigurationManager.AppSettings["FootVersion"];

        private System.Resources.ResourceManager RM = new System.Resources.ResourceManager("AA.Pango.TestWinApp.Properties.Resources", typeof(Resources).Assembly);

        public string Plate { get; set; }

        public FrmOnScreenKeyboardV3()
        {
            InitializeComponent();
        }

        public FrmOnScreenKeyboardV3(int direction)
        {
            InitializeComponent();

            this.BackColor = ColorTranslator.FromHtml(DataFileParser.Styles["BackgroundColor"]);
            this.ForeColor = ColorTranslator.FromHtml(DataFileParser.Styles["FontColor"]);

            this.label1.BackColor = ColorTranslator.FromHtml(DataFileParser.Styles["KeyboardHeaderBackColor"]);
            this.label1.ForeColor = ColorTranslator.FromHtml(DataFileParser.Styles["KeyboardHeaderFontColor"]);

            this.panelFoot.BackColor = ColorTranslator.FromHtml(DataFileParser.Styles["FootBackgroundColor"]);
            this.panelFoot.ForeColor = ColorTranslator.FromHtml(DataFileParser.Styles["FootFontColor"]);

            if (footVersion == "1")
            {
                this.tableLayoutFoot.Visible = true;
                this.tableLayoutFoot2.Visible = false;
                this.tableLayoutFoot3.Visible = false;

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
            else if (footVersion == "2")
            {
                this.tableLayoutFoot.Visible = false;
                this.tableLayoutFoot2.Visible = true;
                this.tableLayoutFoot3.Visible = false;

                this.imgFootLogoPango2.BackgroundImage = (Image)RM.GetObject(DataFileParser.Styles["FootLogoPango"]);
                this.imgFootLogo2.BackgroundImage = (Image)RM.GetObject(DataFileParser.Styles["FootLogoV2"]);
                this.imgFootBtn24h.BackgroundImage = (Image)RM.GetObject(DataFileParser.Styles["FootBtn24h"]);
            }
            else
            {
                this.tableLayoutFoot.Visible = false;
                this.tableLayoutFoot2.Visible = false;
                this.tableLayoutFoot3.Visible = true;

                this.imgFootBtn24h3.BackgroundImage = (Image)RM.GetObject(DataFileParser.Styles["FootBtn24h"]);

                if (direction == 1)
                {
                    this.lblFootPrice3.Text = LanguageLocalizationParser.GetCurrentLanguageTags["Form_Keyboard_Price"];
                    lblFootPrice3.BringToFront();
                }
                if (direction == 2)
                {
                    imgFootBtn24h3.BringToFront();
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            this.WindowState = FormWindowState.Maximized;
            this.plateInput1.PlateEntered += c_PlateEntered;

            InitWorker();

        }

        public void c_PlateEntered(object sender, PlateEnteredEventArgs e)
        {
            _log.Debug("The plate was entered: " + e.Plate?.ToUpper());

            if (!string.IsNullOrEmpty(e.Plate))
            {
                this.Plate = e.Plate.ToUpper();
                this.DialogResult = DialogResult.OK;

            }
            else
            {
                this.Plate = null;
                this.DialogResult = DialogResult.Cancel;
            }

            this.Close();

        }

        private void plateInput1_Load(object sender, EventArgs e)
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
                        if (!lblFootDate3.IsDisposed)
                        {
                            if (lblFootDate3.InvokeRequired)
                            {
                                lblFootDate3.Invoke((MethodInvoker)(() =>
                                {
                                    lblFootDate3.Text = DateTime.Now.ToString(dateTimeFormat, deC);
                                }));
                            }
                            else
                                lblFootDate3.Text = DateTime.Now.ToString(dateTimeFormat, deC);
                        }

                        Thread.Sleep(500);

                        if ((DateTime.Now - start).TotalMilliseconds > keyboardFormTimeout)
                        {
                            CloseForm();
                        }
                    }
                    catch (Exception ex)
                    {
                        _log.Error("ERROR KEYBOARD", ex);
                        CloseForm();
                    }
                }
            })));

            Worker.Start();
        }

        private void label1_Click(object sender, EventArgs e)
        {

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
