using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
//using System.Windows.Forms;

namespace CCI.Globalcom.GlobalcomRetailProtocol
{   /// <summary>
    /// Response Class for Key Information Requests
    /// </summary>
    public class RpResponseKeyInfo : RpResponseBase
    {
        public List<string> _keyLines;

        public string _md5Family;

        public string _pinKsi;
        public string _dataKsi;      
        

        public RpResponseKeyInfo(byte[] response)
             : base(response)
        {
            _pinKsi = _pinKsi = _dataKsi = null;
            if (VerifyOutcome() != EOutcome.OK)
                return;
                
            _keyLines = new List<string>();

            string sInfo = Encoding.UTF8.GetString(Info);
            _keyLines.AddRange(sInfo.Split('\x0A'));

            // Pin Key
            string parse = "";

            parse = _keyLines.FirstOrDefault(s => s.Contains("0002dukpt.ksn"));
            if (parse != null)
                _pinKsi = parse.Substring(parse.LastIndexOf(' ')+1).Trim('\r');

            parse = _keyLines.FirstOrDefault(s => s.Contains("0003dukpt.ksn"));
            if (parse != null)
                _dataKsi = parse.Substring(parse.LastIndexOf(' ')+1).Trim('\r');

            parse = _keyLines.FirstOrDefault(s => s.Contains("MD5 key"));
            if (parse != null)
                _md5Family = parse.Substring(parse.LastIndexOf(' ')+1).Trim('\r');
        }

        public string GetFamily()
        {
            //changed this from the word none to ?? because sometimes the firmware doesn't support the get keys command unless the unit is anti-removal armed... and its confusing with "none".
            if (String.IsNullOrEmpty(_md5Family))
                return "?? ";

            if (_md5Family.Contains("482B881876DF710334337A54E5D1431C"))
                return "Develop";
            else if (_md5Family.Contains("BA1646A92DF01F51C5EC7763A9075C67"))
                return "Foreign";
            else if (_md5Family.Contains("CE4A924E2D1D13EE8B88B572E9A3C355"))
                return "Amano";
            else if (_md5Family.Contains("5E5104697E57DF4A945191847F328DE1"))
                return "WPS";
            else if (_md5Family.Contains("078E557949398C07F7F059551F0B7904"))
                return "T2";
            else if (_md5Family.Contains("BDEA672552696405FB6A86E0CA692585"))
                return "Gilbarco";
            else if (_md5Family.Contains("4DE5C6A5A65C5BC719C4AFBF20DBFFDF"))
                return "Hectronic";
            else if (_md5Family.Contains("B3F3805744AE8B146673D34FE486B534"))
                return "SAP";
            else if (_md5Family.Contains("078E557949398C07F7F059551F0B7904"))
                return "T2";
            else if (_md5Family.Contains("89C96D6924B3DAEA3927899DF7753AFF"))
                return "EGenuity";
            else if (_md5Family.Contains("F33475AEDC1BA7CF4AD99DD17783D248"))
                return "Designa";
            else if (_md5Family.Contains("B5C9D2BDF25DD9834F7F87EA056C97AA"))
                return "Keytop";
            else if (_md5Family.Contains("E1722AB9275667EA6010989EFB161037"))
                return "Keytop-CA";
            else if (_md5Family.Contains("2DDDCBEC2DDEAF8D72EA4C907B2035D5"))
                return "RTB";
            else
                return "??";
        }

        public string GetPinType()
        {
            if (String.IsNullOrEmpty(_pinKsi))
                return "??";

            // Global Test PIN Key (FFFF3D0100)
            if (_pinKsi.Contains("3D0100"))
                return "Apriva Global Test";
            //    FFFF619745 Cenex CHS II
            else if (_pinKsi.Contains("FFFF619745"))
                return "Cenex CHS II";
            //    FFFF619747 NBS GENERIC II
            else if (_pinKsi.Contains("FFFF619747"))
                return "NBS GENERIC II";
            //    FFFF615669 NBS Test Key
            else if (_pinKsi.Contains("FFFF615669"))
                return "NBS Test Key";
            else if (_pinKsi.Contains("FFFF987654"))
                return "NMI Test";
            else
                return ("?? " + _pinKsi);
        }

        public string GetDataType()
        {
            if (String.IsNullOrEmpty(_dataKsi))
                return "??";

            // Apriva Test Key (217832CA01)
            if (_dataKsi.Contains("217832CA01"))
                return "Apriva Test";
            // NMI Test Key (FFFF987654)
            else if (_dataKsi.Contains("FFFF987654"))
                return "NMI Test";
            // Apriva Production Key (218832CA01)
            else if (_dataKsi.Contains("218832CA01"))
                return "Apriva Production";
            else if (_dataKsi.Contains("8944170902"))
                return "NMI Production";
            else
                return ("?? " + _dataKsi);
        }

        /// <summary>
        /// Returns a well formatted string of the content of the object
        /// </summary>
        public string PrintDetails()
        {
            StringBuilder sb = new StringBuilder(1000);
            sb.AppendLine("Keys:");

            if (VerifyOutcome() != EOutcome.OK)
            {
                sb.AppendLine(" ######### Error Reading keys: Outcome:" + VerifyOutcome());
            }
            else if (_keyLines.Count < 2)
                sb.AppendLine(" ######## No Loaded Keys.");
            else
            {
                foreach (string line in _keyLines)
                {
                    if (line.Length > 1)
                    {
                        string keyId = line.Substring(0, line.LastIndexOf(' ')).Trim('\0');
                        if (keyId.Contains("0002dukpt.ksn"))
                            sb.Append("-->" + keyId + " (" + GetPinType()  + ")  " + line.Substring(line.LastIndexOf(' ')).Trim('\0'));
                        else if (keyId.Contains("0003dukpt.ksn"))
                            sb.Append("-->" + keyId + " (" + GetDataType() + ")  " + line.Substring(line.LastIndexOf(' ')).Trim('\0'));
                        else if (keyId.Contains("MD5 key"))
                            sb.Append("-->" + keyId + " (" + GetFamily() + ")  " + line.Substring(line.LastIndexOf(' ')).Trim('\0'));
                        else
                            sb.Append("-->" + keyId + "  " + line.Substring(line.LastIndexOf(' ')).Trim('\0'));
                    }
                }
            }

            sb.AppendLine();
            sb.AppendLine();
            return sb.ToString();
        }

        /// <summary>
        /// Returns a well formatted string of the content of the object
        /// </summary>
        public string PrintDetailsAsXml()
        {
            StringBuilder sb = new StringBuilder(1000);
            // Error Outcome
            if (VerifyOutcome() != EOutcome.OK)
            {
                sb.AppendLine("<Keys/>");
                return sb.ToString();
            }
            else if (_keyLines.Count < 2)
            {
                sb.AppendLine("<Keys/>");
                return sb.ToString();
            }

            // Pull Details
            sb.AppendLine("<Keys>");

            foreach (string line in _keyLines)
            {
                if (line.Length > 1)
                {
                    string keyId = line.Substring(0, line.LastIndexOf(' ')).Trim('\0');
                    string desc = getKeyDesc(keyId);

                    if (string.IsNullOrEmpty(desc))
                        sb.AppendLine("  <Key> <Name>" + keyId +
                                    "</Name><Value>" + line.Substring(line.LastIndexOf(' ')).Trim('\r') +
                                    "</Value></Key>");
                    else
                        sb.AppendLine("  <Key> <Name>" + keyId +
                                    "</Name><Value>" + line.Substring(line.LastIndexOf(' ')).Trim('\r') +
                                    "</Value><Desc>" + getKeyDesc(keyId) + "</Desc></Key>");
                }
            }

            sb.AppendLine("</Keys>");
            return sb.ToString();
        }

        public string getKeyDesc(string keyId)
        {
            if (keyId.Contains("0002dukpt.ksn"))
                return GetPinType();
            else if (keyId.Contains("0003dukpt.ksn"))
                return GetDataType();
            else if (keyId.Contains("MD5 key"))
                return GetFamily();
            
            return "";
        }
    }
}
