using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Configuration;

namespace AA.LinuxController
{
    public class LinuxControllerPrinter
    {
        private ILog log = LogManager.GetLogger("LinuxControllerPrinter");

        private static int printerTimeout = int.Parse(ConfigurationManager.AppSettings["LinuxControllerPrinterTimeout"] ?? "20000");

        private readonly LinuxControllerClient client = new LinuxControllerClient();

        public bool Print(string templateId, Dictionary<string, string> variables)
        {
            var request = new JObject(new JProperty("1", new JObject(new JProperty(templateId, JObject.FromObject(variables)))));

            LinuxControllerResponse response = client.Send(request.ToString(Formatting.None), printerTimeout);
            if (response == null)
                return false;

            if (!response.OperationResult)
            {
                log.Error("PRINT NOT DONE: " + response.Message);
                return false;
            }

            return true;
        }
    }
}