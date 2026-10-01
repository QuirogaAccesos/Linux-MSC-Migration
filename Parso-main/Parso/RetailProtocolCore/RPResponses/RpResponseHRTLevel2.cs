using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CCI.Globalcom.GlobalcomRetailProtocol
{
    /// <summary> Resposne class to handle Level 2 Parameter file </summary>
    public class RpResponseHRTLevel2 : RpResponseBase
    {
        public Level2PrmFile prmFile;
        public int fullPrmFileLen;

        public RpResponseHRTLevel2(byte[] response, bool buffered = false) : base(response)
        {
            if (VerifyOutcome() != EOutcome.OK)
                return;

            prmFile = new Level2PrmFile();
            if (!buffered)
            {
                prmFile.Decode(Info, InfoLength);
                fullPrmFileLen = InfoLength;
            }
            else
            {
                fullPrmFileLen = EndianBitConverter.Big.ToInt16(Info, 0);
            }
        }

        public string GetPartialFile()
        {
            return Encoding.UTF8.GetString(Info, 2, InfoLength - 3);
        }

        public int GetPartialFile(ref byte[] bytesOut, int nPos)
        {
            // InfoLength includes Result and TotalFileLength
            // But Info only has the TotalFile Length we need to skip
            Buffer.BlockCopy(Info, 2, bytesOut, nPos, InfoLength - 3);
            return (InfoLength - 3);
        }

        public void LoadPrm(byte[] prmFileInfo, int prmFileLen)
        {
            prmFile = new Level2PrmFile();
            prmFile.Decode(prmFileInfo, prmFileLen);
        }

        /// <summary>
        /// Returns a well formatted string of the content of the object
        /// </summary>
        public string PrintDetails(string sconfig = null)
        {
            if (VerifyOutcome() != EOutcome.OK || InfoLength < 2)
                return "No Level 2 data!";

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Level2.prm:");

            sb.AppendLine(Encoding.ASCII.GetString(Info));
            sb.AppendLine(Environment.NewLine);
            return sb.ToString();
        }

        /// <summary>
        /// Returns a well formatted string of the content of the object
        /// </summary>
        public string PrintDetailsAsXml(string sconfig = null)
        {
            if (VerifyOutcome() != EOutcome.OK || InfoLength < 2)
                return "<HRTLevel2Parameters/>";

            return prmFile.GetXml();
        }
    }

}
