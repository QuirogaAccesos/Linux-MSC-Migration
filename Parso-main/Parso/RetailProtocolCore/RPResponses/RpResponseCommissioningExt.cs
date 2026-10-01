using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCI.Globalcom.GlobalcomRetailProtocol
{
    /// <summary>
    /// Response Class for Extended commissioning requests
    /// </summary>
    public class RpResponseCommissioningExt : RpResponseBase
    {
        public byte[] TypeOfLockCode;
        public byte[] SerialNumber;
        public byte[] Random;
        public byte[] KeyBytes;

        public RpResponseCommissioningExt(byte[] response)
            : base(response)
        {
            TypeOfLockCode = new byte[3];
            SerialNumber = new byte[10];
            Random = new byte[8];
            KeyBytes = new byte[21];

            Buffer.BlockCopy(response, 7, TypeOfLockCode, 0, 3);
            Buffer.BlockCopy(response, 10, SerialNumber, 0, 10);
            Buffer.BlockCopy(response, 20, Random, 0, 8);
            
            Buffer.BlockCopy(response, 7, KeyBytes, 0, 21);
        }
    }
}
