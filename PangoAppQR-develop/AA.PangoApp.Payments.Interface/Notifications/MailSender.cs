using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace AA.PangoApp.Payments.Interface.Notifications
{
    /// <summary>
    /// Punto ÚNICO de envío de correos de alerta del sistema.
    ///
    ///  - Enriquece asunto y cuerpo con el contexto del terminal (parking, installation,
    ///    terminal, máquina, versión de app y fecha) de forma central.
    ///  - Envía por Microsoft Graph (HttpClient + client credentials, SIN SDK).
    ///  - Nunca lanza excepción: un fallo notificando no debe tumbar el terminal (se loguea).
    ///
    /// Claves de configuración (AppSettings) que usa:
    ///   Envío destino/remite:   EmailRecipients, EmailUser
    ///   Graph:                  Graph-TenantId, Graph-ClientId, Graph-ClientSecret, Graph-Sender
    ///   Contexto (enriquecido): ParkingName, InstallationId, TerminalId
    /// </summary>
    public static class MailSender
    {
        private static readonly ILog log = LogManager.GetLogger("MailSender");

        // Un único HttpClient reutilizable (buena práctica) con timeout de 30 s.
        private static readonly HttpClient httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        /// <summary>Envía un correo de alerta por Microsoft Graph. Enriquece asunto y cuerpo.</summary>
        public static void Send(string subject, string body)
        {
            try
            {
                string recipients = Setting("EmailRecipients");
                if (string.IsNullOrWhiteSpace(recipients))
                {
                    log.Warn("MailSender: no hay 'EmailRecipients' configurado; no se envía correo. Subject=" + subject);
                    return;
                }

                string enrichedSubject = BuildSubject(subject);
                string enrichedBody = BuildBody(body);

                SendViaGraph(enrichedSubject, enrichedBody, recipients);
            }
            catch (Exception ex)
            {
                // Un fallo enviando el correo NO debe propagarse al flujo del terminal.
                log.Error("MailSender.Send failed. Subject=" + subject, ex);
            }
        }

        // ---------------------------------------------------------------------
        //  Enriquecimiento central
        // ---------------------------------------------------------------------

        private static string BuildSubject(string subject)
        {
            var parts = new List<string>();
            string parking = Setting("ParkingName");
            string inst = Setting("InstallationId");
            string term = Setting("TerminalId");

            if (!string.IsNullOrEmpty(parking)) parts.Add(parking);
            if (!string.IsNullOrEmpty(inst)) parts.Add("Inst " + inst);
            if (!string.IsNullOrEmpty(term)) parts.Add("Term " + term);

            string prefix = parts.Count > 0 ? "[" + string.Join(" · ", parts) + "] " : "";
            return prefix + subject;
        }

        private static string BuildBody(string body)
        {
            string nowUtc = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " UTC";
            string nowLocal = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            var sb = new StringBuilder();
            sb.Append(body ?? string.Empty);
            sb.Append("<hr style=\"margin-top:16px\" />");
            sb.Append("<div style=\"font-family:Segoe UI,Arial,sans-serif;font-size:11px;color:#888\">");
            sb.Append("Parking: ").Append(Html(Setting("ParkingName")))
              .Append(" &nbsp;·&nbsp; InstallationId: ").Append(Html(Setting("InstallationId")))
              .Append(" &nbsp;·&nbsp; TerminalId: ").Append(Html(Setting("TerminalId"))).Append("<br />");
            sb.Append("Máquina: ").Append(Html(SafeMachineName()))
              .Append(" &nbsp;·&nbsp; App: ").Append(Html(AppVersion())).Append("<br />");
            sb.Append(Html(nowLocal)).Append(" (local) &nbsp;·&nbsp; ").Append(Html(nowUtc));
            sb.Append("</div>");
            return sb.ToString();
        }

        // ---------------------------------------------------------------------
        //  Microsoft Graph (HttpClient, client credentials, sin SDK)
        // ---------------------------------------------------------------------

        private static void SendViaGraph(string subject, string body, string recipients)
        {
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
            // Se ejecuta en el threadpool para evitar deadlocks si el llamante está en un hilo
            // con SynchronizationContext (p.ej. el hilo de UI de WinForms).
            Task.Run(() => SendViaGraphAsync(subject, body, recipients)).GetAwaiter().GetResult();
        }

        private static async Task SendViaGraphAsync(string subject, string body, string recipients)
        {
            string token = await GetGraphTokenAsync().ConfigureAwait(false);

            string sender = Setting("Graph-Sender");
            if (string.IsNullOrEmpty(sender)) sender = Setting("EmailUser");

            var request = new GraphSendMailRequest
            {
                message = new GraphMessage
                {
                    subject = subject,
                    importance = "high",
                    body = new GraphBody { contentType = "HTML", content = body },
                    toRecipients = ToRecipients(recipients),
                    ccRecipients = ToRecipients(Setting("EmailUser")) // CC a sí mismo, como el envío anterior
                },
                saveToSentItems = false
            };

            string json = SerializeJson(request);
            string url = "https://graph.microsoft.com/v1.0/users/" + sender + "/sendMail";

            using (var httpRequest = new HttpRequestMessage(HttpMethod.Post, url))
            {
                httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");

                using (var response = await httpClient.SendAsync(httpRequest).ConfigureAwait(false))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        string respBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        throw new Exception("Graph sendMail HTTP " + (int)response.StatusCode + ": " + respBody);
                    }
                }
            }
            log.Info("MailSender: correo enviado por Graph. Subject=" + subject);
        }

        private static async Task<string> GetGraphTokenAsync()
        {
            string tenant = Setting("Graph-TenantId");
            string url = "https://login.microsoftonline.com/" + tenant + "/oauth2/v2.0/token";

            var form = new Dictionary<string, string>
            {
                { "client_id", Setting("Graph-ClientId") },
                { "client_secret", Setting("Graph-ClientSecret") },
                { "scope", "https://graph.microsoft.com/.default" },
                { "grant_type", "client_credentials" }
            };

            using (var content = new FormUrlEncodedContent(form))
            using (var response = await httpClient.PostAsync(url, content).ConfigureAwait(false))
            {
                string respBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                    throw new Exception("Graph token HTTP " + (int)response.StatusCode + ": " + respBody);

                var token = DeserializeJson<GraphTokenResponse>(respBody);
                if (token == null || string.IsNullOrEmpty(token.access_token))
                    throw new Exception("Graph token vacío o no parseable.");
                return token.access_token;
            }
        }

        private static GraphRecipient[] ToRecipients(string list)
        {
            var result = new List<GraphRecipient>();
            if (!string.IsNullOrEmpty(list))
            {
                foreach (var addr in list.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    string trimmed = addr.Trim();
                    if (trimmed.Length > 0)
                        result.Add(new GraphRecipient { emailAddress = new GraphEmailAddress { address = trimmed } });
                }
            }
            return result.ToArray();
        }

        // ---------------------------------------------------------------------
        //  Helpers
        // ---------------------------------------------------------------------

        private static string Setting(string key)
        {
            return ConfigurationManager.AppSettings[key] ?? string.Empty;
        }

        private static string Html(string value)
        {
            return System.Web.HttpUtility.HtmlEncode(value ?? string.Empty);
        }

        private static string SafeMachineName()
        {
            try { return Environment.MachineName; } catch { return string.Empty; }
        }

        private static string AppVersion()
        {
            try
            {
                var asm = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
                return asm != null && asm.GetName() != null && asm.GetName().Version != null
                    ? asm.GetName().Version.ToString()
                    : string.Empty;
            }
            catch { return string.Empty; }
        }

        private static string SerializeJson<T>(T obj)
        {
            var serializer = new DataContractJsonSerializer(typeof(T));
            using (var ms = new MemoryStream())
            {
                serializer.WriteObject(ms, obj);
                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }

        private static T DeserializeJson<T>(string json)
        {
            var serializer = new DataContractJsonSerializer(typeof(T));
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                return (T)serializer.ReadObject(ms);
            }
        }
    }

    // ------------------- DTOs para (de)serializar JSON de Graph -------------------

    [DataContract]
    internal class GraphSendMailRequest
    {
        [DataMember(Name = "message", Order = 0)] public GraphMessage message { get; set; }
        [DataMember(Name = "saveToSentItems", Order = 1)] public bool saveToSentItems { get; set; }
    }

    [DataContract]
    internal class GraphMessage
    {
        [DataMember(Name = "subject", Order = 0)] public string subject { get; set; }
        [DataMember(Name = "importance", Order = 1)] public string importance { get; set; }
        [DataMember(Name = "body", Order = 2)] public GraphBody body { get; set; }
        [DataMember(Name = "toRecipients", Order = 3)] public GraphRecipient[] toRecipients { get; set; }
        [DataMember(Name = "ccRecipients", Order = 4)] public GraphRecipient[] ccRecipients { get; set; }
    }

    [DataContract]
    internal class GraphBody
    {
        [DataMember(Name = "contentType", Order = 0)] public string contentType { get; set; }
        [DataMember(Name = "content", Order = 1)] public string content { get; set; }
    }

    [DataContract]
    internal class GraphRecipient
    {
        [DataMember(Name = "emailAddress")] public GraphEmailAddress emailAddress { get; set; }
    }

    [DataContract]
    internal class GraphEmailAddress
    {
        [DataMember(Name = "address")] public string address { get; set; }
    }

    [DataContract]
    internal class GraphTokenResponse
    {
        [DataMember(Name = "access_token")] public string access_token { get; set; }
    }
}
