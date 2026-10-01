using Newtonsoft.Json;
using Parso.Utils.Objects;
using System.Globalization;

namespace Parso.Classes.Helpers.Testing
{
    internal class A1Tester
    {
        public string testA1PicobResponses(string picobCommandKey )
        {
            string result;
            var picob = PicobResponse.Instance;

            switch (picobCommandKey)
            {
                case "R":
                    picob.R ^= 1;
                    result = JsonConvert.SerializeObject(new Response(true, 200, true, $"SUCCESS: COMMAND SENT AND RESPONSE RECEIVED",
                        JsonConvert.SerializeObject(new { picob.R }, Formatting.None)), Formatting.None);
                    break;
                case "G":
                    picob.G ^= 1;
                    result = JsonConvert.SerializeObject(new Response(true, 200, true, $"SUCCESS: COMMAND SENT AND RESPONSE RECEIVED",
                        JsonConvert.SerializeObject(new { picob.G }, Formatting.None)), Formatting.None);
                    break;
                case "B":
                    picob.B ^= 1;
                    result = JsonConvert.SerializeObject(new Response(true, 200, true, $"SUCCESS: COMMAND SENT AND RESPONSE RECEIVED",
                        JsonConvert.SerializeObject(new { picob.B }, Formatting.None)), Formatting.None);
                    break;
                case "F":
                    result = JsonConvert.SerializeObject(new Response(true, 200, true, $"SUCCESS: COMMAND SENT AND RESPONSE RECEIVED",
                        JsonConvert.SerializeObject(new { picob.F }, Formatting.None)), Formatting.None);
                    break;
                case "Z":
                    result = JsonConvert.SerializeObject(new Response(true, 200, true, $"SUCCESS: COMMAND SENT AND RESPONSE RECEIVED",
                        JsonConvert.SerializeObject(new { picob.Z }, Formatting.None)), Formatting.None);
                    break;
                case "H":
                    result = JsonConvert.SerializeObject(new Response(true, 200, true, $"SUCCESS: COMMAND SENT AND RESPONSE RECEIVED",
                        JsonConvert.SerializeObject(new { picob.H }, Formatting.None)), Formatting.None);
                    Environment.Exit(0);
                    break;
                case "T":
                    result = JsonConvert.SerializeObject(new Response(true, 200, true, $"SUCCESS: COMMAND SENT AND RESPONSE RECEIVED",
                        JsonConvert.SerializeObject(new { picob.T }, Formatting.None)), Formatting.None);
                    break;
                case "U":
                    result = JsonConvert.SerializeObject(new Response(true, 200, true, $"SUCCESS: COMMAND SENT AND RESPONSE RECEIVED",
                        JsonConvert.SerializeObject(new { picob.U }, Formatting.None)), Formatting.None);
                    break;
                case "J":
                    result = JsonConvert.SerializeObject(new Response(true, 200, true, $"SUCCESS: COMMAND SENT AND RESPONSE RECEIVED",
                        JsonConvert.SerializeObject(new { picob.J }, Formatting.None)), Formatting.None);
                    break;
                case "L":
                    result = JsonConvert.SerializeObject(new Response(true, 200, true, $"SUCCESS: COMMAND SENT AND RESPONSE RECEIVED",
                        JsonConvert.SerializeObject(new { picob.L }, Formatting.None)), Formatting.None);
                    break;
                case "S":
                    result = JsonConvert.SerializeObject(new Response(true, 200, true, $"SUCCESS: COMMAND SENT AND RESPONSE RECEIVED",
                        JsonConvert.SerializeObject(new { picob.S }, Formatting.None)), Formatting.None);
                    break;
                case "X":
                    result = JsonConvert.SerializeObject(new Response(true, 200, true, $"SUCCESS: COMMAND SENT AND RESPONSE RECEIVED",
                        JsonConvert.SerializeObject(new { picob }, Formatting.None)), Formatting.None);
                    break;
                default:
                    result = JsonConvert.SerializeObject(new Response(false, 400, false, $"ERROR: UNRECOGNIZED COMMAND"), Formatting.None);
                    break;
            }
            return result;
        }
    }
}
