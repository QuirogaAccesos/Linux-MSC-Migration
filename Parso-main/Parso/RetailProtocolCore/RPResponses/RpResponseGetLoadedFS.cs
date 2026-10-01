using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCI.Globalcom.GlobalcomRetailProtocol
{
    /// <summary>
    /// Response class for parsing the Last Loaded FS
    /// </summary>
    public class RpResponseGetLoadedFS : RpResponseBase
    {
        public string LoadedFS;

        public RpResponseGetLoadedFS(byte[] response) : base(response)
        {
            LoadedFS = Encoding.ASCII.GetString(Info, 0, (InfoLength - 1));
        }

        /// <summary>
        /// Returns a well formatted string of the content of the object
        /// </summary>
        public string PrintDetails()
        {
            StringBuilder sb = new StringBuilder(1000);

            if (VerifyOutcome() == EOutcome.OK)
                sb.AppendLine("File System:" + LoadedFS.Replace('\n', ' '));
            else if (VerifyOutcome() == EOutcome.ErrorOutcome)
                sb.AppendLine("--> FileSystem: {The application may be too old to check!}");
            else
                sb.AppendLine("--> File System: " + VerifyOutcome().ToString());

            return sb.ToString();
        }

        /// <summary>
        /// Returns a well formatted string of the content of the object
        /// </summary>
        public string PrintDetailsAsXml()
        {
            StringBuilder sb = new StringBuilder(1000);
            if (VerifyOutcome() == EOutcome.OK)
                sb.AppendLine("<FileSystem>" + LoadedFS.Replace('\n', ' ') + "</FileSystem>");
            else if (VerifyOutcome() == EOutcome.ErrorOutcome)
                sb.AppendLine("<FileSystem>The application may be too old to check!</FileSystem>");
            else
                sb.AppendLine("<FileSystem>" + VerifyOutcome().ToString() + "</FileSystem>");

            return sb.ToString();
        }
    }
}
