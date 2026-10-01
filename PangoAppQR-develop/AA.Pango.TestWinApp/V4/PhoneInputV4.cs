using AA.Pango.TestWinApp.Utils;
using log4net;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace AA.Pango.TestWinApp
{
    public partial class PhoneInputV4 : UserControl
    {
        protected ILog _log = LogManager.GetLogger("PhoneInputV4");

        public event EventHandler<PhoneEnteredEventArgs> PhoneEntered;

        protected virtual void OnPhoneEntered(PhoneEnteredEventArgs e)
        {
            EventHandler<PhoneEnteredEventArgs> handler = PhoneEntered;
            handler?.Invoke(this, e);
        }

        public PhoneInputV4()
        {
            InitializeComponent();
        }

        private void TypeInput(string input)
        {
            if (!this.IsDisposed && !txtInput.IsDisposed)
            {
                if (this.txtInput.ForeColor == Color.Red)
                    this.txtInput.ForeColor = ColorTranslator.FromHtml(DataFileParser.Styles["KeyboardInputFontColor"]);

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
                if (this.txtInput.ForeColor == Color.Red)
                    this.txtInput.ForeColor = ColorTranslator.FromHtml(DataFileParser.Styles["KeyboardInputFontColor"]);

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

                if (this.txtInput.ForeColor == Color.Red)
                    this.txtInput.ForeColor = ColorTranslator.FromHtml(DataFileParser.Styles["KeyboardInputFontColor"]);

                this.txtInput.Text = this.txtInput.Text.Substring(0, this.txtInput.Text.Length - 1);
            }
        }

        private void PhoneInputV4_Load(object sender, EventArgs e)
        {
            this.txtInput.Focus();

            this.btnProcess.Text = LanguageLocalizationParser.GetCurrentLanguageTags["Form_Keyboard_btnConfirm"];
            this.btnClearInput.Text = LanguageLocalizationParser.GetCurrentLanguageTags["Form_Keyboard_btnClear"];
            this.btnGoBack.Text = LanguageLocalizationParser.GetCurrentLanguageTags["Form_Keyboard_btnBack"];

            if (DataFileParser.Styles != null && DataFileParser.Styles.Count > 0)
            {
                var backColor = ColorTranslator.FromHtml(DataFileParser.Styles["KeyboardBackgroundColor"]);
                this.BackColor = backColor;
                this.panelInput.BackColor = backColor;
                this.txtInput.BackColor = ColorTranslator.FromHtml(DataFileParser.Styles["KeyboardInputBackColor"]);
                this.txtInput.ForeColor = ColorTranslator.FromHtml(DataFileParser.Styles["KeyboardInputFontColor"]);

                this.btnGoBack.BackColor = ColorTranslator.FromHtml(DataFileParser.Styles["BtnBackBackgroundColor"]);
                this.btnGoBack.ForeColor = ColorTranslator.FromHtml(DataFileParser.Styles["BtnBackFontColor"]);
                this.btnGoBack.FlatAppearance.BorderColor = ColorTranslator.FromHtml(DataFileParser.Styles["BtnBackBorderColor"]);

                this.panelKeys.ForeColor = ColorTranslator.FromHtml(DataFileParser.Styles["KeyboardKeyFontColor"]);

                var keyBackColor = ColorTranslator.FromHtml(DataFileParser.Styles["KeyboardKeyBackColor"]);
                this.btnProcess.FlatAppearance.BorderColor = keyBackColor;
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
        }

        private void btnProcess_Click(object sender, EventArgs e)
        {
            if (this.txtInput.Text.Length != 10)
                this.txtInput.ForeColor = Color.Red;
            else
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

        private void btnGoBack_Click(object sender, EventArgs e)
        {
            _log.Debug("GoBack");
            OnPhoneEntered(new PhoneEnteredEventArgs
            {
                Phone = "",
                Date = DateTime.Now
            });
        }
    }
}
