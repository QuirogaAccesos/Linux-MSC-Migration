using AA.Pango.TestWinApp.Utils;
using log4net;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace AA.Pango.TestWinApp
{
    public partial class PhoneInputV3 : UserControl
    {
        protected ILog _log = LogManager.GetLogger("PhoneInputV3");

        public event EventHandler<PhoneEnteredEventArgs> PhoneEntered;

        protected virtual void OnPhoneEntered(PhoneEnteredEventArgs e)
        {
            EventHandler<PhoneEnteredEventArgs> handler = PhoneEntered;
            handler?.Invoke(this, e);
        }

        public PhoneInputV3()
        {
            InitializeComponent();

            this.panelInput.BackColor = ColorTranslator.FromHtml(DataFileParser.Styles["BackgroundColor"]);
            this.txtInput.BackColor = ColorTranslator.FromHtml(DataFileParser.Styles["KeyboardInputBackColor"]);
            this.txtInput.ForeColor = ColorTranslator.FromHtml(DataFileParser.Styles["KeyboardInputFontColor"]);

            this.btnGoBack.BackColor = ColorTranslator.FromHtml(DataFileParser.Styles["BtnBackBackgroundColor"]);
            this.btnGoBack.ForeColor = ColorTranslator.FromHtml(DataFileParser.Styles["BtnBackFontColor"]);
            this.btnGoBack.FlatAppearance.BorderColor = ColorTranslator.FromHtml(DataFileParser.Styles["BtnBackBorderColor"]);

            this.panelKeys.ForeColor = ColorTranslator.FromHtml(DataFileParser.Styles["KeyboardKeyFontColor"]);

            var keyBackColor = ColorTranslator.FromHtml(DataFileParser.Styles["KeyboardKeyBackColor"]);
            this.button1.BackColor = keyBackColor;
            this.button2.BackColor = keyBackColor;
            this.button3.BackColor = keyBackColor;
            this.button4.BackColor = keyBackColor;
            this.button5.BackColor = keyBackColor;
            this.button6.BackColor = keyBackColor;
            this.button7.BackColor = keyBackColor;
            this.button8.BackColor = keyBackColor;
            this.button9.BackColor = keyBackColor;
            this.button0.BackColor = keyBackColor;
        }

        private void TypeInput(string input)
        {
            if (!this.IsDisposed && !txtInput.IsDisposed)
            {
                var foreColor = ColorTranslator.FromHtml(DataFileParser.Styles["KeyboardInputFontColor"]);
                if (this.txtInput.ForeColor != foreColor)
                {
                    this.txtInput.ForeColor = foreColor;
                }
                if (this.txtInput.Text.Length > 0)
                {
                    this.txtInput.Text = this.txtInput.Text + input;
                }
                else
                {
                    this.txtInput.Text = input;
                }
            }
        }

        private void btnClearInput_Click(object sender, EventArgs e)
        {
            if (!this.IsDisposed && !txtInput.IsDisposed)
            {
                this.txtInput.Clear();
                this.txtInput.Focus();
            }
        }

        private void btnBackspace_Click(object sender, EventArgs e)
        {
            if (!this.IsDisposed && !txtInput.IsDisposed)
            {
                if (this.txtInput.Text == null)
                {
                    this.txtInput.Text = "";
                    return;
                }

                if (this.txtInput.Text == "")
                {
                    return;
                }

                this.txtInput.Text = this.txtInput.Text.Substring(0, this.txtInput.Text.Length - 1);
            }
        }

        private void PhoneInputV3_Load(object sender, EventArgs e)
        {
            this.txtInput.Focus();
        }

        private void btnProcess_Click(object sender, EventArgs e)
        {
            OnPhoneEntered(new PhoneEnteredEventArgs
            {
                Phone = this.txtInput.Text,
                Date = DateTime.Now
            });
        }

        private void button1_Click(object sender, EventArgs e)
        {
            TypeInput("1");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            TypeInput("2");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            TypeInput("3");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            TypeInput("4");
        }

        private void button5_Click(object sender, EventArgs e)
        {
            TypeInput("5");
        }

        private void button6_Click(object sender, EventArgs e)
        {
            TypeInput("6");
        }

        private void button7_Click(object sender, EventArgs e)
        {
            TypeInput("7");
        }

        private void button8_Click(object sender, EventArgs e)
        {
            TypeInput("8");
        }

        private void button9_Click(object sender, EventArgs e)
        {
            TypeInput("9");
        }

        private void button0_Click(object sender, EventArgs e)
        {
            TypeInput("0");
        }

        private void button1_Paint(object sender, PaintEventArgs e)
        {
            //e.
        }

        private void btnGoBack_Click(object sender, EventArgs e)
        {
            _log.Debug("GoBack");
            OnPhoneEntered(new PhoneEnteredEventArgs
            {
                Phone = "",
                Date = DateTime.Now
            });
        }

        public void SetInputForeColor(Color color)
        {
            this.txtInput.ForeColor = color;
        }
    }
}
