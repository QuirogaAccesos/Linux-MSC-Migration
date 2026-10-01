using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CCI.Globalcom.GlobalcomRetailProtocol;


namespace CCI.Globalcom.GlobalcomRetailProtocol
{
    /// <summary>
    ///   Structure to hold the Dictionary of Keys and their fingerprint
    /// </summary>
    public static class GBCKeyTypes
    {
        /// <summary>
        ///  Dictionary of Keys and their fingerprints
        /// </summary>
        public static Dictionary<string, string> gbcKeysDictionary =
            new Dictionary<string, string>()
            {
                {"482B881876DF710334337A54E5D1431C", "Develop"},
                {"BA1646A92DF01F51C5EC7763A9075C67", "Foreign"},
                {"CE4A924E2D1D13EE8B88B572E9A3C355", "Amano"},
                {"5E5104697E57DF4A945191847F328DE1", "WPS"},
                {"4DE5C6A5A65C5BC719C4AFBF20DBFFDF", "Hectronic"},
                {"B3F3805744AE8B146673D34FE486B534", "SAP"},
                {"078E557949398C07F7F059551F0B7904", "T2"},
                {"89C96D6924B3DAEA3927899DF7753AFF", "EGenuity"},
                {"F33475AEDC1BA7CF4AD99DD17783D248", "Designa"},
                {"B5C9D2BDF25DD9834F7F87EA056C97AA", "Keytop"},
                {"E1722AB9275667EA6010989EFB161037", "Keytop-CA"},
                {"BDEA672552696405FB6A86E0CA692585", "Gilbarco"},
                {"2DDDCBEC2DDEAF8D72EA4C907B2035D5", "RTB" },
            };

        public static string GetSigningKeyInfo(RetailProtocol rp)
        {
            // Pull the Keys
            RpResponseBase rpKeys = rp.RP_GetKeyInformation();
            if (rpKeys.VerifyOutcome() != EOutcome.OK)
                throw new Exception("Error Reading keys: Outcome:" + rpKeys.VerifyOutcome());
            string keyInfo = Encoding.UTF8.GetString(rpKeys.Info);

            // Find the Match
            List<string> logLines = new List<string>();
            logLines.AddRange(keyInfo.Split('\x0A'));

            string theKey = "";
            foreach (string line in logLines)
            {
                if (line.Length == 0)
                    continue;

                string keyType = line.Substring(0, line.LastIndexOf(' '));
                if (keyType.Equals("MD5 key"))
                {
                    theKey = line.Substring(line.LastIndexOf(' '));
                    theKey = theKey.TrimStart(' ');
                    theKey = theKey.TrimEnd('\x0D');

                    break;
                }
            }

            if (theKey.Length == 0)
                throw new Exception("Couldn't determine signing Key");

            theKey = GBCKeyTypes.gbcKeysDictionary[theKey];

            if (theKey.Length == 0)
                throw new Exception("Unknown Key");

            return theKey;
        }
    }
}