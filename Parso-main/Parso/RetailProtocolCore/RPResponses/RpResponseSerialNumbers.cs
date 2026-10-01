using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCI.Globalcom.GlobalcomRetailProtocol
{
    /// <summary>
    /// Response class for Serial Number requests
    /// </summary>
    public class RpResponseSerialNumbers : RpResponseBase
    {
        public List<string> SerialNumbers = new List<string>();

        /// <summary> Constructor </summary>
        /// <param name="response"></param>
        public RpResponseSerialNumbers(byte[] response)
            : base(response)
        {
            if (VerifyOutcome() == EOutcome.OK)
            {
                for (int i = 0; i < (Info.Length - 1); i += 13)
                {
                    byte[] val = new byte[13];
                    Buffer.BlockCopy(Info, i, val, 0, 13);
                    SerialNumbers.Add(Encoding.ASCII.GetString(val));
                }
            }
        }

        /// <summary>
        /// Returns a well formatted string of the content of the object
        /// </summary>
        public string PrintDetails()
        {
            StringBuilder sb = new StringBuilder(1000);
            sb.AppendLine("Serial Number Configuration:");

            if (VerifyOutcome() != EOutcome.OK)
            {
                sb.AppendLine("--> Request Failed" + Environment.NewLine);
                return sb.ToString();
            }

            sb.AppendLine("-->" + SerialNumbers[0] ?? "");

            if (SerialNumbers.Count > 1)
                sb.AppendLine("--> Slave 1:" + SerialNumbers[1] ?? "");
            if (SerialNumbers.Count > 2)
                sb.AppendLine("--> Slave 2:" + SerialNumbers[2] ?? "");

            sb.AppendLine();
            return sb.ToString();
        }

        /// <summary>
        /// Returns a well formatted string of the content of the object
        /// </summary>
        public string PrintDetailsAsXml()
        {
            StringBuilder sb = new StringBuilder(1000);
            sb.AppendLine("<SerialNumbers>");

            if (VerifyOutcome() != EOutcome.OK)
                return sb.ToString();

            sb.AppendLine("  <Master>" + (SerialNumbers[0] ?? "") + "</Master>");

            if (SerialNumbers.Count > 1)
                sb.AppendLine("  <Slave1>" + (SerialNumbers[1] ?? "") + "</Slave1>");
            if (SerialNumbers.Count > 2)
                sb.AppendLine("  <Slave2>" + (SerialNumbers[2] ?? "") + "</Slave2>");

            sb.AppendLine("</SerialNumbers>");
            return sb.ToString();
        }
    }

}
