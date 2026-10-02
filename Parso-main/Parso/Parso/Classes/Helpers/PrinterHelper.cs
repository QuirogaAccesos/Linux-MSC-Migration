using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Parso.Utils;
using Parso.Utils.Objects;
using Serilog;
using System.Xml;
using TREAPrinting;
using TREAPrinting.Printer.Controllers;

namespace Parso.Classes.Helpers
{
    internal class PrinterHelper
    {
        private XmlDocument _xmlDocument = new XmlDocument();
        private IPrinterController _treaPrinterController = new CrossPlatformPrinterController();
        private ProjectConstants _projectConstants = ProjectConstants.Instance;

        // The request state lives in fields and the kiosk and the RestApi can print at the same time
        private static readonly object _printLock = new object();

        private string? templateFileName;
        private Dictionary<string, string> matchedVariablesDictionary = new Dictionary<string, string>();

        public string printCommand(string command)
        {
            lock (_printLock)
            {
                return printTemplate(command);
            }
        }

        private string printTemplate(string command)
        {
            try
            {
                // Parse JSON command
                var json = JObject.Parse(command);
                var operation = json.Properties().First();
                var templateData = (JObject)operation.Value;
                var templateProperty = templateData.Properties().First();

                string templateId = templateProperty.Name;
                JObject variablesData = (JObject)templateProperty.Value;

                templateFileName = _getTemplateTextFileAddressFromId(templateId);
                if (templateFileName != null)
                {
                    var printingVariables = _getPrintingVariables();
                    matchedVariablesDictionary = _getMatchedVariables(variablesData, printingVariables);

                    _treaPrinterController.Print(templateFileName, matchedVariablesDictionary, "Consolas", 8,
                        PaperSize: _projectConstants.PRINTING_PAPER_SIZE_MM, printerName: _projectConstants.PRINTING_PRINTER_NAME);

                    string result = JsonConvert.SerializeObject(new Response(true, 200, true, $"SUCCESS: PRINTING COMMAND SENT SUCCESSFULLY"), Newtonsoft.Json.Formatting.None);
                    return result;
                }
                return JsonConvert.SerializeObject(new Response(true, 400, false, $"ERROR: PRINTING TEMPLATE NOT FOUND"), Newtonsoft.Json.Formatting.None);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return JsonConvert.SerializeObject(new Response(false, 500, false, $"FATAL ERROR: EXCEPTION ENCOUNTERED - {ex.Message}"), Newtonsoft.Json.Formatting.None);
            }
        }

        private string? _getTemplateTextFileAddressFromId(string templateId)
        {
            _xmlDocument.Load($"{_projectConstants.PRINTING_TEMPLATE_FOLDER_LOCATION}/PrintingConfig.xml");
            XmlNode xmlRootNode = _xmlDocument.SelectSingleNode("root");

            List<string> variables = new List<string>();

            foreach (XmlNode variableNode in xmlRootNode.ChildNodes)
            {
                if (variableNode.Name == "template")
                {
                    if (variableNode.SelectSingleNode("id").InnerText == templateId)
                    {
                        return $"{_projectConstants.PRINTING_TEMPLATE_FOLDER_LOCATION}/{variableNode.SelectSingleNode("templateFileName").InnerText}";
                    }
                }
            }
            return null;
        }

        private Dictionary<string, string> _getPrintingVariables()
        {
            Dictionary<string, string> printingVariablesDictionary = new Dictionary<string, string>();

            string variableId = "";
            string variableTextId = "";

            _xmlDocument.Load($"{_projectConstants.PRINTING_TEMPLATE_FOLDER_LOCATION}/PrintingConfig.xml");
            XmlNode xmlRootNode = _xmlDocument.SelectSingleNode("root");

            List<string> variables = new List<string>();

            foreach (XmlNode variableNode in xmlRootNode.ChildNodes)
            {
                if (variableNode.Name == "variable")
                {
                    foreach (XmlNode dataNode in variableNode.ChildNodes)
                    {
                        if (dataNode.Name == "id")
                        {
                            variableId = dataNode.InnerText;
                        }
                        if (dataNode.Name == "textId")
                        {
                            variableTextId = dataNode.InnerText;
                        }
                    }
                    printingVariablesDictionary.Add(variableId, variableTextId);
                }
            }
            return printingVariablesDictionary;
        }

        private Dictionary<string, string> _getMatchedVariables(JObject variablesData, Dictionary<string, string> printingVariablesDictionary)
        {
            var variableDictionary = new Dictionary<string, string>();

            foreach (var variable in variablesData.Properties())
            {
                var xmlKey = variable.Name;
                var value = variable.Value.ToString();

                if (printingVariablesDictionary.TryGetValue(xmlKey, out var placeholder))
                {
                    variableDictionary[placeholder] = value;
                }
            }

            return variableDictionary;
        }
    }
}
