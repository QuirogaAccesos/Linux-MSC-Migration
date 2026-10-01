using AA.Pango.ServiceLayer;
using AA.Pango.ServiceLayer.ApiDtos;
using AA.Pango.TestWinApp;
using AA.Pango.TestWinApp.Utils;
using log4net;
using System;
using System.Configuration;
using System.Windows.Forms;

namespace AA.Pango.App.Exit.V3
{
    public partial class PlateOrPhone : UserControl
    {
        protected ILog _log = LogManager.GetLogger("PlateOrPhone");

        public event EventHandler<StatusEventArgs> Registered;

        protected virtual void OnRegistered(StatusEventArgs e)
        {
            EventHandler<StatusEventArgs> handler = Registered;
            handler?.Invoke(this, e);
        }

        public PlateOrPhone()
        {
            InitializeComponent();
        }

        private void OnLoad(object sender, EventArgs e)
        {
            this.btnPlate.Text = LanguageLocalizationParser.GetCurrentLanguageTags["PlateOrPhone_Plate"];
            this.btnPhone.Text = LanguageLocalizationParser.GetCurrentLanguageTags["PlateOrPhone_Phone"];
        }

        private void btnPlate_Click(object sender, EventArgs e)
        {
            var tempBtnText = this.btnPlate.Text;
            this.btnPlate.Text = "SENDING - PLEASE WAIT...";
            this.btnPlate.Enabled = false;
            labelError.Text = "";

            var form = new FrmOnScreenKeyboardV3(2);
            var dialogResult = form.ShowDialog(this);

            if (dialogResult == DialogResult.OK)
            {
                if (string.IsNullOrEmpty(form.Plate))
                    return;

                var (plateRequestSent, plateResponse) = PangoService.ChargePlate(new ChargePlateRequest
                {
                    Amount = 0M,
                    Currency = "USD",
                    InstallationID = ConfigurationManager.AppSettings["InstallationId"],
                    TerminalId = ConfigurationManager.AppSettings["TerminalId"],
                    Plate = form.Plate
                });

                _log.Debug("Plate request sent: " + (plateRequestSent ? "OK" : "NOT OK"));
                _log.Debug("Template ID received: " + plateResponse.templateId);

                if (plateRequestSent)
                {
                    if (plateResponse.templateId == "payAtExitMaxAmount" || plateResponse.templateId == "payAtExit")
                    {
                        OnRegistered(new StatusEventArgs
                        {
                            Type = 1,
                            Status = "OK",
                            chargeResponse = plateResponse
                        });
                    }
                    else
                    {
                        _log.Error("Invalid template ID: " + plateResponse.templateId);
                        OnRegistered(new StatusEventArgs
                        {
                            Type = 1,
                            Status = "KO"
                        });
                    }
                }
            }
            else
            {
                _log.Warn("No plate was read from the user.");
            }

            this.btnPlate.Enabled = true;
            this.btnPlate.Text = tempBtnText;
        }

        private void btnPhone_Click(object sender, EventArgs e)
        {
            var tempBtnText = this.btnPhone.Text;
            this.btnPhone.Text = "SENDING - PLEASE WAIT...";
            this.btnPhone.Enabled = false;
            labelError.Text = "";

            var form = new ScreenDigitKeyboardV3(2, false, LanguageLocalizationParser.GetCurrentLanguageTags["Form_Keyboard_HeaderPhone"]);
            var dialogResult = form.ShowDialog(this);

            if (dialogResult == DialogResult.OK)
            {
                if (string.IsNullOrEmpty(form.Phone))
                    return;

                if (form.Phone.Length != 10)
                {
                    labelError.Text = "Invalid Phone";
                    this.btnPhone.Enabled = true;
                    this.btnPhone.Text = tempBtnText;
                    return;
                }

                var (phoneRequestSent, phoneResponse) = PangoService.ChargePhone(new ChargePhoneRequest
                {
                    installationID = ConfigurationManager.AppSettings["InstallationId"],
                    terminalId = ConfigurationManager.AppSettings["TerminalId"],
                    phone = form.Phone
                });

                _log.Debug("Phone request sent: " + (phoneRequestSent ? "OK" : "NOT OK"));
                _log.Debug("Template ID received: " + phoneResponse.templateId);

                if (phoneRequestSent)
                {
                    if (phoneResponse.templateId == "payAtExitMaxAmount" || phoneResponse.templateId == "payAtExit")
                    {
                        OnRegistered(new StatusEventArgs
                        {
                            Type = 2,
                            Status = "OK",
                            chargeResponse = phoneResponse
                        });
                    }
                    else
                    {
                        _log.Error("Invalid template ID: " + phoneResponse.templateId);
                        OnRegistered(new StatusEventArgs
                        {
                            Type = 2,
                            Status = "KO"
                        });
                    }
                }
            }
            else
            {
                _log.Warn("No phone was read from the user.");
            }

            this.btnPhone.Enabled = true;
            this.btnPhone.Text = tempBtnText;
        }
    }
}
