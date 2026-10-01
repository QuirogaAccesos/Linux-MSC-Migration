using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCI.Globalcom.GlobalcomRetailProtocol
{
    /// <summary>
    /// Response Class for test ethernet (Ping)
    /// </summary>
    public class RpResponseTestEth : RpResponseBase
    {
        /// <summary> Result of the Ping Test </summary>
        /// <param name="response"></param>
        public RpResponseTestEth(byte[] response) : base(response) { }


        public string GetUPTIP()
        {
            string sIP = "";

            string[] pingLines = Encoding.ASCII.GetString(Info).Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
            foreach (string line in pingLines)
            {
                if (line.Contains("IP ADDRESS"))
                {
                    sIP = line.Substring(12);
                    break;
                }
            }

            return sIP;
        }

        /// <summary>
        /// Returns a well formatted string of the content of the object
        /// </summary>
        public string PrintDetails(string ipaddress)
        {
            StringBuilder sb = new StringBuilder(100);
            sb.AppendLine("Network Ping Test");
            sb.AppendLine("Pinging " + ipaddress + " ...");

            sb.AppendLine(Encoding.ASCII.GetString(Info));
            sb.AppendLine();
            return sb.ToString();
        }

        /// <summary>
        /// Returns an xml formatted string of the content of the object
        /// </summary>
        public string PrintDetailsAsXml(string ipaddress)
        {
            StringBuilder sb = new StringBuilder(100);
            sb.AppendLine("<Ping IPAddress='" + ipaddress + "'>");
            sb.AppendLine(Encoding.ASCII.GetString(Info));
            sb.AppendLine("</Ping>");
            return sb.ToString();
        }

    }
}
