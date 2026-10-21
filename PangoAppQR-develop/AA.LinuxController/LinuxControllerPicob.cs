using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Configuration;

namespace AA.LinuxController
{
    public class LinuxControllerPicob
    {
        private ILog log = LogManager.GetLogger("LinuxControllerPicob");

        private static int picobTimeout = int.Parse(ConfigurationManager.AppSettings["LinuxControllerPicobTimeout"] ?? "5000");

        private const string StatusRequest = "{\"2\":\"STATUS\"}";
        private const string OpenBarrierRequest = "{\"2\":\"A\"}";
        private const string HardResetRequest = "{\"2\":\"H\"}";

        private readonly LinuxControllerClient client = new LinuxControllerClient();

        public LinuxControllerPicobStatus GetStatus()
        {
            LinuxControllerResponse response = client.Send(StatusRequest, picobTimeout);
            if (response == null)
                return null;

            if (!response.OperationResult || string.IsNullOrEmpty(response.DeviceResponse))
                return new LinuxControllerPicobStatus { Connected = false };

            try
            {
                return JsonConvert.DeserializeObject<LinuxControllerPicobStatus>(response.DeviceResponse);
            }
            catch (JsonException ex)
            {
                log.Error("INVALID PICOB STATUS: " + response.DeviceResponse + " - " + ex.Message);
                return null;
            }
        }

        public bool OpenBarrier()
        {
            LinuxControllerResponse response = client.Send(OpenBarrierRequest, picobTimeout);
            if (response == null)
                return false;

            try
            {
                if (response.OperationResult && !string.IsNullOrEmpty(response.DeviceResponse) &&
                    JObject.Parse(response.DeviceResponse).Value<int?>("A") == 1)
                    return true;
            }
            catch (Exception ex)
            {
                log.Error("INVALID OPEN BARRIER RESPONSE: " + response.DeviceResponse + " - " + ex.Message);
                return false;
            }

            log.Error("BARRIER NOT OPENED: " + response.Message + " " + response.DeviceResponse);
            return false;
        }

        public bool HardReset()
        {
            LinuxControllerResponse response = client.Send(HardResetRequest, picobTimeout);
            if (response == null)
                return false;

            if (!response.OperationResult)
            {
                log.Error("HARD RESET NOT SENT: " + response.Message);
                return false;
            }

            return true;
        }
    }
}