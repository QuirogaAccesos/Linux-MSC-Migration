using AA.Pango.TestWinApp.Utils;
using log4net;
using System;
using System.Configuration;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace AA.Pango.App.Exit.V4
{
    public partial class FormLoading : Form
    {
        protected ILog _log = LogManager.GetLogger("FormLoading");

        private Thread Worker { get; set; }

        private int showLoadingTimeout = int.Parse(ConfigurationManager.AppSettings["showLoadingTimeout"] ?? "500");

        public FormLoading()
        {
            InitializeComponent();
        }

        public FormLoading(int newTimeOut)
        {
            showLoadingTimeout = newTimeOut;
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Init();
        }

        private void Init()
        {
            InitSettings();
            InitWorker();
        }

        private void InitSettings()
        {
            _log.Debug("init settings...");

            LanguageLocalizationParser.LoadLocalizations();

            #region Style Edits
            this.BackColor = ColorTranslator.FromHtml(DataFileParser.Styles["BackgroundColor"]);
            this.ForeColor = ColorTranslator.FromHtml(DataFileParser.Styles["FontColor"]);

            #endregion Style Edits

            this.lblLoading.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4Loading"];
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

                    Thread.Sleep(showLoadingTimeout);

                    CloseForm();
                }
            })));

            Worker.SetApartmentState(ApartmentState.STA);
            Worker.Start();
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
