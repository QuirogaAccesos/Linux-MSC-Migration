using AA.Pango.Model;
using AA.Pango.ServiceLayer;
using AA.Pango.ServiceLayer.ApiDtos;
using AA.ParkingStation.Service;
using log4net;
using Swashbuckle.Swagger;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Drawing.Printing;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Description;
using ZXing;
using ZXing.Common;
using ZXing.Rendering;
using ZXing.QrCode.Internal;
using AA.Pango.RestApi.Services;
using log4net.Util;
using System.Threading.Tasks;

namespace AA.Pango.RestApi.Controllers
{
    /// <summary>
    /// API controller service for the terminal methods
    /// </summary>
    public class RestController : ApiController
    {

        private static readonly byte[] ResetCommand = { 0x45 }; //E //RESET
        private static ILog Log = LogManager.GetLogger(typeof(RestController));

        /// <summary>
        /// Forces Opening a Barrier for a terminal
        /// </summary>
        /// <param name="installationId">the installation id</param>
        /// <param name="terminalId">the terminal id</param>
        /// <returns></returns>
        [Route("PostForceBarrier")]
        [HttpPost]
        [ResponseType(typeof(PostForceBarrierResponse))]
        public IHttpActionResult PostForceBarrier([FromUri] string installationId, string terminalId, string serialNumber)
        {
            Log.Info("PostForceBarrier - PARAM: installationId:" + installationId + " - terminalId" + terminalId);
            try
            {
                var terminal = TerminalService.GetTerminal(
                   terminalId,
                   serialNumber,
                   installationId);

                if (terminal != null)
                {
                    terminal.ForceOpenBarrier = true;
                    var result = TerminalService.UpdateTerminal(terminal);

                    return Ok(new PostForceBarrierResponse
                    {
                        Success = result,
                        ResultCode = result ? "ACK" : "NACK",
                        Description = result ? "Success" : "Failed",
                        InstallationId = installationId,
                        TerminalId = terminalId,
                        SerialNumber = serialNumber
                    });
                }
                else
                {
                    return Ok(new PostForceBarrierResponse { Success = false, ResultCode = "NACK", Description = "Failed" });
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Forces the whitelist upload for a terminal
        /// </summary>
        /// <param name="installationId">the installation id</param>
        /// <param name="terminalId">the terminal id</param>
        /// <param name="serialNumber">the serial number</param>
        /// <returns></returns>
        [Route("PostForceWhitelistUpload")]
        [HttpPost]
        [ResponseType(typeof(PostForceWhitelistUploadResponse))]
        public IHttpActionResult PostForceWhitelistUpload([FromUri] string installationId, string terminalId, string serialNumber)
        {
            Log.Info("PostForceWhitelistUpload - PARAM: installationId:" + installationId + " - terminalId" + terminalId);
            try
            {
                var terminal = TerminalService.GetTerminal(
                    terminalId.ToString(),
                    serialNumber.ToString(),
                    installationId.ToString());

                if (terminal != null)
                {
                    terminal.ForceWhitelistUpload = true;
                    var result = TerminalService.UpdateTerminal(terminal);

                    return Ok(new PostForceWhitelistUploadResponse
                    {
                        Success = result,
                        ResultCode = result ? "ACK" : "NACK",
                        InstallationId = installationId,
                        TerminalId = terminalId,
                        SerialNumber = serialNumber
                    });
                }
                else
                {
                    return Ok(new PostForceWhitelistUploadResponse { Success = false, ResultCode = "NACK" });
                }
            }catch (Exception ex)
            {
                Log.Error(ex);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Shows the stored whitelist information
        /// </summary>
        /// <param name="installationId">the installation id</param>
        /// <param name="terminalId">the terminal id</param>
        /// <returns></returns>
        [ResponseType(typeof(IEnumerable<AuthorizedTicket>))]
        [Route("PostShowWhitelist")]
        [HttpPost]
        public IHttpActionResult PostShowWhitelist([FromUri] int installationId, int terminalId)
        {
            Log.Info("PostShowWhitelist - PARAM: installationId:" + installationId + " - terminalId" + terminalId);
            try
            {
                var tickets = TicketService.GetTicketsByTerminal(terminalId);
                return Ok(tickets);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Updates the ticket information in the local database
        /// </summary>
        /// <param name="ticket"></param>
        /// <returns></returns>
        [Route("PutTicket")]
        [HttpPut]
        public IHttpActionResult PutTicket([FromBody] AuthorizedTicket ticket)
        {
            Log.Info("PutTicket - PARAM: ticket:" + ticket.CustomerIdReceived + " - terminalId" + ticket.TerminalId);
            try
            {
                if (ticket != null && !string.IsNullOrEmpty(ticket.CustomerIdReceived))
                {
                    var persistentTicket = TicketService.GetTicketByCode(ticket.CustomerIdReceived);

                    if (persistentTicket != null)
                    {
                        //update fields
                        persistentTicket.HasBeenUsed = ticket.HasBeenUsed;

                        var updateResponse = TicketService.UpdateTicket(persistentTicket);
                        return Ok(new { Success = updateResponse });
                    }
                    else
                    {
                        return Ok(new { Success = false, Message = "ticket code not found on terminal." });
                    }
                }
                else
                {
                    return Ok(new { Success = false, Message = "no ticket data received" });
                }
            }catch (Exception ex)
            {
                Log.Error(ex);
                return BadRequest(ex.Message);
            }
        }


        /// <summary>
        /// Creates a charge plate request into the current terminal
        /// </summary>
        /// <param name="request">Request data</param>
        /// <returns>Response data indicating if the operation succeeded.</returns>
        [Route("ChargePlate")]
        [HttpPost]
        public IHttpActionResult PostChargePlate([FromBody] ChargePlateRequest request)
        {
            
            try
            {
                Log.Debug("Request received: " + Newtonsoft.Json.JsonConvert.SerializeObject(request));
            }
            catch (Exception e)
            {
                Log.Warn(e);
            }

            Log.Info("PostChargePlate - PARAM: TransientID:" + request.TransientID + " - terminalId" + request.TerminalId + " - plate" + request.Plate);

            if (request.Amount <= 0M)
            {
                return Ok(new { Success = false, Message = "Amount to charge in the request has to be greater than zero." });
            }

            var paymentAttempt = new PaymentAttempt
            {
                _id = Guid.NewGuid(),
                AmountToCharge = request.Amount,
                Currency = request.Currency ?? "USD",
                Date = DateTime.Now,
                Processed = false,
                TerminalId = request.TerminalId,
                InstallationId = request.InstallationID,
                Plate = request.Plate,
                TransientId = request.TransientID,
                Phone = request.Phone
            };

            if (request.ParamList?.Any() == true)
            {

                var parkingFee = request.ParamList.FirstOrDefault(x => x.param_name == "parkingFee")?.param_value ?? "";
                var convFee = request.ParamList.FirstOrDefault(x => x.param_name == "convFee")?.param_value ?? "";
                var entryDate = request.ParamList.FirstOrDefault(x => x.param_name == "entryDate")?.param_value ?? "";
                var entryTime = request.ParamList.FirstOrDefault(x => x.param_name == "entryTime")?.param_value ?? "";
                var feePct = request.ParamList.FirstOrDefault(x => x.param_name == "convFeePercent")?.param_value ?? "";
                var showTime = int.Parse(request.ParamList.FirstOrDefault(x => x.param_name == "showTime")?.param_value ?? "40");
                var priceDetail = request.ParamList.FirstOrDefault(x => x.param_name == "priceDetail")?.param_value ?? "";

                paymentAttempt.TransactionFee = convFee;
                paymentAttempt.EntryDate = entryDate;
                paymentAttempt.EntryTime = entryTime;
                paymentAttempt.TransactionPercentage = feePct;
                paymentAttempt.ParkingFee = parkingFee;
                paymentAttempt.ShowTime = showTime;
                paymentAttempt.PriceDetail = priceDetail;
            }

            var response = PaymentService.UpsertPaymentAttempt(paymentAttempt);

            return Ok(new { Success = response, Message = "" });
        }


        /// <summary>
        /// Creates a change screen request for the current terminal.
        /// </summary>
        /// <param name="request">Request data.</param>
        /// <returns>Response data indicating if the operation succeeded.</returns>
        [Route("ChangeScreen")]
        [HttpPost]
        public IHttpActionResult PutChangeScreen([FromBody] ChangeScreenRequest request)
        {
            Log.Info("PutChangeScreen - PARAM: TemplateId:" + request.TemplateId + " - terminalId" + request.TerminalId + " - InstallationId" + request.InstallationId);

            try
            {
                Log.Debug("Request received: " + Newtonsoft.Json.JsonConvert.SerializeObject(request));
            }
            catch (Exception e)
            {
                Log.Warn(e);
            }

            try { 
                var result = false;

                var templateScreen = new TemplateChangeScreen
                {
                    _id = Guid.NewGuid(),
                    Date = DateTime.Now,
                    Processed = false,
                    TerminalId = request.TerminalId,
                    TemplateId = request.TemplateId, 
                    InstallationId = request.InstallationId.ToString(),
                };
       
                if (request?.ParamList?.Any() == true)
                {

                    var tmplShowTime = request?.ParamList?.FirstOrDefault(x => x.param_name == "showTime");

                    if (tmplShowTime != null)
                    {
                        templateScreen.SecondsToShowOnScreen = int.Parse(tmplShowTime.param_value);
                    }

                    var tmplWaitUser = request?.ParamList?.FirstOrDefault(x => x.param_name == "waitUserResponse");

                    if (tmplWaitUser != null)
                    {
                        templateScreen.RequiresUserResponse = tmplWaitUser.param_value == "1";
                    }

                    // CR-378 Free Flow: matrícula leída por LPR para mostrarla en la pantalla de entrada.
                    var tmplPlate = request?.ParamList?.FirstOrDefault(x => x.param_name == "plate");

                    if (tmplPlate != null)
                    {
                        templateScreen.Plate = tmplPlate.param_value;
                    }
                }

                result = ChangeScreenService.UpsertTemplateChangeScreen(templateScreen);
                
                return Ok(new { Success = result, Message = "" });
            }catch (Exception ex)
            {
                Log.Error(ex);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get the payments stored on the current terminal.
        /// </summary>
        /// <param name="installationId">Installation ID</param>
        /// <param name="terminalId">Terminal ID</param>
        /// <param name="showValidatedOnly">Return validated payments only</param>
        /// <returns></returns>
        [ResponseType(typeof(IEnumerable<AuthorizedTicket>))]
        [Route("GetTickets/{installationId}/{terminalId}/{showValidatedOnly}")]
        [HttpGet]
        public IHttpActionResult GetPayments(string installationId, string terminalId, bool showValidatedOnly)
        {
            Log.Info("GetPayments - PARAM:  - terminalId" + terminalId + " - InstallationId" + installationId);
            try
            {
                var payments = PaymentService.GetPayments(installationId, terminalId, showValidatedOnly);
                return Ok(new { Success = true, Payments = payments });
            }catch (Exception ex)
            {
                Log.Error(ex);
                return BadRequest(ex.Message);
             }
        }


        /// <summary>
        /// Forces Reboot for a terminal
        /// </summary>
        /// <param name="installationId">the installation id</param>
        /// <param name="terminalId">the terminal id</param>
        /// <returns></returns>
        [Route("PostForceReboot")]
        [HttpPost]
        public IHttpActionResult PostForceReboot([FromUri] string installationId, string terminalId)
        {
            Log.Info("PostForceReboot - PARAM: installationId:" + installationId + " - terminalId" + terminalId);
            try
            {
                Process.Start(ConfigurationManager.AppSettings["RebootTerminal"]);
                return Ok(new { Success = true, Message = "Launched successfully rebooted" });                
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Forces ScreenShot capture
        /// </summary>
        /// <param name="installationId">the installation id</param>
        /// <param name="terminalId">the terminal id</param>
        /// <returns></returns>
        [Route("PostScreenShot")]
        [HttpPost]
        public IHttpActionResult PostScreenShot([FromUri] string installationId, string terminalId)
        {
            Log.Info("PostScreenShot - PARAM: installationId:" + installationId + " - terminalId" + terminalId);
            try
            {
                string path = ConfigurationManager.AppSettings["PythonPath"];
                try
                {
                    // Create the file, or overwrite if the file exists.
                    using (FileStream fs = File.Create(path))
                    {
                        byte[] info = new UTF8Encoding(true).GetBytes("This is some text in the file for screenshot.");
                        // Add some information to the file.
                        fs.Write(info, 0, info.Length);
                    }

                    // Open the stream and read it back.
                    using (StreamReader sr = File.OpenText(path))
                    {
                        string s = "";
                        while ((s = sr.ReadLine()) != null)
                        {
                            Log.Info(s);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
                return Ok(new { Success = true, Message = "Successfully screenshot" });
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return BadRequest(ex.Message);
            }
        }


        /// <summary>
        /// Forces ScreenShot capture
        /// </summary>
        /// <param name="installationId">the installation id</param>
        /// <param name="terminalId">the terminal id</param>
        /// <returns></returns>
        [Route("PostHardReset")]
        [HttpPost]
        public IHttpActionResult PostHardReset([FromUri] string installationId, string terminalId)
        {
            Log.Info("PostHardReset - PARAM: installationId:" + installationId + " - terminalId" + terminalId);
            try
            {
                var terminal = TerminalService.GetTerminal(terminalId);
                if (terminal != null)
                {
                    Log.Info("HardReset -- Close APP");
                    Process.Start(ConfigurationManager.AppSettings["CloseApp"]);
                    Thread.Sleep(2000);
                    Task.Run(() => HardResetService.HardReset());
                    Log.Info("HardReset -- OK");
                    return Ok(new { Success = true, Message = "Successfully Hard Reset" });                    
                }
                else
                {
                    return BadRequest("TERMINAL NO VALIDO");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return BadRequest(ex.Message);
            }
        }



        /// <summary>
        /// Print Ticket
        /// </summary>
        /// <param name="terminalId"></param>
        /// <param name="transientId"></param>
        /// <returns></returns>
        [Route("PrintTicket")]
        [HttpPost]
        public IHttpActionResult PrintTicket([FromUri] string terminalId, string transientId)
        {
            Log.Info("PrintTicket - PARAM: transientId:" + transientId + " - terminalId" + terminalId);
            try
            {

                Log.Debug("Printing ticket...");

                Bitmap bitmap = null;
                string[] header = PrinterService.GetTicketHeader();
                string[] footer = PrinterService.GetTicketFooter();

                header = header.Append("                Ticket ID: " + transientId).ToArray();
                header = header.Append("       Date: " + DateTime.Now.ToString("MMM dd, yyyy hh:mm tt")).ToArray();

                StringBuilder sbTextoHeader = new StringBuilder();
                foreach (string str in header)
                {
                    sbTextoHeader.AppendLine(str);
                }

                StringBuilder sbTextoFooter = new StringBuilder();
                foreach (string str in footer)
                {
                    sbTextoFooter.AppendLine(str);
                }

                #region Print QR Code
                var qrCodeWidth = int.Parse(ConfigurationManager.AppSettings["TicketQRWidth"] ?? "150");
                var qrCodeHeight = int.Parse(ConfigurationManager.AppSettings["TicketQRHeight"] ?? "150");

                BarcodeWriter barcodeWriter = new BarcodeWriter();
                EncodingOptions encodingOptions = new EncodingOptions()
                {
                    Width = qrCodeWidth,
                    Height = qrCodeHeight,
                    Margin = 0,
                    PureBarcode = false
                };
                encodingOptions.Hints.Add(EncodeHintType.ERROR_CORRECTION, ErrorCorrectionLevel.H);
                barcodeWriter.Renderer = new BitmapRenderer();
                barcodeWriter.Options = encodingOptions;
                barcodeWriter.Format = BarcodeFormat.QR_CODE;
                string ticketQR = PrinterService.generateTicketQR(int.Parse(transientId));
                bitmap = barcodeWriter.Write(ticketQR);
                #endregion

                int lineHeight = 10;
                int pointYQR = 50;
                float pointYFooter = 0;
                pointYQR = lineHeight * (header.Length + 1);
                pointYFooter = pointYQR + qrCodeHeight + lineHeight;

                PrintDocument printDocument = new PrintDocument();
                printDocument.PrintPage +=
                    new PrintPageEventHandler((object sender, PrintPageEventArgs e) =>
                    {
                        e.Graphics.DrawString(sbTextoHeader.ToString(), new Font("Arial", 7), Brushes.Black, 3f, 0.0f, new StringFormat());
                        e.Graphics.DrawImage(bitmap, new Point(5, pointYQR));
                        e.Graphics.DrawString(sbTextoFooter.ToString(), new Font("Arial", 7), Brushes.Black, 3f, pointYFooter, new StringFormat());
                    });

                try
                {
                    printDocument.Print();
                }
                catch (InvalidPrinterException ipe)
                {
                    Log.Error("PRINTER ERROR", ipe);
                    return Ok(new { Success = false, Message = ipe });
                }
                catch (Exception e)
                {
                    Log.Error(e);
                    return Ok(new { Success = false, Message = e });
                }

                sbTextoHeader.Clear();
                sbTextoFooter.Clear();

                Log.DebugFormat("Ticket printed successfully");

               
                return Ok(new { Success = true, Message = "Ticket printed successfully" });
               
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return BadRequest(ex.Message);
            }
        }
    }
}