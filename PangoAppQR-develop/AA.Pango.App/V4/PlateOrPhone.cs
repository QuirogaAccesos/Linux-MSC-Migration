using AA.Pango.ServiceLayer;
using AA.Pango.ServiceLayer.ApiDtos;
using AA.Pango.TestWinApp;
using AA.Pango.TestWinApp.Utils;
using log4net;
using System;
using System.Configuration;
using System.Drawing;
using System.Windows.Forms;

namespace AA.Pango.App.V4
{
    public partial class PlateOrPhone : UserControl
    {
        protected ILog _log = LogManager.GetLogger("PlateOrPhone");
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
            this.btnPlate_3opt.Text = LanguageLocalizationParser.GetCurrentLanguageTags["PlateOrPhone_Plate"];
            this.btnPhone_3opt.Text = LanguageLocalizationParser.GetCurrentLanguageTags["PlateOrPhone_Phone"];
            this.btnTicket_3opt.Text = LanguageLocalizationParser.GetCurrentLanguageTags["PlateOrPhone_Ticket"];
            this.btnTicket_1opt.Text = LanguageLocalizationParser.GetCurrentLanguageTags["PlateOrPhone_Ticket"];
            this.btnPlate_1optB.Text = LanguageLocalizationParser.GetCurrentLanguageTags["PlateOrPhone_Plate"];

            var foreColor = ColorTranslator.FromHtml(DataFileParser.Styles["FontColor"]);
            this.BackColor = ColorTranslator.FromHtml(DataFileParser.Styles["BackgroundColor"]);
            this.ForeColor = foreColor;
            this.btnPlate.FlatAppearance.BorderColor = foreColor;
            this.btnPhone.FlatAppearance.BorderColor = foreColor;
            this.btnPlate_3opt.FlatAppearance.BorderColor = foreColor;
            this.btnPhone_3opt.FlatAppearance.BorderColor = foreColor;
            this.btnTicket_3opt.FlatAppearance.BorderColor = foreColor;
            this.btnTicket_1opt.FlatAppearance.BorderColor = foreColor;
            this.btnPlate_1optB.FlatAppearance.BorderColor = foreColor;

            this.btnPlate.BackColor = ColorTranslator.FromHtml(DataFileParser.Styles["BtnPlateBackgroundColor"]);
            this.btnPhone.BackColor = ColorTranslator.FromHtml(DataFileParser.Styles["BtnPhoneBackgroundColor"]);
            this.btnPlate_3opt.BackColor = ColorTranslator.FromHtml(DataFileParser.Styles["BtnPlateBackgroundColor"]);
            this.btnPhone_3opt.BackColor = ColorTranslator.FromHtml(DataFileParser.Styles["BtnPhoneBackgroundColor"]);
            this.btnTicket_3opt.BackColor = ColorTranslator.FromHtml(DataFileParser.Styles["BtnTicketBackgroundColor"]);
            this.btnTicket_1opt.BackColor = ColorTranslator.FromHtml(DataFileParser.Styles["BtnTicketBackgroundColor"]);
            this.btnPlate_1optB.BackColor = ColorTranslator.FromHtml(DataFileParser.Styles["BtnPlateBackgroundColor"]);

            var buttonsFontColor = ColorTranslator.FromHtml(DataFileParser.Styles["ButtonsFontColor"]);
            this.btnPlate.ForeColor = buttonsFontColor;
            this.btnPhone.ForeColor = buttonsFontColor;
            this.btnPlate_3opt.ForeColor = buttonsFontColor;
            this.btnPhone_3opt.ForeColor = buttonsFontColor;
            this.btnTicket_3opt.ForeColor = buttonsFontColor;
            this.btnTicket_1opt.ForeColor = buttonsFontColor;
            this.btnPlate_1optB.ForeColor = buttonsFontColor;

            if (numOpt == "1")
            {
                this.panel1opt.Visible = true;
                this.panel1opt.BringToFront();
                this.panel1optB.Visible = false;
                this.panel1optB.SendToBack();
                this.panel2opt.Visible = false;
                this.panel2opt.SendToBack();
                this.panel3opt.Visible = false;
                this.panel3opt.SendToBack();
            }
            if (numOpt == "1B")
            {
                this.panel1opt.Visible = false;
                this.panel1opt.SendToBack();
                this.panel1optB.Visible = true;
                this.panel1optB.BringToFront();
                this.panel2opt.Visible = false;
                this.panel2opt.SendToBack();
                this.panel3opt.Visible = false;
                this.panel3opt.SendToBack();
            }
            else if (numOpt == "2")
            {
                this.panel1opt.Visible = false;
                this.panel1opt.SendToBack();
                this.panel1optB.Visible = false;
                this.panel1optB.SendToBack();
                this.panel2opt.Visible = true;
                this.panel2opt.BringToFront();
                this.panel3opt.Visible = false;
                this.panel3opt.SendToBack();
            }
            else if (numOpt == "3")
            {
                this.panel1opt.Visible = false;
                this.panel1opt.SendToBack();
                this.panel1optB.Visible = false;
                this.panel1optB.SendToBack();
                this.panel2opt.Visible = false;
                this.panel2opt.SendToBack();
                this.panel3opt.Visible = true;
                this.panel3opt.BringToFront();
            }
        }

        private void btnPlate_Click(object sender, EventArgs e)
        {
            var tempBtnText = this.btnPlate.Text;
            if (numOpt == "1B")
                tempBtnText = this.btnPlate_1optB.Text;
            else if (numOpt == "3")
                tempBtnText = this.btnPlate_3opt.Text;

            if (numOpt == "1B")
            {
                this.btnPlate_1optB.Text = "SENDING - PLEASE WAIT...";
                this.btnPlate_1optB.Enabled = false;
            }
            else if (numOpt == "2")
            {
                this.btnPlate.Text = "SENDING - PLEASE WAIT...";
                this.btnPlate.Enabled = false;
            }
            else if (numOpt == "3")
            {
                this.btnPlate_3opt.Text = "SENDING - PLEASE WAIT...";
                this.btnPlate_3opt.Enabled = false;
            }

            labelError.Text = "";

            var form = new FrmOnScreenKeyboardV4(1);
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
                                Status = "OK"
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
                    var (plateRequestSent, plateResponse) = PangoService.TransientPlateOrPhone(new TransientPlateOrPhoneRequest
                    {
                        installationID = ConfigurationManager.AppSettings["InstallationId"],
                        terminalId = ConfigurationManager.AppSettings["TerminalId"],
                        plate = form.Plate
                    });

                    _log.Debug("Plate request sent: " + (plateRequestSent ? "OK" : "NOT OK"));

                    if (plateRequestSent)
                    {
                        _log.Debug("status received: " + plateResponse.status);

                        if (plateResponse.status == 1)
                        {
                            OnRegistered(new StatusEventArgs
                            {
                                Type = 1,
                                Status = "OK",
                                TransientId = plateResponse.transientId,
                                TicketQR = plateResponse.ticketQR
                            });
                        }
                        else
                        {
                            labelError.Text = "Error Try Again...";
                            _log.Error("error message: " + plateResponse.message);
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
            else if (numOpt == "2")
            {
                this.btnPlate.Enabled = true;
                this.btnPlate.Text = tempBtnText;
            }
            else if (numOpt == "3")
            {
                this.btnPlate_3opt.Enabled = true;
                this.btnPlate_3opt.Text = tempBtnText;
            }
        }

        private void btnPhone_Click(object sender, EventArgs e)
        {
            var tempBtnText = this.btnPhone.Text;
            if (numOpt == "3")
                tempBtnText = this.btnPhone_3opt.Text;

            if (numOpt == "2")
            {
                this.btnPhone.Text = "SENDING - PLEASE WAIT...";
                this.btnPhone.Enabled = false;
            }
            else if (numOpt == "3")
            {
                this.btnPhone_3opt.Text = "SENDING - PLEASE WAIT...";
                this.btnPhone_3opt.Enabled = false;
            }
            
            labelError.Text = "";

            var form = new ScreenDigitKeyboardV4(1);
            var dialogResult = form.ShowDialog(this);

            if (dialogResult == DialogResult.OK)
            {
                if (string.IsNullOrEmpty(form.Phone))
                    return;

                if (form.Phone.Length != 10)
                {
                    labelError.Text = "Invalid Phone";
                    if (numOpt == "2")
                    {
                        this.btnPhone.Enabled = true;
                        this.btnPhone.Text = tempBtnText;
                    }
                    else if (numOpt == "3")
                    {
                        this.btnPhone_3opt.Enabled = true;
                        this.btnPhone_3opt.Text = tempBtnText;
                    }
                    return;
                }

                var (phoneRequestSent, phoneResponse) = PangoService.TransientPlateOrPhone(new TransientPlateOrPhoneRequest
                {
                    installationID = ConfigurationManager.AppSettings["InstallationId"],
                    terminalId = ConfigurationManager.AppSettings["TerminalId"],
                    plate = numOpt == "1B" ? plate1B : null,
                    phone = form.Phone
                });
                
                _log.Debug("Phone request sent: " + (phoneRequestSent ? "OK" : "NOT OK"));

                if (phoneRequestSent)
                {
                    _log.Debug("status received: " + phoneResponse.status);

                    if (phoneResponse.status == 1)
                    {
                        OnRegistered(new StatusEventArgs
                        {
                            Type = 2,
                            Status = "OK",
                            TransientId = phoneResponse.transientId,
                            TicketQR = phoneResponse.ticketQR
                        });
                    }
                    else
                    {
                        labelError.Text = "Error Try Again...";
                        _log.Error("error message: " + phoneResponse.message);
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

            if (numOpt == "2")
            {
                this.btnPhone.Enabled = true;
                this.btnPhone.Text = tempBtnText;
            }
            else if (numOpt == "3")
            {
                this.btnPhone_3opt.Enabled = true;
                this.btnPhone_3opt.Text = tempBtnText;
            }
        }

        private void btnTicket_Click(object sender, EventArgs e)
        {
            var tempBtnText = this.btnTicket_1opt.Text;
            if (numOpt == "3")
                tempBtnText = this.btnTicket_3opt.Text;

            if (numOpt == "1")
            {
                this.btnTicket_1opt.Text = "SENDING - PLEASE WAIT...";
                this.btnTicket_1opt.Enabled = false;
            }
            else if (numOpt == "3")
            {
                this.btnTicket_3opt.Text = "SENDING - PLEASE WAIT...";
                this.btnTicket_3opt.Enabled = false;
            }

            labelError.Text = "";

            var (ticketRequestSent, ticketResponse) = PangoService.TransientPlateOrPhone(new TransientPlateOrPhoneRequest
            {
                installationID = ConfigurationManager.AppSettings["InstallationId"],
                terminalId = ConfigurationManager.AppSettings["TerminalId"],
                ticket = true
            });

            _log.Debug("Ticket request sent: " + (ticketRequestSent ? "OK" : "NOT OK"));

            if (ticketRequestSent)
            {
                _log.Debug("status received: " + ticketResponse.status);

                if (ticketResponse.status == 1)
                {
                    if (numOpt == "1")
                        this.btnTicket_1opt.Text = "PRINTING TICKET...";
                    else if (numOpt == "3")
                        this.btnTicket_3opt.Text = "PRINTING TICKET...";

                    PangoService.PrintTicketQR(ConfigurationManager.AppSettings["InstallationId"], ConfigurationManager.AppSettings["TerminalId"], ticketResponse.transientId.ToString(), ticketResponse.ticketQR, false, false);

                    OnRegistered(new StatusEventArgs
                    {
                        Type = 3,
                        Status = "OK",
                        TransientId = ticketResponse.transientId,
                        TicketQR = ticketResponse.ticketQR
                    });
                }
                else
                {
                    labelError.Text = "Error Try Again...";
                    _log.Error("error message: " + ticketResponse.message);
                    OnRegistered(new StatusEventArgs
                    {
                        Type = 3,
                        Status = "KO"
                    });
                }
            }

            if (numOpt == "1")
            {
                this.btnTicket_1opt.Enabled = true;
                this.btnTicket_1opt.Text = tempBtnText;
            }
            else if (numOpt == "3")
            {
                this.btnTicket_3opt.Enabled = true;
                this.btnTicket_3opt.Text = tempBtnText;
            }
        }
    }
}
