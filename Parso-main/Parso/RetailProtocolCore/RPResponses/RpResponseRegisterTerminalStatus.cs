using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCI.Globalcom.GlobalcomRetailProtocol
{
    /// <summary>
    /// Response Class for Terminal Register requests
    /// </summary>
    public class RpResponseRegisterTerminalStatus : RpResponseBase
    {
        public bool Registered;
        public string HostResponseCode;
        public string HostResponseText;

        public RpResponseRegisterTerminalStatus(byte[] response)
            : base(response)
        {
            if (VerifyOutcome() != EOutcome.OK)
                return;

            Registered = ((response[6] == 0x31)) ? true : false;

            string HostResp = Encoding.UTF8.GetString(response, 7, response.Length - 7 - 2);

            string[] lines = HostResp.Split(new string[] { "\r\n" }, StringSplitOptions.None);

            HostResponseCode = lines[0];

            if (lines.Count() > 1)
                HostResponseText = lines[1];
        }
    }
}
