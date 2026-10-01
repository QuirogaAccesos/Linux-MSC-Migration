using AA.Pango.App.Properties;
using AA.Pango.Model;
using AA.Pango.ServiceLayer;
using AA.Pango.TestWinApp.Utils;
using log4net;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;

namespace AA.Pango.App.V4
{
    /// <summary>
    /// CR-378 Free Flow. Pantalla de entrada automática (sin tocar el touch): muestra la matrícula
    /// leída por la cámara LPR dentro del marco tipo placa y un mensaje WELCOME. El LPR la dispara
    /// vía ChangeScreen (template "freeFlowEntry") tras invocar checkCCTransient. Se autocierra y
    /// vuelve al main. Cámara, barrera y flechas reutilizan los recursos existentes; el marco de la
    /// matrícula es el único recurso nuevo (plateDetectedFrame) y el número se pinta encima.
    /// </summary>
    public partial class FormV4FreeFlowEntry : Form
    {
        protected ILog _log = LogManager.GetLogger("FormV4FreeFlowEntry");
        private Thread Worker { get; set; }
        private System.Resources.ResourceManager RM = new System.Resources.ResourceManager("AA.Pango.App.Properties.Resources", typeof(Resources).Assembly);

        CultureInfo deC = new CultureInfo("en-US");
        private static string dateTimeFormat = ConfigurationManager.AppSettings["DateTimeFormat"];
        private static string footVersion = ConfigurationManager.AppSettings["FootVersion"];

        private string _plate;

        public FormV4FreeFlowEntry()
        {
            InitializeComponent();
        }

        public FormV4FreeFlowEntry(TemplateChangeScreen changeScreenData)
        {
            _log.Debug("changeScreenData - id: " + changeScreenData._id + ", template: " + changeScreenData.TemplateId + ", plate: " + changeScreenData.Plate);

            _plate = changeScreenData.Plate;

            InitializeComponent();

            ChangeScreenService.SetProcessedChangeScreen(changeScreenData._id);

            Thread _thread = new Thread(() =>
            {
                // CR-378: cierre único tras el tiempo configurado. Antes era while(true), lo que dejaba
                // un hilo huérfano vivo por cada apertura (~cientos/día) llamando a CloseForm en bucle.
                Thread.Sleep(changeScreenData.SecondsToShowOnScreen * 1000);

                this.CloseForm();
            });

            _thread.SetApartmentState(ApartmentState.STA);
            _thread.Start();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Init();
            this.Disposed += FormV4FreeFlowEntry_Disposed;
        }

        private void Init()
        {
            InitSettings();
            SetVersion();
            InitWorker();
        }

        private void SetVersion()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            string driverVersion = fvi.FileVersion;

            var version = "Versión: " + driverVersion;

            statusStrip1.Items["tlblVersion"].Text = version;
        }

        private void InitSettings()
        {
            _log.Debug("init settings...");

            LanguageLocalizationParser.LoadLocalizations();

            int terminalId = int.Parse(ConfigurationManager.AppSettings["TerminalId"]);
            var eventNotificationTypeToSend = ConfigurationManager.AppSettings["EventNotificationTypeToSend"] ?? "Entry";

            var descTerminalType = eventNotificationTypeToSend == "Entry" ?
                LanguageLocalizationParser.GetCurrentLanguageTags["EntryTerminalDescription"] :
                LanguageLocalizationParser.GetCurrentLanguageTags["ExitTerminalDescription"];

            #region Style Edits
            this.BackColor = ColorTranslator.FromHtml(DataFileParser.Styles["BackgroundColor"]);
            this.ForeColor = ColorTranslator.FromHtml(DataFileParser.Styles["FontColor"]);

            this.panelFoot.BackColor = ColorTranslator.FromHtml(DataFileParser.Styles["FootBackgroundColor"]);
            this.panelFoot.ForeColor = ColorTranslator.FromHtml(DataFileParser.Styles["FootFontColor"]);

            // Cámara (izquierda) y barrera (derecha): mismos recursos que el resto de pantallas.
            this.pictureBox1.BackgroundImage = (Image)RM.GetObject(DataFileParser.Styles["ImgCamera"]);
            this.pictureBox5.BackgroundImage = (Image)RM.GetObject(DataFileParser.Styles["ImgBarrier"]);

            if (footVersion == "1")
            {
                this.tableLayoutFoot.Visible = true;
                this.imgFootLogoPango.BackgroundImage = (Image)RM.GetObject(DataFileParser.Styles["FootLogoPango"]);
                this.tableLayoutFoot2.Visible = false;
            }
            else
            {
                this.tableLayoutFoot.Visible = false;
                this.tableLayoutFoot2.Visible = true;
                this.imgFootLogoPango2.BackgroundImage = (Image)RM.GetObject(DataFileParser.Styles["FootLogoPango"]);
                this.imgFootLogo2.BackgroundImage = (Image)RM.GetObject(DataFileParser.Styles["FootLogoV2"]);
                this.imgFootBtn24h.BackgroundImage = (Image)RM.GetObject(DataFileParser.Styles["FootBtn24h"]);
            }

            #endregion Style Edits

            this.lblStatusOK.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4FreeFlowEntry_Status"];
            this.lblFootDownload.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4_DownloadApp"];
            this.lblFootPrice.Text = LanguageLocalizationParser.GetCurrentLanguageTags["FormV4_Price"];

            this.tlblTerminalId.Text = "Terminal: " + terminalId + ". " + descTerminalType;
            this.tlblDate.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            this.lblCurrentDate.Text = DateTime.Now.ToString(dateTimeFormat);
            this.lblFootDate2.Text = DateTime.Now.ToString(dateTimeFormat);

            this.tlblStatusTerminal.Text = "";

            this.picPlate.Invalidate();
        }

        /// <summary>
        /// Pinta la matrícula leída por LPR dentro del marco (zona blanca inferior), en negrita,
        /// centrada y al máximo tamaño posible sin que se salga (hasta 7 dígitos).
        /// </summary>
        private void picPlate_Paint(object sender, PaintEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_plate))
                return;

            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

            var rect = new RectangleF(picPlate.Width * 0.07f, picPlate.Height * 0.46f, picPlate.Width * 0.86f, picPlate.Height * 0.44f);

            using (var brush = new SolidBrush(ColorTranslator.FromHtml("#14304A")))
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center, FormatFlags = StringFormatFlags.NoWrap })
            {
                float fontSize = rect.Height;
                Font f = null;
                while (fontSize > 12f)
                {
                    f = new Font("Calibri", fontSize, FontStyle.Bold, GraphicsUnit.Pixel);
                    SizeF sz = g.MeasureString(_plate, f);
                    if (sz.Width <= rect.Width && sz.Height <= rect.Height)
                        break;
                    f.Dispose();
                    f = null;
                    fontSize -= 4f;
                }
                if (f == null)
                    f = new Font("Calibri", 12f, FontStyle.Bold, GraphicsUnit.Pixel);

                g.DrawString(_plate, f, brush, rect, sf);
                f.Dispose();
            }
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

                    try
                    {
                        if (!statusStrip1.IsDisposed)
                        {
                            if (statusStrip1.InvokeRequired)
                            {
                                statusStrip1.Invoke((MethodInvoker)(() =>
                                {
                                    statusStrip1.Items["tlblDate"].Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
                                }));
                            }
                            else
                            {
                                statusStrip1.Items["tlblDate"].Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
                            }
                        }

                        if (footVersion == "1")
                        {
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
                        }
                        else
                        {
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
                        }
                    }
                    catch (Exception e)
                    {
                        _log.Error("Unexpected error in Worker thread", e);
                        CloseForm();
                    }
                    finally
                    {
                        Thread.Sleep(60000);
                    }
                }
            })));

            Worker.SetApartmentState(ApartmentState.STA);
            Worker.Start();
        }

        private void FormV4FreeFlowEntry_Disposed(object sender, EventArgs e)
        {
            _log.Error("FORM DISPOSED!!!");
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
