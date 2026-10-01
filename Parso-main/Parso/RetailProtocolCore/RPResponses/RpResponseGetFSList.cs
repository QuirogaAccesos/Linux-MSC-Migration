using CCI.Globalcom.GlobalcomRetailProtocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCI.Globalcom.GlobalcomRetailProtocol
{
    /// <summary>
    /// Response class for parsing the Get FS List and MD5 List
    /// </summary>
    public class RpResponseGetFSList : RpResponseBase
    {
        public List<string> FSList;
        public List<Tuple<string, string, string>> FSMD5List;

        public int fullFileLen;

        public RpResponseGetFSList(byte[] response) : base(response)
        {
            fullFileLen = 0;
            if (VerifyOutcome() == EOutcome.OK)
                fullFileLen = EndianBitConverter.Big.ToInt16(Info, 0);
        }

        public int GetPartialFile(ref byte[] bytesOut, int nPos)
        {
            // InfoLength includes Result and TotalFileLength
            // But Info only has the TotalFile Length we need to skip
            Buffer.BlockCopy(Info, 2, bytesOut, nPos, InfoLength - 3);
            return (InfoLength - 3);
        }

        public void LoadFile(byte[] prmFileInfo, int prmFileLen)
        {
            string FullList = Encoding.ASCII.GetString(prmFileInfo, 0, (prmFileLen - 1));
            FSList = FullList.Split(new string[] { "\r\n" }, StringSplitOptions.None).ToList();
        }

        public void LoadCLMD5File(byte[] prmFileInfo, int prmFileLen)
        {
            FSMD5List = new List<Tuple<string, string, string>>();

            string FullList = Encoding.ASCII.GetString(prmFileInfo, 0, (prmFileLen - 1));
            string[] lines = FullList.Split(new string[] { "\r\n" }, StringSplitOptions.None);

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Length < 1) break;

                List<string> columns = lines[i].Split(new string[] { "  " }, StringSplitOptions.None).ToList();

                if (columns.Count() == 1)
                    FSMD5List.Add(new Tuple<string, string, string>(columns[0], "", ""));
                else if (columns.Count() == 2)
                    FSMD5List.Add(new Tuple<string, string, string>(columns[0], columns[1], ""));
                else
                    FSMD5List.Add(new Tuple<string, string, string>(columns[0], columns[1], columns[2]));
            }
        }
    }
}
