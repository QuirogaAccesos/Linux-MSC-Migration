using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace AA.Pango.TestWinApp.Utils
{
    public class DataFileParser
    {
        private static ILog _log = LogManager.GetLogger("DataFileParser");

        public static Dictionary<string, string> Styles = new Dictionary<string, string>();

        public static void LoadStyles(string pFile)
        {
            try
            {
                if (!File.Exists(pFile))
                    return;

                XmlDocument xmldoc = new XmlDocument();
                xmldoc.Load(pFile);
                XmlNodeList nodeList = xmldoc.SelectNodes("Language/Localized");

                if (nodeList.Count > 0)
                {
                    foreach (XmlNode node in nodeList)
                    {
                        if (!Styles.ContainsKey(node.Attributes["name"].Value))
                            Styles.Add(node.Attributes["name"].Value, node.Attributes["value"].Value);
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
