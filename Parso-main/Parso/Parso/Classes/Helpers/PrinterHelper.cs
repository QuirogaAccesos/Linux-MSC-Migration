using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Parso.Utils;
using Parso.Utils.Objects;
using QRCoder;
using Serilog;
using System.Text;
using System.Xml;
using TREAPrinting;
using TREAPrinting.Printer.Controllers;
using TREAPrinting.Printer.Modelos.TREAPrinting;

namespace Parso.Classes.Helpers
{
    internal class PrinterHelper
    {
        private XmlDocument _xmlDocument = new XmlDocument();
        private IPrinterController _treaPrinterController = new CrossPlatformPrinterController();
        private ProjectConstants _projectConstants = ProjectConstants.Instance;

        // The request state lives in fields and the kiosk and the RestApi can print at the same time
        private static readonly object _printLock = new object();

        private const string QrMarker = "@@QR@@";
        private const string TextFont = "Consolas";
        private const int TextSize = 8;

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

                    // The reserved variable QR is printed as an image where the template has the marker
                    string? qrText = variablesData["QR"]?.ToString();
                    string templateText = string.IsNullOrEmpty(qrText) ? "" : _getTemplateText(templateFileName, matchedVariablesDictionary);

                    if (templateText.Contains(QrMarker))
                    {
                        _printWithQr(templateText, qrText!);
                    }
                    else
                    {
                        _treaPrinterController.Print(templateFileName, matchedVariablesDictionary, TextFont, TextSize,
                            PaperSize: _projectConstants.PRINTING_PAPER_SIZE_MM, printerName: _projectConstants.PRINTING_PRINTER_NAME);
                    }

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

        private string _getTemplateText(string templateFile, Dictionary<string, string> variables)
        {
            var text = new StringBuilder();
            foreach (var line in File.ReadLines(templateFile)) { text.AppendLine(line); }
            foreach (var variable in variables) { text.Replace(variable.Key, variable.Value); }
            return text.ToString();
        }

        private void _printWithQr(string templateText, string qrText)
        {
            int markerIndex = templateText.IndexOf(QrMarker);
            string textBefore = templateText.Substring(0, markerIndex).TrimEnd('\r', '\n');
            string textAfter = templateText.Substring(markerIndex + QrMarker.Length).Replace(QrMarker, "").TrimStart('\r', '\n');

            // Left aligned text keeps the padding spaces of the template, the QR is centered
            var elements = new List<PrintElement>();
            if (!string.IsNullOrWhiteSpace(textBefore))
            {
                elements.Add(new PrintElement { Text = textBefore, FontFamily = TextFont, FontSize = TextSize, Alignment = "left" });
            }
            elements.Add(new PrintElement { ImageData = _getQrImage(qrText), Alignment = "center" });
            if (!string.IsNullOrWhiteSpace(textAfter))
            {
                elements.Add(new PrintElement { Text = textAfter, FontFamily = TextFont, FontSize = TextSize, Alignment = "left" });
            }

            _treaPrinterController.PrintDynamic(elements, _projectConstants.PRINTING_PAPER_SIZE_MM, _projectConstants.PRINTING_PRINTER_NAME);
        }

        private byte[] _getQrImage(string qrText)
        {
            QRCodeGenerator.ECCLevel eccLevel = _projectConstants.PRINTING_QR_ECC_LEVEL switch
            {
                "M" => QRCodeGenerator.ECCLevel.M,
                "Q" => QRCodeGenerator.ECCLevel.Q,
                "H" => QRCodeGenerator.ECCLevel.H,
                _ => QRCodeGenerator.ECCLevel.L
            };

            using var qrGenerator = new QRCodeGenerator();
            using QRCodeData qrData = qrGenerator.CreateQrCode(qrText, eccLevel);
            return new PngByteQRCode(qrData).GetGraphic(_projectConstants.PRINTING_QR_PIXELS_PER_MODULE);
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
