using System;
using System.Collections.Generic;
//using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace CCI.Globalcom.GlobalcomRetailProtocol
{
    /// <summary>
    /// Response Class for Firmware Version Requests
    /// </summary>
    public class RpResponseFirmwareVersion : RpResponseBase
    {
        public string MasterFirmwareVersion;
        public List<string> SlaveFirmwareVersions;

        public RpResponseFirmwareVersion(byte[] response)
            : base(response)
        {
            if (VerifyOutcome() != EOutcome.OK)
                return;

            SlaveFirmwareVersions = new List<string>(4);
            byte[] val;

            int start = 0;
            for (int i = 0; i < Info.Length; i++)
            {
                // Separator
                if (Info[i] != 0x00 && i < Info.Length-1)
                    continue;

                // Empty
                if (start + 1 == i)
                    continue;

                // Assuming first Version is the Master
                if (start == 0)
                {
                    val = new byte[i];
                    Buffer.BlockCopy(Info, 0, val, 0, i);
                    MasterFirmwareVersion = Encoding.ASCII.GetString((val));
                }
                // Last one but no 0x00 delimiter
                else if (Info[i] != 0x00)
                {
                    val = new byte[i - start + 1];
                    Buffer.BlockCopy(Info, start, val, 0, i - start + 1);
                    SlaveFirmwareVersions.Add(Encoding.ASCII.GetString(val));
                }
                else
                {
                    // Strip the extra 0x00s
                    for (int j = start; j < i; j++)
                    {
                        if (Info[j] == 0x00)
                            start++;
                        else
                            break;
                    }

                    // Empty
                    if (i - start > 0)
                    {
                        val = new byte[i - start];
                        Buffer.BlockCopy(Info, start, val, 0, i - start);
                        SlaveFirmwareVersions.Add(Encoding.ASCII.GetString(val));
                    }
                }

                // Increment appropriately
                start = ++i;
            }
        }

        public string getContactlessVersion()
        {
            if (MasterFirmwareVersion.Contains("GBC"))
                return MasterFirmwareVersion;

            foreach (var ver in SlaveFirmwareVersions)
            {
                if (ver.Contains("GBC"))
                    return ver;
            }

            return "";
        }

        public string getPinpadVersion()
        {
            foreach (var ver in SlaveFirmwareVersions)
            {
                if (ver.Contains("GBK"))
                    return ver;
            }

            return "";
        }

        public string getMasterInfo()
        {
            int nStart = MasterFirmwareVersion.IndexOf(".GB") + 1;
            int nEnd = MasterFirmwareVersion.IndexOf(".", nStart);
            return MasterFirmwareVersion.Substring(nStart, nEnd-nStart);
        }

        public string getMasterType()
        {
            if (MasterFirmwareVersion.IndexOf(".GBR") > 0)
                return "Reader";
            else if (MasterFirmwareVersion.IndexOf(".GBC") > 0)
                return "Contactless";
            else if (MasterFirmwareVersion.IndexOf(".GBK") > 0)
                return "PinPad";
            else
                return "Unknown";
        }

        public string getPCIVersion()
        {
            string sFW = MasterFirmwareVersion.Trim('\0');
            if (sFW.IndexOf(".GBR") > 0)
                sFW = sFW.Substring(sFW.IndexOf(".GBR") + 4, 2); 
            else
                sFW = sFW.Substring(sFW.IndexOf(".GBC") + 4, 2);

            string sPinPad = ""; string sCL = "";
            foreach (var ver in SlaveFirmwareVersions)
            {
                if (ver.Contains("GBK"))
                    sPinPad = ver.Substring(ver.IndexOf("GBK")+3, 2);
                else if (ver.Contains("GBC"))
                    sCL = ver.Substring(ver.IndexOf("GBC") + 3, 2);
            }
            
            // No Pinpad or CL attached
            if (String.IsNullOrEmpty(sPinPad) && String.IsNullOrEmpty(sCL))
                return sFW;

            // If Pinpad attached
            else if (!String.IsNullOrEmpty(sPinPad))
            {
                // If they match
                if (sFW.Equals(sPinPad))
                {
                    // Pinpad & Reader Match
                    if (String.IsNullOrEmpty(sCL))
                        return sFW;

                    // And CL matches
                    else if (sFW.Equals(sCL))
                        return sFW;
                }

                // something doesnt match
                return "Mismatch";
            }
            // If CL only attached
            else if (!String.IsNullOrEmpty(sCL))
            {
                // If they match
                if (sFW.Equals(sCL))
                    return sFW;
                   
            }

            return "Mismatch";
        }

        /// <summary>
        /// Return a name value pair dictionary
        /// </summary>
        public Dictionary<string, string> ToDictionary()
        {
            var VersionList = new Dictionary<string, string>();

            int nextPosOfChar = MasterFirmwareVersion.IndexOf(":");
            if (nextPosOfChar > 0)
                VersionList.Add(MasterFirmwareVersion.Substring(0, nextPosOfChar++),
                    MasterFirmwareVersion.Substring(nextPosOfChar, MasterFirmwareVersion.Length - nextPosOfChar));
            else
                VersionList.Add("Master", MasterFirmwareVersion.Trim('\0'));

            for (int i = 0; i < SlaveFirmwareVersions.Count; i++)
            {
                nextPosOfChar = SlaveFirmwareVersions[i].IndexOf(":");

                if (nextPosOfChar > 0)
                    VersionList.Add(SlaveFirmwareVersions[i].Substring(0, nextPosOfChar++),
                        SlaveFirmwareVersions[i].Substring(nextPosOfChar, SlaveFirmwareVersions[i].Length - nextPosOfChar));
                else
                    VersionList.Add("Slave " + (i+1).ToString(), SlaveFirmwareVersions[i].Trim('\0'));
            }

            return VersionList;
        }
        /// <summary>
        /// Returns a well formatted string of the content of the object
        /// </summary>
        public string PrintDetails()
        {
            StringBuilder sb = new StringBuilder(1000);

            int nextPosOfChar = MasterFirmwareVersion.IndexOf(":");
            if (nextPosOfChar > 0)
                sb.AppendLine(MasterFirmwareVersion.Trim('\0'));
            else
                sb.AppendLine("Firmware Version:" + MasterFirmwareVersion.Trim('\0'));

            for (int i = 0; i < SlaveFirmwareVersions.Count; i++)
            {
                nextPosOfChar = SlaveFirmwareVersions[i].IndexOf(":");

                if (nextPosOfChar > 0)
                    sb.AppendLine(SlaveFirmwareVersions[i].Trim('\0'));
                else
                    sb.AppendLine("--> Slave " + (i+1).ToString() +" Version:" + SlaveFirmwareVersions[i].Trim('\0'));
            }

            sb.AppendLine();
            return sb.ToString();
        }

        /// <summary>
        /// Returns a well formatted XML string of the content of the object
        /// </summary>
        public string PrintDetailsAsXml()
        {
            StringBuilder sb = new StringBuilder(1000);

            sb.AppendLine("<FirmwareVersions>");
            sb.AppendLine("  <Master>" + MasterFirmwareVersion.Trim('\0') + "</Master>");

            for (int i = 0; i < SlaveFirmwareVersions.Count; i++)
            {
                sb.AppendLine("  <Slave Number='" + (i + 1).ToString() + "'>" + SlaveFirmwareVersions[i].Trim('\0') + "</Slave>");
            }

            sb.AppendLine("</FirmwareVersions>");
            return sb.ToString();
        }
    }

}
