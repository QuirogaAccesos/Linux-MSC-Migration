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
    public partial class PlateOrPhone : UserControl
    {
        protected ILog _log = LogManager.GetLogger("PlateOrPhone");

        private static string footVersion = ConfigurationManager.AppSettings["FootVersion"];
        private static string numOpt = ConfigurationManager.AppSettings["PlateOrPhoneOptions"];
        private static string checkPermitPlate = ConfigurationManager.AppSettings["CheckPermitPlate"];
        private static string plate1B;

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
            this.btnPlate_1optB.Text = LanguageLocalizationParser.GetCurrentLanguageTags["PlateOrPhone_Plate"];

            var foreColor = ColorTranslator.FromHtml(DataFileParser.Styles["FontColor"]);
            this.BackColor = ColorTranslator.FromHtml(DataFileParser.Styles["BackgroundColor"]);
            this.ForeColor = foreColor;
            this.btnPlate.FlatAppearance.BorderColor = foreColor;
            this.btnPhone.FlatAppearance.BorderColor = foreColor;
            this.btnPlate_1optB.FlatAppearance.BorderColor = foreColor;

            this.btnPlate.BackColor = ColorTranslator.FromHtml(DataFileParser.Styles["BtnPlateBackgroundColor"]);
            this.btnPhone.BackColor = ColorTranslator.FromHtml(DataFileParser.Styles["BtnPhoneBackgroundColor"]);
            this.btnPlate_1optB.BackColor = ColorTranslator.FromHtml(DataFileParser.Styles["BtnPlateBackgroundColor"]);

            var buttonsFontColor = ColorTranslator.FromHtml(DataFileParser.Styles["ButtonsFontColor"]);
            this.btnPlate.ForeColor = buttonsFontColor;
            this.btnPhone.ForeColor = buttonsFontColor;
            this.btnPlate_1optB.ForeColor = buttonsFontColor;

            if (numOpt == "1B")
            {
                this.panel2opt.Visible = false;
                this.panel2opt.SendToBack();
                this.panel1optB.Visible = true;
                this.panel1optB.BringToFront();
            }
            else
            {
                this.panel2opt.Visible = true;
                this.panel2opt.BringToFront();
                this.panel1optB.Visible = false;
                this.panel1optB.SendToBack();
            }
        }

        private void btnPlate_Click(object sender, EventArgs e)
        {
            var tempBtnText = this.btnPlate.Text;
            if (numOpt == "1B")
                tempBtnText = this.btnPlate_1optB.Text;

            if (numOpt == "1B")
            {
                this.btnPlate_1optB.Text = "SENDING - PLEASE WAIT...";
                this.btnPlate_1optB.Enabled = false;
            }
            else
            {
                this.btnPlate.Text = "SENDING - PLEASE WAIT...";
                this.btnPlate.Enabled = false;
            }
            
            labelError.Text = "";

            var form = new FrmOnScreenKeyboardV4(2);
            var dialogResult = form.ShowDialog(this);

            if (dialogResult == DialogResult.OK)
            {
                if (string.IsNullOrEmpty(form.Plate))
                    return;

                if (checkPermitPlate == "true")
                {
                    var (plateRequestSent, plateResponse) = PangoService.CheckPermitPlate(new CheckPermitPlateRequest
                    {
                        installationID = ConfigurationManager.AppSettings["InstallationId"],
                        terminalId = ConfigurationManager.AppSettings["TerminalId"],
                        plate = form.Plate
                    });

                    _log.Debug("Check Permit Plate request sent: " + (plateRequestSent ? "OK" : "NOT OK"));

                    if (plateRequestSent)
                    {
                        _log.Debug("status received: " + plateResponse.resultCode);

                        if (plateResponse.allowAccess)
                        {
                            OnRegistered(new StatusEventArgs
                            {
                                Type = 1,
                                Status = "OK",
                                HasPermit = true
                            });
                            return;
                        }
                        else
                        {
                            _log.Error("error message: " + plateResponse.description);
                            plate1B = form.Plate;
                        }
                    }
                }

                if (numOpt == "1B")
                {
                    btnPhone_Click(sender, e);
                }
                else
                {
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
            }
            else
            {
                _log.Warn("No plate was read from the user.");
            }

            if (numOpt == "1B")
            {
                this.btnPlate_1optB.Enabled = true;
                this.btnPlate_1optB.Text = tempBtnText;
            }
            else
            {
                this.btnPlate.Enabled = true;
                this.btnPlate.Text = tempBtnText;
            }
        }

        private void btnPhone_Click(object sender, EventArgs e)
        {
            var tempBtnText = this.btnPhone.Text;
            if (numOpt == "2")
            {
                this.btnPhone.Text = "SENDING - PLEASE WAIT...";
                this.btnPhone.Enabled = false;
            }
            labelError.Text = "";

            var form = new ScreenDigitKeyboardV4(2);
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

                bool requestSent;
                ChargePlateResponse response;
                if (numOpt == "1B")
                {
                    var(platePhoneRequestSent, platePhoneResponse) = PangoService.ChargePlatePhone(new ChargePlatePhoneRequest
                    {
                        installationID = ConfigurationManager.AppSettings["InstallationId"],
                        terminalId = ConfigurationManager.AppSettings["TerminalId"],
                        plate = plate1B,
                        phone = form.Phone
                    });
                    requestSent = platePhoneRequestSent;
                    response = platePhoneResponse;
                }
                else
                {
                    var(phoneRequestSent, phoneResponse) = PangoService.ChargePhone(new ChargePhoneRequest
                    {
                        installationID = ConfigurationManager.AppSettings["InstallationId"],
                        terminalId = ConfigurationManager.AppSettings["TerminalId"],
                        phone = form.Phone
                    });
                    requestSent = phoneRequestSent;
                    response = phoneResponse;
                }
                
                _log.Debug("Phone request sent: " + (requestSent ? "OK" : "NOT OK"));
                _log.Debug("Template ID received: " + response.templateId);

                if (requestSent)
                {
                    if (response.templateId == "payAtExitMaxAmount" || response.templateId == "payAtExit")
                    {
                        OnRegistered(new StatusEventArgs
                        {
                            Type = 2,
                            Status = "OK",
                            chargeResponse = response
                        });
                    }
                    else
                    {
                        _log.Error("Invalid template ID: " + response.templateId);
                        OnRegistered(new StatusEventArgs
                        {
                            Type = 2,
                            Status = "KO",
                            TemplateId = response.templateId,
                            Plate = numOpt == "1B" ? plate1B : "",
                            Phone = numOpt == "1B" ? form.Phone : ""
                        });
                    }
                }
            }
            else
            {
                _log.Warn("No phone was read from the user.");
            }

            if (numOpt == "2")
            {
                this.btnPhone.Enabled = true;
                this.btnPhone.Text = tempBtnText;
            }
        }
    }
}
