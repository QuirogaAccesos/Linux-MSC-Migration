using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
//using System.Windows.Forms;
using System.Xml;

namespace CCI.Globalcom.GlobalcomRetailProtocol
{
    /// <summary>
    /// Response class for retrieve the Notify/Post auth data
    /// </summary>
    public class RpResponseGetNotify: RpResponseBase
    {
        public string xmlData;
        private XmlDocument _domReq;
        private string _hostResp;

        public RpResponseGetNotify(byte[] response) : base(response)
        {
            xmlData = Encoding.ASCII.GetString(Info, 0, (InfoLength - 1));
            if (xmlData == null || xmlData.Length == 0)
                throw new Exception("No Post Data Returned!");

            try
            {
                // Load the xml
                _domReq = new XmlDocument();
                _domReq.LoadXml(Regex.Unescape(xmlData).Normalize());
            }
            catch { }
            {
            }
        }

        /// <summary>
        ///   Post the received content to the the URL in the request.
        /// </summary>
        /// <returns>if the data was sent successfully</returns>
        /// <exception cref="Exception"></exception>
        public bool postRequest()
        {
            // Parse the Data
            string sIp = "";
            int nPort = 0;
            if (!getConnectionInfo(ref sIp, ref nPort))
                throw new Exception("Unable to parse the destination!");

            string sXml = getNotifyXml();
            if (sXml == null)
                throw new Exception("Unable to parse the content!");

            // Post content to URL
            string sResp;
            TcpClient client = new TcpClient(sIp, nPort);

            using (SslStream sslStream = new SslStream(client.GetStream(), false,
                       new RemoteCertificateValidationCallback(ValidateServerCertificate), null))
            {
                sslStream.AuthenticateAsClient(sIp);
                // This is where you read and send data
                byte[] byte_array = Encoding.ASCII.GetBytes(sXml);
                sslStream.Write(byte_array);

                byte[] recv_array = new byte[1000];
                int size = sslStream.Read(recv_array, 0, 1000);

                _hostResp = Encoding.UTF8.GetString(recv_array, 0, size);
            }

            client.Close();

            return true;
        }

        public string getReceiptXml()
        {
            if (_hostResp == null)
                return "No Receipt Available";

            // Grab the necessary data out of response
            XmlDocument hostRespDom = new XmlDocument();
            hostRespDom.LoadXml(Regex.Unescape(_hostResp).Normalize());
            XmlNode node = hostRespDom.DocumentElement.SelectSingleNode("Credit");
            if (node == null) throw new Exception("Failed to parse response!");

            node = node.SelectSingleNode("ResponseCode");
            if (node == null) throw new Exception("Failed to parse response code!");

            string sResult = node.InnerText;

            // Now build result
            XmlDocument respDom = new XmlDocument();
            respDom.LoadXml(Regex.Unescape(xmlData).Normalize());

            // First Load in the Response from the host
            node = respDom.DocumentElement.SelectSingleNode("TransactionData");
            if (node == null) throw new Exception("Failed to find Transaction Data!");

            node = node.SelectSingleNode("Error");
            if (node == null) throw new Exception("Failed to find Transaction Data!");
            node.InnerText = sResult;

            // Now remove the Original Data
            XmlNode gw = respDom.DocumentElement.SelectSingleNode("Gateway");
            if (gw == null) return null;

            XmlNode postAuthReq = gw.SelectSingleNode("PostAuthRequest");
            if (postAuthReq == null) return null;

            gw.RemoveChild(postAuthReq);

            // Populate the response
            XmlNode newBook = gw.OwnerDocument.ImportNode(hostRespDom.FirstChild, true);
            gw.AppendChild(newBook);

            return respDom.InnerXml;
        }

        public static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        private bool getConnectionInfo(ref string ip, ref Int32 port)
        {
            if (_domReq == null)
                return false;

            // Grab the Post Auth Block
            XmlNode gw = _domReq.DocumentElement.SelectSingleNode("Gateway");
            if (gw == null) return false;

            // Is the Destination Apriva
            XmlNode postAuthReq = gw.SelectSingleNode("PostAuthRequest");
            if (postAuthReq == null)
                return false;

            // Get the URL
            if (postAuthReq.Attributes["URL"] == null)
                return false;

            ip = postAuthReq.Attributes["URL"].Value;

            // Get the port
            if (postAuthReq.Attributes["port"] == null)
                return false;

            port = Convert.ToInt32(postAuthReq.Attributes["port"].Value);

            return true;
        }

        private string getNotifyXml()
        {
            if (_domReq == null)
                return null;

            // Grab the Post Auth Block
            XmlNode gw = _domReq.DocumentElement.SelectSingleNode("Gateway");
            if (gw == null) return null;

            // Is the Destination Apriva
            XmlNode postAuthReq = gw.SelectSingleNode("PostAuthRequest");
            if (postAuthReq == null)
                return null;

            return postAuthReq.InnerXml;
        }


        /// <summary>
        /// Returns a well formatted string of the content of the object
        /// </summary>
        public string PrintDetails()
        {
            StringBuilder sb = new StringBuilder(1000);
            sb.AppendLine("Notify Content -->" + xmlData.Replace('\n', ' '));

            return sb.ToString();
        }
    }
}
