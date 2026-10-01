using AA.Pango.ServiceLayer;
using AA.Pango.ServiceLayer.ApiDtos;
using AA.Pango.TestWinApp;
using AA.Pango.TestWinApp.Utils;
using log4net;
using System;
using System.Configuration;
using System.Drawing;
using System.Windows.Forms;

namespace AA.Pango.App.Exit.V4
{
    public partial class PaymentOptions : UserControl
    {
        protected ILog _log = LogManager.GetLogger("PaymentOptions");

        private static int WhiteListId = 0;

        public event EventHandler<StatusEventArgs> Registered;

        protected virtual void OnRegistered(StatusEventArgs e)
        {
            EventHandler<StatusEventArgs> handler = Registered;
            handler?.Invoke(this, e);
        }

        public PaymentOptions()
        {
            InitializeComponent();
        }

        public PaymentOptions(int whiteListId)
        {
            InitializeComponent();
            WhiteListId = whiteListId;
        }

        public void setWhiteList(int whiteListId)
        {
            WhiteListId = whiteListId;
        }

        private void OnLoad(object sender, EventArgs e)
        {
            this.btnPango.Text = LanguageLocalizationParser.GetCurrentLanguageTags["PaymentOptions_Pango"];
            this.btnLG1.Text = LanguageLocalizationParser.GetCurrentLanguageTags["PaymentOptions_LG1"];

            var foreColor = ColorTranslator.FromHtml(DataFileParser.Styles["FontColor"]);
            this.BackColor = ColorTranslator.FromHtml(DataFileParser.Styles["BackgroundColor"]);
            this.ForeColor = foreColor;
            this.btnPango.FlatAppearance.BorderColor = foreColor;
            this.btnLG1.FlatAppearance.BorderColor = foreColor;

            this.btnPango.BackColor = ColorTranslator.FromHtml(DataFileParser.Styles["BtnPangoBackgroundColor"]);
            this.btnLG1.BackColor = ColorTranslator.FromHtml(DataFileParser.Styles["BtnLG1BackgroundColor"]);

            var buttonsFontColor = ColorTranslator.FromHtml(DataFileParser.Styles["ButtonsFontColor"]);
            this.btnPango.ForeColor = buttonsFontColor;
            this.btnLG1.ForeColor = buttonsFontColor;
        }

        private void btnPango_Click(object sender, EventArgs e)
        {
            var tempBtnText = this.btnPango.Text;

            this.btnPango.Text = "SENDING - PLEASE WAIT...";
            this.btnPango.Enabled = false;

            //labelError.Text = "";

            var (chargeRequestSent, chargeResponse) = PangoService.ChargePermitExpired(new ChargePermitExpiredRequest
            {
                terminalId = ConfigurationManager.AppSettings["TerminalId"],
                action = 1,
                whileListId = WhiteListId
            });

            _log.Debug("Charge Permit request sent: " + (chargeRequestSent ? "OK" : "NOT OK"));
            _log.Debug("resultCode: " + chargeResponse.status);

            if (chargeRequestSent)
            {
                OnRegistered(new StatusEventArgs
                {
                    Type = 1,
                    Status = chargeResponse.status == 1 ? "OK" : "KO"
                });
            }

            this.btnPango.Enabled = true;
            this.btnPango.Text = tempBtnText;
        }

        private void btnLG1_Click(object sender, EventArgs e)
        {
            var tempBtnText = this.btnLG1.Text;

            this.btnLG1.Text = "SENDING - PLEASE WAIT...";
            this.btnLG1.Enabled = false;

            //labelError.Text = "";

            var (chargeRequestSent, chargeResponse) = PangoService.ChargePermitExpired(new ChargePermitExpiredRequest
            {
                terminalId = ConfigurationManager.AppSettings["TerminalId"],
                action = 2,
                whileListId = WhiteListId
            });

            _log.Debug("Charge Permit request sent: " + (chargeRequestSent ? "OK" : "NOT OK"));
            _log.Debug("resultCode: " + chargeResponse.status);

            if (chargeRequestSent)
            {
                OnRegistered(new StatusEventArgs
                {
                    Type = 2,
                    Status = chargeResponse.status == 1 ? "OK" : "KO"
                });
            }

            this.btnLG1.Enabled = true;
            this.btnLG1.Text = tempBtnText;
        }
    }
}
