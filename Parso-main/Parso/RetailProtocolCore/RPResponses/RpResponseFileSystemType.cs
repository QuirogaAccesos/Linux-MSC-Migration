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
    public class RpResponseFileSystemType : RpResponseBase
    {
        public EFileSystemType FileSystemType;

        public RpResponseFileSystemType(byte[] response)
            : base(response)
        {
            FileSystemType = ((response[6] == 0x01)) ? EFileSystemType.HCC : EFileSystemType.Chan;
        }
    }
}
