using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCI.Globalcom.GlobalcomRetailProtocol
{
    /// <summary>
    /// Response Class for get CA Cert
    /// </summary>
    public class RpResponseGetCACert : RpResponseBase
    {
        public RpResponseGetCACert(byte[] response) : base(response) { }

        /// <summary>
        /// Returns a well formatted string of the content of the object
        /// </summary>
        public string PrintDetails()
        {
            StringBuilder sb = new StringBuilder(100);
            sb.AppendLine("TLS Server Certificate:");

            sb.AppendLine(Encoding.ASCII.GetString(Info));
            sb.AppendLine(Environment.NewLine);
            return sb.ToString();
        }

        public string PrintDetailsV2()
        {
            StringBuilder sb = new StringBuilder(100);
            sb.AppendLine("TLS Server Certificate:");

            string sInfo = Encoding.ASCII.GetString(Info);
            sInfo = sInfo.Replace('\r', '|');
            sInfo = sInfo.Replace('\n', ' ');

            string[] theList = sInfo.Split('|');
            foreach (string line in theList)
            {
                if (line.Contains("issuer name") || line.Contains("subject name") || line.Contains("expires on"))
                    sb.AppendLine(line);
            }

            sb.AppendLine();
            return sb.ToString();
        }

        /// <summary>
        /// Return the name of the cert
        /// </summary>
        public string GetName()
        {
            StringBuilder sb = new StringBuilder(100);

            string sInfo = Encoding.ASCII.GetString(Info);
            if (String.IsNullOrEmpty(sInfo))
                return "";

            int nstart = sInfo.IndexOf("subject name");
            nstart = sInfo.IndexOf(':', nstart) + 1;
            int nend = sInfo.IndexOf('\r', nstart) - 1;
            sb.Append(sInfo.Substring(nstart, nend-nstart));

            return sb.ToString();
        }

        /// <summary>
        /// Returns a well formatted string of the content of the object
        /// </summary>
        public string PrintDetailsAsXml(string sconfig = null)
        {
            StringBuilder sb = new StringBuilder(100);
            sb.AppendLine("<TLSServerCert>");
            string certInfo = Encoding.ASCII.GetString(Info);

            int start = certInfo.IndexOf("\r\n", 0) + 2; 
            int pos = certInfo.IndexOf("\n\r\n", start);
            while (pos > 0)
            {
                string line = certInfo.Substring(start, pos-start);
                sb.AppendLine(line);

                start = pos+3;
                pos = certInfo.IndexOf("\n\r", start);
            }

            if (start < certInfo.Length - 1)
            {
                string line = certInfo.Substring(start, certInfo.Length - start - 1);
                sb.AppendLine(line);
            }

            sb.AppendLine("</TLSServerCert>");
            return sb.ToString();
        }
    }
}
