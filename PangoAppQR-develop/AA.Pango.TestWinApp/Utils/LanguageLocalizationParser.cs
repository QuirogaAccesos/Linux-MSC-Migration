using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Xml;

namespace AA.Pango.TestWinApp.Utils
{
    public class LanguageLocalizationParser
    {
        private static ILog _log = LogManager.GetLogger("LanguageLocalizationParser");

        public static Dictionary<string, Dictionary<string, string>> Localizations = new Dictionary<string, Dictionary<string, string>>();

        public static readonly string CurrentLocalization = ConfigurationManager.AppSettings["CurrentLanguage"] ?? "Spanish";

        public static Dictionary<string, string> GetCurrentLanguageTags
        {
            get
            {
                return Localizations[CurrentLocalization];
            }
        }

        public static bool LoadLocalizations()
        {
            try
            {
                if (!File.Exists("languagesSupported.xml"))
                {
                    return false;
                }

                XmlDocument xmldoc = new XmlDocument();
                xmldoc.Load("languagesSupported.xml");
                XmlNodeList nodeList = xmldoc.SelectNodes("Languages/Language");

                if (nodeList.Count > 0)
                {
                    foreach (XmlNode node in nodeList)
                    {
                        LoadLocalization(node.Attributes["file"].Value);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                return false;
            }
        }

        private static void LoadLocalization(string pFile)
        {
            try
            {
                if (!File.Exists(pFile))
                {
                    return;
                }

                XmlDocument xmldoc = new XmlDocument();
                xmldoc.Load(pFile);
                XmlNodeList nodeList = xmldoc.SelectNodes("Language/Localized");

                var pLang = xmldoc.GetElementsByTagName("Language")[0].Attributes["name"].Value;

                if(!Localizations.ContainsKey(pLang))
                    Localizations.Add(pLang, new Dictionary<string, string>());

                if (nodeList.Count > 0)
                {
                    foreach (XmlNode node in nodeList)
                    {
                        if(!Localizations[pLang].ContainsKey(node.Attributes["name"].Value))
                            Localizations[pLang].Add(node.Attributes["name"].Value, node.Attributes["value"].Value);
                    }
                }

            }
            catch (Exception ex)
            {
                _log.Warn(ex.Message);
            }
        }
    }
}