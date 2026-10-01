using AA.Pango.Model;
using AA.Pango.ServiceLayer.ApiDto;
using AA.Pango.ServiceLayer.ApiDtos;
using AA.ParkingStation.Service;
using log4net;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using ZXing;
using ZXing.Common;
using ZXing.QrCode.Internal;
using ZXing.Rendering;
using static AA.Pango.ServiceLayer.ApiDto.CheckQRCode;
using static AA.Pango.ServiceLayer.ApiDto.WhiteListData;

namespace AA.Pango.ServiceLayer
{
    public static class PangoService
    {
        private static ILog log = LogManager.GetLogger(typeof(PangoService));

        public static Dictionary<string, string> TicketInfo = new Dictionary<string, string>();

        public static CheckQRCodeResponse CheckQR(CheckQRCodeRequest requestData)
        {
            var url = ConfigurationManager.AppSettings["PangoUrl"] + "checkQRcode";
            var token = ConfigurationManager.AppSettings["PangoToken"];
            var client = new RestClient(url);
            client.Timeout = 30000;
            var request = new RestRequest(Method.POST);
            request.AddHeader("X-QR-KEY", token);
            request.AddJsonBody(requestData);
            IRestResponse response = client.Execute(request);
            LogRestResponse(response, "checkQRcode");
            CheckQRCodeResponse res = null;

            try
            {
                res = Newtonsoft.Json.JsonConvert.DeserializeObject<CheckQRCodeResponse>(response.Content);
            }
            catch (Exception e)
            {
                log.Error(e);
                return null;
            }

            return res;
        }

        public static bool DeleteExpiredTickets()
        {
            DeleteTicketInfo info = TicketService.DeleteExpiredTickets();
            log.Debug(info.ToString());
            return info.IsSuccess();
        }

        /// <summary>
        /// Downloads the whitelist from the API and inserts the information into the local database.
        /// </summary>
        /// <param name="requestData">the request parameters</param>
        /// <returns></returns>
        public static bool DownloadWhitelist(WhiteListRequest requestData)
        {
            var url = ConfigurationManager.AppSettings["PangoUrl"] + "getWhitelist";
            var token = ConfigurationManager.AppSettings["PangoToken"];

            log.Debug($"Calling {url}...");

            var client = new RestClient(url);
            client.Timeout = 30000;
            var request = new RestRequest(Method.POST);
            request.AddHeader("X-QR-KEY", token);
            request.AddJsonBody(requestData);
            IRestResponse response = client.Execute(request);
            if (response?.IsSuccessful == true)
            {
                log.Debug("Successful response from Pango API getWhitelist");
                //LogRestResponse(response);
            }

            WhiteListResponse res = null;

            try
            {
                res = Newtonsoft.Json.JsonConvert.DeserializeObject<WhiteListResponse>(response?.Content);
            }
            catch (Exception e)
            {
                log.Error(e);
                return false;
            }

            if (res != null && res.listCredentials.Any())
            {

                log.DebugFormat("Downloaded {0} authorizations!", res.listCredentials.Count);

                foreach (var item in res.listCredentials)
                {
                    var ticket = new AuthorizedTicket
                    {
                        IsValid = true,
                        StartDate = item.start_time,
                        EndDate = item.end_time,
                        TerminalId = int.Parse(requestData.terminalId),
                        HasBeenUsed = false,
                        CustomerIdReceived = item.code,
                        EventDate = DateTime.UtcNow
                    };

                    var insertedTicket = TicketService.GetTicketByCode(item.code);

                    if (insertedTicket == null)
                    {
                        if (TicketService.InsertTicket(ticket) > 0)
                        {
                            //log.DebugFormat("Ticket inserted locally successfully. Code {0}", ticket.CustomerIdReceived);
                        }
                        else
                        {
                            log.WarnFormat("Ticket NOT inserted locally. Code {0}", ticket.CustomerIdReceived);
                        }
                    }
                    else
                    {
                        //log.DebugFormat("Ticket with code {0} already exists, updating data", item.code);

                        insertedTicket.StartDate = item.start_time;
                        insertedTicket.EndDate = item.end_time;

                        if (TicketService.UpdateTicket(insertedTicket))
                        {
                            //log.DebugFormat("Ticket updated locally successfully. Code {0}", ticket.CustomerIdReceived);
                        }
                        else
                        {
                            log.WarnFormat("Ticket NOT updated locally. Code {0}", ticket.CustomerIdReceived);
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="requestData"></param>
        /// <returns></returns>
        public static bool ForceWhitelistUpload(WhiteListRequest requestData)
        {
            var url = ConfigurationManager.AppSettings["PangoUrl"] + "forceWhiteListUpload";
            var token = ConfigurationManager.AppSettings["PangoToken"];

            var client = new RestClient(url);
            client.Timeout = 30000;
            var request = new RestRequest(Method.POST);
            request.AddHeader("X-QR-KEY", token);
            request.AddJsonBody(requestData);
            IRestResponse response = client.Execute(request);
            if (response != null)
            {
                log.Debug("Got response from forceWhiteListUpload");
                LogRestResponse(response, "forceWhiteListUpload");
            }

            WhiteListResponse res = null;

            try
            {
                res = Newtonsoft.Json.JsonConvert.DeserializeObject<WhiteListResponse>(response?.Content);
            }
            catch (Exception e)
            {
                log.Error(e);
                return false;
            }

            log.Debug("Response Content was parsed successfully to DTO");

            if (res != null && res.listCredentials.Any())
            {
                foreach (var item in res.listCredentials)
                {
                    var ticket = new AuthorizedTicket
                    {
                        IsValid = true,
                        StartDate = item.start_time,
                        EndDate = item.end_time,
                        TerminalId = int.Parse(requestData.terminalId),
                        HasBeenUsed = false,
                        CustomerIdReceived = item.code,
                        EventDate = DateTime.UtcNow
                    };

                    var insertedTicket = TicketService.GetTicketByCode(item.code);

                    if (insertedTicket == null)
                    {
                        if (TicketService.InsertTicket(ticket) > 0)
                        {
                            log.DebugFormat("Ticket inserted locally successfully. Code {0}", ticket.CustomerIdReceived);
                        }
                        else
                        {
                            log.DebugFormat("Ticket NOT inserted locally. Code {0}", ticket.CustomerIdReceived);
                        }
                    }
                    else
                    {
                        log.DebugFormat("Ticket with code {0} already exists", item.code);
                    }
                }
            }

            return true;
        }

        public static string PostEventNotification(NotificationRequest requestData)
        {
            var url = ConfigurationManager.AppSettings["PangoUrl"] + "eventNotification";
            var token = ConfigurationManager.AppSettings["PangoToken"];

            var client = new RestClient(url);
            client.Timeout = 30000;
            var request = new RestRequest(Method.POST);
            request.AddHeader("X-QR-KEY", token);
            request.AddJsonBody(requestData);
            IRestResponse response = client.Execute(request);
            LogRestResponse(response, "eventNotification");
            return response.Content;
        }

        public static string ForceOpenBarrier(NotificationRequest requestData)
        {
            var url = ConfigurationManager.AppSettings["PangoUrl"] + "forceOpenBarrier";
            var token = ConfigurationManager.AppSettings["PangoToken"];

            var client = new RestClient(url);
            client.Timeout = 30000;
            var request = new RestRequest(Method.POST);
            request.AddHeader("X-QR-KEY", token);
            request.AddJsonBody(requestData);
            IRestResponse response = client.Execute(request);
            LogRestResponse(response, "forceOpenBarrier");
            return response.Content;
        }

        public static bool CheckPlate(CheckPlateRequest requestData)
        {
            var url = ConfigurationManager.AppSettings["PangoUrl"] + "checkPlate";
            var token = ConfigurationManager.AppSettings["PangoToken"];
            var client = new RestClient(url);
            client.Timeout = 30000;
            var request = new RestRequest(Method.POST);
            request.AddHeader("X-QR-KEY", token);
            request.AddJsonBody(requestData);
            IRestResponse response = client.Execute(request);
            LogRestResponse(response, "checkPlate");
            return response?.IsSuccessful == true && response?.StatusCode == System.Net.HttpStatusCode.NoContent;
        }

        public static (bool, ChargePlateResponse) ChargePlate(ChargePlateRequest requestData)
        {
            var url = ConfigurationManager.AppSettings["PangoUrl"] + "checkPlate";
            var token = ConfigurationManager.AppSettings["PangoToken"];
            var client = new RestClient(url);
            client.Timeout = 30000;
            var request = new RestRequest(Method.POST);
            request.AddHeader("X-QR-KEY", token);
            request.AddJsonBody(requestData);
            IRestResponse response = client.Execute(request);

            LogRestResponse(response, "ChargePlate");

            ChargePlateResponse res = null;

            try
            {
                res = Newtonsoft.Json.JsonConvert.DeserializeObject<ChargePlateResponse>(response.Content);
            }
            catch (Exception e)
            {
                log.Error(e);
            }

            return (response?.IsSuccessful == true && response?.StatusCode == System.Net.HttpStatusCode.OK, res);

        }

        public static (bool, ChargePlateResponse) ChargePhone(ChargePhoneRequest requestData)
        {
            var url = ConfigurationManager.AppSettings["PangoUrl"] + "checkPhone";
            var token = ConfigurationManager.AppSettings["PangoToken"];
            var client = new RestClient(url);
            client.Timeout = 30000;
            var request = new RestRequest(Method.POST);
            request.AddHeader("X-QR-KEY", token);
            request.AddJsonBody(requestData);
            IRestResponse response = client.Execute(request);

            LogRestResponse(response, "checkPhone");

            ChargePlateResponse res = null;

            try
            {
                res = Newtonsoft.Json.JsonConvert.DeserializeObject<ChargePlateResponse>(response.Content);
            }
            catch (Exception e)
            {
                log.Error(e);
            }

            return (response?.IsSuccessful == true && response?.StatusCode == System.Net.HttpStatusCode.OK, res);
        }

        public static (bool, ChargePlateResponse) ChargePlatePhone(ChargePlatePhoneRequest requestData)
        {
            var url = ConfigurationManager.AppSettings["PangoUrl"] + "chargePlateAndPhone";
            var token = ConfigurationManager.AppSettings["PangoToken"];
            var client = new RestClient(url);
            client.Timeout = 30000;
            var request = new RestRequest(Method.POST);
            request.AddHeader("X-QR-KEY", token);
            request.AddJsonBody(requestData);
            IRestResponse response = client.Execute(request);

            LogRestResponse(response, "ChargePlatePhone");

            ChargePlateResponse res = null;

            try
            {
                res = JsonConvert.DeserializeObject<ChargePlateResponse>(response.Content);
            }
            catch (Exception e)
            {
                log.Error(e);
            }

            return (response?.IsSuccessful == true && response?.StatusCode == System.Net.HttpStatusCode.OK, res);

        }

        public static bool ChangeScreen(ChangeScreenRequest requestData)
        {
            var url = ConfigurationManager.AppSettings["PangoUrl"] + "changeScreen";
            var token = ConfigurationManager.AppSettings["PangoToken"];
            var client = new RestClient(url);
            client.Timeout = 30000;
            var request = new RestRequest(Method.POST);
            request.AddHeader("X-QR-KEY", token);
            request.AddJsonBody(requestData);
            IRestResponse response = client.Execute(request);
            LogRestResponse(response, "changeScreen");
            return response?.IsSuccessful == true && response?.StatusCode == System.Net.HttpStatusCode.NoContent;
        }

        public static bool PostChargePlateResult(ChargePlateResultRequest requestData)
        {
            var url = ConfigurationManager.AppSettings["PangoUrl"] + "chargePlateResult";
            var token = ConfigurationManager.AppSettings["PangoToken"];
            var client = new RestClient(url);
            client.Timeout = 30000;
            var request = new RestRequest(Method.POST);
            request.AddHeader("X-QR-KEY", token);
            request.AddJsonBody(requestData);
            IRestResponse response = client.Execute(request);

            LogRestResponse(response, "chargePlateResult");

            return response?.IsSuccessful == true && response?.StatusCode == System.Net.HttpStatusCode.NoContent;

        }

        /// <summary>
        /// Check Credit Card Transient (Pay By Plate read by LPR Camera)
        /// </summary>
        /// <param name="requestData"></param>
        /// <returns></returns>
        public static (bool, CheckCCTransientResponse) CheckCCTransient(CheckCCTransientRequest requestData)
        {
            var url = ConfigurationManager.AppSettings["PangoUrl"] + "checkCCTransient";
            var token = ConfigurationManager.AppSettings["PangoToken"];
            var client = new RestClient(url);
            client.Timeout = 30000;
            var request = new RestRequest(Method.POST);
            request.AddHeader("X-QR-KEY", token);
            request.AddJsonBody(requestData);
            IRestResponse response = client.Execute(request);

            LogRestResponse(response, "checkCCTransient");

            CheckCCTransientResponse res = null;

            try
            {
                res = Newtonsoft.Json.JsonConvert.DeserializeObject<CheckCCTransientResponse>(response.Content);
            }
            catch (Exception e)
            {
                log.Error(e);
            }

            return (response?.IsSuccessful == true && response?.StatusCode == System.Net.HttpStatusCode.OK, res);
        }

        public static (bool, TransientPlateOrPhoneResponse) TransientPlateOrPhone(TransientPlateOrPhoneRequest requestData)
        {
            var url = ConfigurationManager.AppSettings["PangoUrl"] + "transientPlateOrPhone";
            var token = ConfigurationManager.AppSettings["PangoToken"];
            var client = new RestClient(url);
            client.Timeout = 30000;
            var request = new RestRequest(Method.POST);
            request.AddHeader("X-QR-KEY", token);
            request.AddJsonBody(requestData);
            IRestResponse response = client.Execute(request);

            LogRestResponse(response, "transientPlateOrPhone");

            TransientPlateOrPhoneResponse res = null;

            try
            {
                res = Newtonsoft.Json.JsonConvert.DeserializeObject<TransientPlateOrPhoneResponse>(response.Content);
            }
            catch (Exception e)
            {
                log.Error(e);
            }

            return (response?.IsSuccessful == true && response?.StatusCode == System.Net.HttpStatusCode.OK, res);
        }

        public static bool TapForTicket(TapForTicketRequest requestData)
        {
            var url = ConfigurationManager.AppSettings["PangoUrl"] + "tapForTicket";
            var token = ConfigurationManager.AppSettings["PangoToken"];
            var client = new RestClient(url);
            client.Timeout = 30000;
            var request = new RestRequest(Method.POST);
            request.AddHeader("X-QR-KEY", token);
            request.AddJsonBody(requestData);
            IRestResponse response = client.Execute(request);
            LogRestResponse(response, "tapForTicket");
            return response?.IsSuccessful == true && response?.StatusCode == System.Net.HttpStatusCode.NoContent;
        }

        public static bool TapForReceipt(TapForReceiptRequest requestData)
        {
            var url = ConfigurationManager.AppSettings["PangoUrl"] + "tapForReceipt";
            var token = ConfigurationManager.AppSettings["PangoToken"];
            var client = new RestClient(url);
            client.Timeout = 30000;
            var request = new RestRequest(Method.POST);
            request.AddHeader("X-QR-KEY", token);
            request.AddJsonBody(requestData);
            IRestResponse response = client.Execute(request);
            LogRestResponse(response, "tapForReceipt");
            return response?.IsSuccessful == true && response?.StatusCode == System.Net.HttpStatusCode.NoContent;
        }

        public static (bool, CheckPermitPlateResponse) CheckPermitPlate(CheckPermitPlateRequest requestData)
        {
            var url = ConfigurationManager.AppSettings["PangoUrl"] + "checkPermitPlate";
            var token = ConfigurationManager.AppSettings["PangoToken"];
            var client = new RestClient(url);
            client.Timeout = 30000;
            var request = new RestRequest(Method.POST);
            request.AddHeader("X-QR-KEY", token);
            request.AddJsonBody(requestData);
            IRestResponse response = client.Execute(request);
            LogRestResponse(response, "checkPermitPlate");

            CheckPermitPlateResponse res = null;

            try
            {
                res = JsonConvert.DeserializeObject<CheckPermitPlateResponse>(response.Content);
            }
            catch (Exception e)
            {
                log.Error(e);
            }

            return (response?.IsSuccessful == true && response?.StatusCode == System.Net.HttpStatusCode.OK, res);
        }

        public static (bool, ChargePermitExpiredResponse) ChargePermitExpired(ChargePermitExpiredRequest requestData)
        {
            var url = ConfigurationManager.AppSettings["PangoUrl"] + "chargePermitExpired";
            var token = ConfigurationManager.AppSettings["PangoToken"];
            var client = new RestClient(url);
            client.Timeout = 30000;
            var request = new RestRequest(Method.POST);
            request.AddHeader("X-QR-KEY", token);
            request.AddJsonBody(requestData);
            IRestResponse response = client.Execute(request);
            LogRestResponse(response, "chargePermitExpired");

            ChargePermitExpiredResponse res = null;

            try
            {
                res = JsonConvert.DeserializeObject<ChargePermitExpiredResponse>(response.Content);
            }
            catch (Exception e)
            {
                log.Error(e);
            }

            return (response?.IsSuccessful == true && response?.StatusCode == System.Net.HttpStatusCode.OK, res);
        }

        public static (bool, ChargePangoPassResponse) ChargePangoPass(ChargePangoPassRequest requestData)
        {
            var url = ConfigurationManager.AppSettings["PangoUrl"] + "chargePangoPass";
            var token = ConfigurationManager.AppSettings["PangoToken"];
            var client = new RestClient(url);
            client.Timeout = 30000;
            var request = new RestRequest(Method.POST);
            request.AddHeader("X-QR-KEY", token);
            request.AddJsonBody(requestData);
            IRestResponse response = client.Execute(request);
            LogRestResponse(response, "chargePangoPass");

            ChargePangoPassResponse res = null;

            try
            {
                res = JsonConvert.DeserializeObject<ChargePangoPassResponse>(response.Content);
            }
            catch (Exception e)
            {
                log.Error(e);
            }

            return (response?.IsSuccessful == true && response?.StatusCode == System.Net.HttpStatusCode.OK, res);
        }

        private static void LogRestResponse(IRestResponse response, string method)
        {
            log.DebugFormat("Response Method: {0}", method);
            log.DebugFormat("Response Successful: {0}", response?.IsSuccessful == true);
            log.DebugFormat("Response Status Code: {0}", response?.StatusCode);
            log.DebugFormat("Response Status Description: {0}", response?.StatusDescription);
            log.DebugFormat("Response Content: {0}", response?.Content);
            if (response?.IsSuccessful == false)
            {
                log.Debug("Response error message: " + response?.ErrorMessage);
                if (response?.ErrorException != null)
                {
                    log.Error("An Exception was returned on the API response", response?.ErrorException);
                }
            }

        }

        public static void LoadTicketData(string pFile)
        {
            try
            {
                if (!File.Exists(pFile))
                {
                    return;
                }

                XmlDocument xmldoc = new XmlDocument();
                xmldoc.Load(pFile);
                XmlNodeList nodeList = xmldoc.SelectNodes("Language/Localized");

                if (nodeList.Count > 0)
                {
                    foreach (XmlNode node in nodeList)
                    {
                        if (!TicketInfo.ContainsKey(node.Attributes["name"].Value))
                            TicketInfo.Add(node.Attributes["name"].Value, node.Attributes["value"].Value);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Warn(ex.Message);
            }
        }

        private static string[] GetTicketHeader()
        {
            return TicketInfo.ContainsKey("Header") ? TicketInfo["Header"].Split('\n') : new string[0];
        }

        private static string[] GetTicketFooter()
        {
            return TicketInfo.ContainsKey("Footer") ? TicketInfo["Footer"].Split('\n') : new string[0];
        }

        public static void PrintTicketQR(string installationId, string terminalId, string transientId, string ticketQR, bool updateTransient, bool openBarrier)
        {
            if (string.IsNullOrEmpty(ticketQR))
            {
                log.Warn("No ticket can be printed, argument is null");
                return;
            }

            try
            {
                if (ConfigurationManager.AppSettings["PostPrintTicket"] == "true")
                {
                    var url = ConfigurationManager.AppSettings["RestApiUrl"] + "PrintTicket?terminalId=" + terminalId + "&transientId=" + transientId;
                    var client = new RestClient(url);
                    client.Timeout = 30000;
                    var request = new RestRequest(Method.POST);
                    IRestResponse response = client.Execute(request);
                    LogRestResponse(response, "PrintTicket");
                }
                else
                {
                    log.Debug("Printing ticket...");

                    Bitmap bitmap = null;
                    string[] header = GetTicketHeader();
                    string[] footer = GetTicketFooter();

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
                    var qrMargin = int.Parse(ConfigurationManager.AppSettings["TicketQRMargin"] ?? "0");
                    var qrPureBarcode = bool.Parse(ConfigurationManager.AppSettings["TicketQRPureBarcode"] ?? "false");
                    var qrMarginLeft = int.Parse(ConfigurationManager.AppSettings["TicketQRMarginLeft"] ?? "5");

                    BarcodeWriter barcodeWriter = new BarcodeWriter();
                    EncodingOptions encodingOptions = new EncodingOptions()
                    {
                        Width = qrCodeWidth,
                        Height = qrCodeHeight,
                        Margin = qrMargin,
                        PureBarcode = qrPureBarcode
                    };

                    var correction = ErrorCorrectionLevel.H;
                    if (ConfigurationManager.AppSettings["TicketQRCorrection"] == "H")
                        correction = ErrorCorrectionLevel.H;
                    else if (ConfigurationManager.AppSettings["TicketQRCorrection"] == "Q")
                        correction = ErrorCorrectionLevel.Q;
                    else if (ConfigurationManager.AppSettings["TicketQRCorrection"] == "M")
                        correction = ErrorCorrectionLevel.M;
                    else if (ConfigurationManager.AppSettings["TicketQRCorrection"] == "L")
                        correction = ErrorCorrectionLevel.L;

                    encodingOptions.Hints.Add(EncodeHintType.ERROR_CORRECTION, correction);
                    barcodeWriter.Renderer = new BitmapRenderer();
                    barcodeWriter.Options = encodingOptions;
                    barcodeWriter.Format = BarcodeFormat.QR_CODE;
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
                            e.Graphics.DrawImage(bitmap, new Point(qrMarginLeft, pointYQR));
                            e.Graphics.DrawString(sbTextoFooter.ToString(), new Font("Arial", 7), Brushes.Black, 3f, pointYFooter, new StringFormat());
                        });

                    try
                    {
                        printDocument.Print();
                    }
                    catch (InvalidPrinterException ipe)
                    {
                        log.Error("PRINTER ERROR", ipe);
                    }
                    catch (Exception e)
                    {
                        log.Error(e);
                    }

                    sbTextoHeader.Clear();
                    sbTextoFooter.Clear();

                    log.DebugFormat("Ticket printed successfully");
                }

                if (updateTransient)
                {
                    TapForTicket(new TapForTicketRequest
                    {
                        installationID = installationId,
                        terminalId = terminalId,
                        transientId = int.Parse(transientId),
                        ticketQR = ticketQR
                    });
                }

                if (openBarrier)
                {
                    new Thread(new ThreadStart(() =>
                    {
                        Thread.Sleep(int.Parse(ConfigurationManager.AppSettings["PrintTicketOpenBarrierTimeout"] ?? "3000"));

                        var terminal = TerminalService.GetTerminal(terminalId);
                        if (terminal != null)
                        {
                            terminal.ForceOpenBarrier = true;
                            TerminalService.UpdateTerminal(terminal);
                        }
                    })).Start();
                }
            }
            catch (Exception ex)
            {
                log.Error("Cannot print ticket", ex);
            }
        }
    }
}