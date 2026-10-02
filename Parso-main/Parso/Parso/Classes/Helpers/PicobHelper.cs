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

                if (_projectConstants.PICOB_PERSISTENT_CONNECTION) return sendCommandThroughSession(_picobCommand);

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

        // Persistent mode: the port belongs to PicobSession, which keeps it open and polls the presence
        private string sendCommandThroughSession(string command)
        {
            PicobSession session = PicobSession.Instance;
            PicobStatus status = session.GetStatus();

            if (!status.Connected)
            {
                return JsonConvert.SerializeObject(new Response(true, 500, false, $"ERROR: PICOB NOT CONNECTED"), Formatting.None);
            }

            try
            {
                if (command.Equals("STATUS", StringComparison.OrdinalIgnoreCase))
                {
                    // ageMs and C are left out until the first successful poll
                    string statusJson = JsonConvert.SerializeObject(new { connected = status.Connected, ageMs = status.AgeMs, C = status.C },
                        new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                    return JsonConvert.SerializeObject(new Response(true, 200, true, $"SUCCESS: DATA SENT TO PICOB SUCCESSFULLY", statusJson), Formatting.None);
                }

                if (_projectConstants.PICOB_FIRE_AND_FORGET_COMMANDS.Contains(command.ToUpperInvariant()))
                {
                    session.Send(command, false);
                    return JsonConvert.SerializeObject(new Response(true, 200, true, $"SUCCESS: COMMAND SENT"), Formatting.None);
                }

                string result = session.Send(command, true);
                if (string.IsNullOrEmpty(result)) { return JsonConvert.SerializeObject(new Response(true, 500, false, $"ERROR: EMPTY RESPONSE RECEIVED FROM PICOB DEVICE"), Formatting.None); }

                return JsonConvert.SerializeObject(new Response(true, 200, true, $"SUCCESS: DATA SENT TO PICOB SUCCESSFULLY", result), Formatting.None);
            }
            catch (InvalidOperationException ex)
            {
                // The port was closed after the status check
                Log.Error(ex.Message);
                return JsonConvert.SerializeObject(new Response(true, 500, false, $"ERROR: PICOB NOT CONNECTED"), Formatting.None);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return JsonConvert.SerializeObject(new Response(false, 400, false, $"FATAL ERROR: EXCEPTION ENCOUNTERED WHILE SENDING DATA TO PICOB - {ex.Message}"), Formatting.None);
            }
        }
    }
}
