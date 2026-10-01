using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCI.Globalcom.GlobalcomRetailProtocol
{    
     /// <summary> Type of Versions that are returnable </summary>
    public enum ExtFirmwareVersionType
    {
        StandardREEVersion = 0x00,
        AllREEVersions = 0x01,
        AllTEEVersions = 0x02,
        AllLXVersions = 0x03,
        ALLREETEELXVersions = 0x0A,
        KCVForAllInjectedKeys = 0x09,
        UndefinedVersion = 0xFF   // Note not in spec
    }
    
    /// <summary>
    /// Response Class for Firmware Version Requests
    /// </summary>
    public class RpResponseExtFirmwareVersion : RpResponseBase
    {
        public ExtFirmwareVersionType VersionType = ExtFirmwareVersionType.UndefinedVersion;
        public Dictionary<string, string> VersionList;

        public RpResponseExtFirmwareVersion(byte[] response, ExtFirmwareVersionType type)
            : base(response)
        {
            // Maybe a BV1000 or failed for other reason
            if (VerifyOutcome() != EOutcome.OK)
                return;

            VersionList = new Dictionary<string,string>(1);
            VersionType = type;

            int start = 1;
            for (int i = 1; i < Info.Length - 2; i++)
            {
                if (Info[i] == 0x0D)
                {
                    string version = Encoding.ASCII.GetString(Info, start, i - start);
                    int nextPosOfChar = 0;
                    if (type == ExtFirmwareVersionType.KCVForAllInjectedKeys)
                        nextPosOfChar = version.IndexOf(' ');
                    else
                        nextPosOfChar = version.IndexOf(':');

                    if (nextPosOfChar > 0)
                        VersionList.Add(version.Substring(0, nextPosOfChar++),
                            version.Substring(nextPosOfChar, version.Length - nextPosOfChar));
                    else
                        VersionList.Add(version, "");

                    i += 2;
                    start = i;
                }
            }

        }

        /// <summary>
        /// Returns a well formatted string of the content of the object
        /// </summary>
        public string PrintDetails()
        {
            StringBuilder sb = new StringBuilder(1000);

            switch (VersionType)
            {
                case ExtFirmwareVersionType.StandardREEVersion:
                    sb.AppendLine("Standard REE Version: ");
                    break;
                case ExtFirmwareVersionType.AllREEVersions:
                    sb.AppendLine("All REE Version: ");
                    break;
                case ExtFirmwareVersionType.AllTEEVersions:
                    sb.AppendLine("All TEE Version: ");
                    break;
                case ExtFirmwareVersionType.AllLXVersions:
                    sb.AppendLine("All LX Version: ");
                    break;
                case ExtFirmwareVersionType.ALLREETEELXVersions:
                    sb.AppendLine("All REE/TEE/LX Version: ");
                    break;
                case ExtFirmwareVersionType.KCVForAllInjectedKeys:
                    sb.AppendLine("KCV For All Injected Keys: ");
                    break;
                default:
                    sb.AppendLine("No Extended Versions" + Environment.NewLine);
                    return sb.ToString();
            }

            foreach (KeyValuePair<string, string> kvp in VersionList)
                sb.AppendLine("--" + kvp.Key + " :" + kvp.Value);

            sb.AppendLine(Environment.NewLine);

            return sb.ToString();
        }
        /// <summary>
        /// Returns a well formatted xml string of the content of the object
        /// </summary>
        public string PrintDetailsAsXml()
        {
            StringBuilder sb = new StringBuilder(1000);
            switch (VersionType)
            {
                case ExtFirmwareVersionType.StandardREEVersion:
                    sb.AppendLine("<ExtendedVersions Type='Standard REE Version'>");
                    break;
                case ExtFirmwareVersionType.AllREEVersions:
                    sb.AppendLine("<ExtendedVersions Type='All REE Version'>");
                    break;
                case ExtFirmwareVersionType.AllTEEVersions:
                    sb.AppendLine("<ExtendedVersions Type='All TEE Version'>");
                    break;
                case ExtFirmwareVersionType.AllLXVersions:
                    sb.AppendLine("<ExtendedVersions Type='All LX Version'>");
                    break;
                case ExtFirmwareVersionType.ALLREETEELXVersions:
                    sb.AppendLine("<ExtendedVersions Type='All REE/TEE/LX Version'>");
                    break;
                case ExtFirmwareVersionType.KCVForAllInjectedKeys:
                    sb.AppendLine("<ExtendedVersions Type='KCV For All Injected Keys'>");
                    break;
                default:
                    sb.AppendLine("<ExtendedVersions/>");
                    return sb.ToString();
            }

            foreach (KeyValuePair<string, string> kvp in VersionList)
                sb.AppendLine("<" + kvp.Key + ">" + kvp.Value + "</" + kvp.Key + ">");

            sb.AppendLine("</ExtendedVersions>");

            return sb.ToString();
        }
    }

}
