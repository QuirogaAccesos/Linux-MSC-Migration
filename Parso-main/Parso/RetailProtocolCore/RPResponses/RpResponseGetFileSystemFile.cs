using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCI.Globalcom.GlobalcomRetailProtocol
{
    /// <summary>
    /// Response Class for commissioning requests
    /// </summary>
    public class RpResponseGetFileSystemFile : RpResponseBase
    {
        public string FullFile;
        public int FileLength;

        public RpResponseGetFileSystemFile(byte[] response) : base(response)
        {
            if (InfoLength > 1)
            {
                FileLength = EndianBitConverter.Big.ToInt16(Info, 0);
                FullFile = Encoding.UTF8.GetString(Info, 2, InfoLength - 3);
            }
            else
            {
                FileLength = 0;
                FullFile = "";
            }
        }

        public int append(string data)
        {
            FullFile += data;
            return FullFile.Length;
        }

    }
}
