using log4net;
using System;
using System.Configuration;
using System.Threading;
using System.Windows.Forms;

namespace AA.Pango.TestWinApp
{
    public partial class FrmOnScreenKeyboard : Form
    {
        protected ILog _log = LogManager.GetLogger("FrmOnScreenKeyboard");

        private int keyboardFormTimeout = int.Parse(ConfigurationManager.AppSettings["KeyboardFormTimeout"] ?? "60000");

        public string Plate { get; set; }

        public FrmOnScreenKeyboard()
        {
            InitializeComponent();
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
                    if (this.IsDisposed)
                    {
                        _log.Debug("exiting thread...");
                        break;
                    }
                    if (!this.IsHandleCreated)
                    {
                        break;
                    }

                    lblCurrentDate.Invoke((MethodInvoker)(() =>
                    {
                        lblCurrentDate.Text = DateTime.Now.ToString("dddd, MMMM dd, yyyy HH:mm:ss");
                    }));

                    Thread.Sleep(500);

                    if ((DateTime.Now - start).TotalMilliseconds > keyboardFormTimeout)
                    {
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
