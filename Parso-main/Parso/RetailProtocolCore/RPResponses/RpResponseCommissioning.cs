using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCI.Globalcom.GlobalcomRetailProtocol
{
    /// <summary>
    /// Response Class for commissioning requests
    /// </summary>
    public class RpResponseCommissioning : RpResponseBase
    {
        public byte[] EventCode;
        public byte[] CompanyCode;
        public byte[] SerialNumber;
        public byte[] Random;
        public byte[] FutureUse;
        public byte[] KeyBytes;

        public RpResponseCommissioning(byte[] response)
            : base(response)
        {
            // no response byte.  make sure at least the expected data is there
            if (response.Length < 20 + 5)
            {
                Outcome = (Byte)EOutcome.ErrorOutcome;
                return;
            }

            Outcome = (Byte)EOutcome.OK;

            EventCode = new byte[3];
            CompanyCode = new byte[3];
            SerialNumber = new byte[5];
            Random = new byte[8];
            FutureUse = new byte[1];
            KeyBytes = new byte[19];

            Buffer.BlockCopy(response, 5, EventCode, 0, 3);
            Buffer.BlockCopy(response, 8, CompanyCode, 0, 3);
            Buffer.BlockCopy(response, 11, SerialNumber, 0, 5);
            Buffer.BlockCopy(response, 16, Random, 0, 8);
            FutureUse[0] = response[24];

            Buffer.BlockCopy(response, 5, KeyBytes, 0, 3);
            Buffer.BlockCopy(response, 8, KeyBytes, 3, 3);
            Buffer.BlockCopy(response, 11, KeyBytes, 6, 5);
            Buffer.BlockCopy(response, 16, KeyBytes, 11, 8);

        }
    }
}
