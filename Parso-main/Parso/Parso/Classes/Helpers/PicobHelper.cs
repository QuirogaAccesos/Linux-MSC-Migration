using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Parso.Utils;
using Parso.Utils.Objects;
using PicobControllers;
using PicobControllers.Controllers;
using Serilog;

namespace Parso.Classes.Helpers
{
    internal class PicobHelper
    {
        ProjectConstants _projectConstants = ProjectConstants.Instance;

        private IPicob _picobController = new PicobController();

        JObject _json;
        JProperty _operationProperty;
        string _picobCommand;

        public string sendCommandToPicob(string command)
        {
            try
            {
                // Parse JSON command
                _json = JObject.Parse(command);
                _operationProperty = _json.Properties().First();
                _picobCommand = _operationProperty.Value.ToString();

                if (!_picobController.OpenPicobPort(_projectConstants.PICOB_COM_PORT, _projectConstants.PICOB_BAUD_RATE, true, true, "\r\n"))
                {
                    return JsonConvert.SerializeObject(new Response(true, 500, false, $"ERROR: CANNOT OPEN PICOB COM PORT {_projectConstants.PICOB_COM_PORT}"), Formatting.None);
                }
            }
            catch (JsonException ex)
            {
                Log.Error(ex.Message);
                return JsonConvert.SerializeObject(new Response(false, 400, false, $"FATAL ERROR: INVALID JSON COMMAND - {ex.Message}"), Formatting.None);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return JsonConvert.SerializeObject(new Response(false, 400, false, $"FATAL ERROR: EXCEPTION ENCOUNTERED WHILE SENDING DATA TO PICOB - {ex.Message}"), Formatting.None);
            }

            try
            {
                string result = _picobController.SendToPicobAndReceive(_picobCommand, _projectConstants.PICOB_REPONSE_TIMEOUT_MS);
                _picobController.ClosePort();
                if (result is null) { return JsonConvert.SerializeObject(new Response(true, 500, false, $"ERROR: EMPTY RESPONSE RECEIVED FROM PICOB DEVICE"), Formatting.None); }
                string trimmedJson = result.Trim();

                return JsonConvert.SerializeObject(new Response(true, 200, true, $"SUCCESS: DATA SENT TO PICOB SUCCESSFULLY", trimmedJson), Formatting.None);
            }
            catch (Exception ex)
            {
                _picobController.ClosePort();
                Log.Error(ex.Message);
                return JsonConvert.SerializeObject(new Response(false, 400, false, $"FATAL ERROR: EXCEPTION ENCOUNTERED WHILE SENDING DATA TO PICOB - {ex.Message}"), Formatting.None);
            }
        }
    }
}
