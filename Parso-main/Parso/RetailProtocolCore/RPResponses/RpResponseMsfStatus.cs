using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCI.Globalcom.GlobalcomRetailProtocol
{
    /// <summary>
    /// Response Class for Mag Store and Forward Status Response
    /// </summary>
    public class RpResponseMsfStatus : RpResponseBase
    {
        public int CurrentOfflineCount;
        public int Maximum;

        /// <summary> Constructor </summary>
        /// <param name="response"></param>
        public RpResponseMsfStatus(byte[] response) : base(response)
        {
            if (response.Length > 10)
            {
                CurrentOfflineCount = EndianBitConverter.Big.ToInt32(response, 6);
            }
            if (response.Length > 13)
            {
                Maximum = EndianBitConverter.Big.ToInt32(response, 10);
            }
        }

        /// <summary>
        /// Returns a well formatted string of the content of the object
        /// </summary>
        public string PrintDetails()
        {
            StringBuilder sb = new StringBuilder(100);
            if (VerifyOutcome() != EOutcome.OK)
                return ("Store and Forward Command Failed");

            sb.AppendLine("Store and Forward Transaction Count: " + CurrentOfflineCount.ToString() +
                          " of " + Maximum.ToString() + " Maximum" + Environment.NewLine);

            return sb.ToString();
        }

        /// <summary>
        /// Returns a well formatted string of xml content of the object
        /// </summary>
        public string PrintDetailsAsXml()
        {
            StringBuilder sb = new StringBuilder(100);
            if (VerifyOutcome() != EOutcome.OK)
                return ("<StoreAndFwd/>");

            sb.AppendLine("<StoreAndFwd>");
            sb.AppendLine("  <Pending>" + CurrentOfflineCount.ToString() + "</Pending>");
            sb.AppendLine("  <Maximum>" + Maximum.ToString() + "</Maximum>");
            sb.AppendLine("</StoreAndFwd>");

            return sb.ToString();
        }
    }
}
