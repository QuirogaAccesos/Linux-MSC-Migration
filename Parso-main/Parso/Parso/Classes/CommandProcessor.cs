//using BAC_ECR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Parso.Classes.Helpers;
using Parso.Classes.Helpers.Testing;
using Parso.Utils;
using Parso.Utils.Objects;
using Serilog;

namespace Parso.Classes
{
    internal class CommandProcessor
    {
        private PrinterHelper _printerHelper = new PrinterHelper();
        private PaymentHelper _rpPaymentHelper = new PaymentHelper();
        private PicobHelper _picobHelper = new PicobHelper();
        private TestingHelper _testingHelper = new TestingHelper();

        private ProjectConstants _projectConstants = ProjectConstants.Instance;

        //IPaymentProcessorService Service { get; set; }

        public string processCommand(string command)
        {
            try
            {
                Log.Information($"Processing command {command}");

                if (string.IsNullOrEmpty(command))
                {
                    return JsonConvert.SerializeObject(new Response(false, 400, false, $"ERROR: COMMAND RECEIVED AS EMPTY"), Newtonsoft.Json.Formatting.None);
                }

                Log.Information($"Command is not null");

                // Validate JSON structure first
                JObject json = JObject.Parse(command);
                var properties = json.Properties().ToList();

                if (properties.Count != 1)
                {
                    return JsonConvert.SerializeObject(new Response(false, 400, false, $"INVALID JSON: EXPECTED EXACTLY ONE TOP-LEVEL PROPERTY"), Newtonsoft.Json.Formatting.None);
                }
                Log.Information($"JSON structure validated");

                var operationProperty = properties[0];
                string operationKey = operationProperty.Name;

                // Parse operation key to determine routing
                if (!int.TryParse(operationKey, out int operationNumber))
                {
                    return JsonConvert.SerializeObject(new Response(false, 400, false, $"INVALID OPERATION KEY: {operationKey} IS NOT A NUMBER"), Newtonsoft.Json.Formatting.None);
                }

                Log.Information("Production mode");
                // Pass original UNMODIFIED command to helpers
                switch (operationNumber)
                {
                    case 1:
                        if (_projectConstants.ENABLE_PRINTER_TEST)
                        {
                            Log.Information("Starting printer test mode");
                            return _testingHelper.testCommand(command, operationNumber);
                        }
                            return _printerHelper.printCommand(command);

                    case 2:
                        if (_projectConstants.ENABLE_PICOB_TEST)
                        {
                            Log.Information("Starting picob test mode");
                            return _testingHelper.testCommand(command, operationNumber);
                        }
                        return _picobHelper.sendCommandToPicob(command);

                    case 3:
                        if (_projectConstants.ENABLE_PAYMENT_TEST)
                        {
                            Log.Information("Starting payment test mode");
                            return _testingHelper.testCommand(command, operationNumber);
                        }
                        return _rpPaymentHelper.CardPaymentCommand(command);
                    default:

                        return JsonConvert.SerializeObject(new Response(false, 400, false, $"ERROR: COMMAND NOT RECOGNIZED"), Newtonsoft.Json.Formatting.None);
                }
            }
            catch (JsonException ex)
            {
                Log.Error(ex.Message);
                return JsonConvert.SerializeObject(new Response(false, 400, false, $"INVALID JSON: EXCEPTION ENCOUNTERED - {ex.Message}"), Newtonsoft.Json.Formatting.None);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return JsonConvert.SerializeObject(new Response(false, 500, false, $"FATAL ERROR: EXCEPTION ENCOUNTERED - {ex.Message}"), Newtonsoft.Json.Formatting.None);
            }
        }
    }
}
