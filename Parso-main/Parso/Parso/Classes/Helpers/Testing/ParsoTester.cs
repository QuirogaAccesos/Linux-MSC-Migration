using Newtonsoft.Json;
using Parso.Utils.Objects;
using System.Globalization;

namespace Parso.Classes.Helpers.Testing
{
    internal class ParsoTester
    {
        public string testParsoPicobResponses(string picobCommandKey, string? picobCommandDetails, string? picobCommandDetails2) {

            string result;
            var picob = PicobResponseParso.Instance;

            switch (picobCommandKey)
            {
                case "A":
                    result = JsonConvert.SerializeObject(new Response(true, 200, true, $"SUCCESS: COMMAND SENT AND RESPONSE RECEIVED",
                        JsonConvert.SerializeObject(new { A = 1 }, Formatting.None)), Formatting.None);
                    break;
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
                case "D":
                    picob.D ^= 1;
                    result = JsonConvert.SerializeObject(new Response(true, 200, true, $"SUCCESS: COMMAND SENT AND RESPONSE RECEIVED",
                        JsonConvert.SerializeObject(new { picob.D }, Formatting.None)), Formatting.None);
                    break;
                case "M":
                    picob.M ^= 1;
                    result = JsonConvert.SerializeObject(new Response(true, 200, true, $"SUCCESS: COMMAND SENT AND RESPONSE RECEIVED",
                        JsonConvert.SerializeObject(new { picob.M }, Formatting.None)), Formatting.None);
                    break;
                case "I":
                    picob.I ^= 1;
                    result = JsonConvert.SerializeObject(new Response(true, 200, true, $"SUCCESS: COMMAND SENT AND RESPONSE RECEIVED",
                        JsonConvert.SerializeObject(new { picob.I }, Formatting.None)), Formatting.None);
                    break;
                case "K":
                    result = JsonConvert.SerializeObject(new Response(true, 200, true, $"SUCCESS: COMMAND SENT AND RESPONSE RECEIVED",
                        JsonConvert.SerializeObject(new { picob.K }, Formatting.None)), Formatting.None);
                    break;
                case "O":
                    picob.O ^= 1;
                    result = JsonConvert.SerializeObject(new Response(true, 200, true, $"SUCCESS: COMMAND SENT AND RESPONSE RECEIVED",
                        JsonConvert.SerializeObject(new { picob.O }, Formatting.None)), Formatting.None);
                    break;
                case "T":
                    result = JsonConvert.SerializeObject(new Response(true, 200, true, $"SUCCESS: COMMAND SENT AND RESPONSE RECEIVED",
                        JsonConvert.SerializeObject(new { picob.T }, Formatting.None)), Formatting.None);
                    break;
                case "C":
                    picob.C[0] ^= 1;
                    result = JsonConvert.SerializeObject(new Response(true, 200, true, $"SUCCESS: COMMAND SENT AND RESPONSE RECEIVED",
                        JsonConvert.SerializeObject(new { C = picob.C[0] }, Formatting.None)), Formatting.None);
                    break;
                case "STATUS":
                    result = JsonConvert.SerializeObject(new Response(true, 200, true, $"SUCCESS: COMMAND SENT AND RESPONSE RECEIVED",
                        JsonConvert.SerializeObject(new { connected = true, ageMs = 100, C = picob.C[0] }, Formatting.None)), Formatting.None);
                    break;
                case "W":
                    result = JsonConvert.SerializeObject(new Response(true, 200, true, $"SUCCESS: COMMAND SENT AND RESPONSE RECEIVED",
                        JsonConvert.SerializeObject(new { picob.W }, Formatting.None)), Formatting.None);
                    break;
                case "U":
                    DateTime currentTime = DateTime.ParseExact(picob.Q, "HH:mm:ss", CultureInfo.InvariantCulture);
                    DateTime openTime = DateTime.ParseExact(picob.P[0], "HH:mm:ss", CultureInfo.InvariantCulture);
                    DateTime closeTime = DateTime.ParseExact(picob.P[1], "HH:mm:ss", CultureInfo.InvariantCulture);
                    if (currentTime < openTime || currentTime > closeTime) { picob.N = 1; picob.U = 0; }
                    result = JsonConvert.SerializeObject(new Response(true, 200, true, $"SUCCESS: COMMAND SENT AND RESPONSE RECEIVED",
                        JsonConvert.SerializeObject(new { picob.U }, Formatting.None)), Formatting.None);
                    break;
                case "J":
                    result = JsonConvert.SerializeObject(new Response(true, 200, true, $"SUCCESS: COMMAND SENT AND RESPONSE RECEIVED",
                        JsonConvert.SerializeObject(new { picob.J }, Formatting.None)), Formatting.None);
                    break;
                case "L":
                    picob.N = 1;
                    picob.U = 0;
                    result = JsonConvert.SerializeObject(new Response(true, 200, true, $"SUCCESS: COMMAND SENT AND RESPONSE RECEIVED",
                        JsonConvert.SerializeObject(new { picob.N }, Formatting.None)), Formatting.None);
                    break;
                case "F":
                    picob.N = 2;
                    picob.U = 1;
                    result = JsonConvert.SerializeObject(new Response(true, 200, true, $"SUCCESS: COMMAND SENT AND RESPONSE RECEIVED",
                        JsonConvert.SerializeObject(new { picob.N }, Formatting.None)), Formatting.None);
                    break;
                case "S":
                    picob.N = 3;
                    picob.U = 0;
                    result = JsonConvert.SerializeObject(new Response(true, 200, true, $"SUCCESS: COMMAND SENT AND RESPONSE RECEIVED",
                        JsonConvert.SerializeObject(new { picob.N }, Formatting.None)), Formatting.None);
                    break;
                case "V":
                    result = JsonConvert.SerializeObject(new Response(true, 200, true, $"SUCCESS: COMMAND SENT AND RESPONSE RECEIVED",
                        JsonConvert.SerializeObject(new { picob.V }, Formatting.None)), Formatting.None);
                    break;
                case "E":
                    result = JsonConvert.SerializeObject(new Response(true, 200, true, $"SUCCESS: COMMAND SENT AND RESPONSE RECEIVED",
                        JsonConvert.SerializeObject(new { picob.E }, Formatting.None)), Formatting.None);
                    break;
                case "H":
                    result = JsonConvert.SerializeObject(new Response(true, 200, true, $"SUCCESS: COMMAND SENT"), Formatting.None);
                    break;
                case "Q":
                    result = JsonConvert.SerializeObject(new Response(true, 200, true, $"SUCCESS: COMMAND SENT AND RESPONSE RECEIVED",
                        JsonConvert.SerializeObject(new { picob.Q }, Formatting.None)), Formatting.None);
                    break;
                case "X":
                    result = JsonConvert.SerializeObject(new Response(true, 200, true, $"SUCCESS: COMMAND SENT AND RESPONSE RECEIVED",
                        JsonConvert.SerializeObject(new { picob }, Formatting.None)), Formatting.None);
                    break;


                //Special Commands
                case "Z":
                    if (picobCommandDetails == null) { result = JsonConvert.SerializeObject(new Response(false, 400, false, $"C: Z COMMAND EXPECTS ONE VALUE WITH HH:MM:SS 24 HOUR FORMAT"), Formatting.None); break; }
                    if (!picob.TrySetCustomTime(picobCommandDetails)) { result = JsonConvert.SerializeObject(new Response(false, 400, false, $"ERROR: INCORRECT TIME FORMAT. Z COMMAND EXPECTS HH:MM:SS 24 HOUR FORMAT"), Formatting.None); break; }

                    picob.Q = picob.CustomTimeString;
                    result = JsonConvert.SerializeObject(new Response(true, 200, true, $"SUCCESS: COMMAND SENT AND RESPONSE RECEIVED",
                        JsonConvert.SerializeObject(new { picob.Q }, Formatting.None)), Formatting.None);
                    break;
                case "P":

                    if (!(TimeSpan.TryParseExact(picobCommandDetails, "hh\\:mm\\:ss", CultureInfo.InvariantCulture, out TimeSpan tspan)
                        && TimeSpan.TryParseExact(picobCommandDetails2, "hh\\:mm\\:ss", CultureInfo.InvariantCulture, out TimeSpan tspan2)))
                    {
                        result = JsonConvert.SerializeObject(new Response(false, 400, false, $"ERROR: P COMMAND EXPECTS TWO VALUES WITH HH:MM:SS 24 HOUR FORMAT"), Formatting.None);
                        break;
                    }
                    picob.P = [picobCommandDetails, picobCommandDetails2];

                    result = JsonConvert.SerializeObject(new Response(true, 200, true, $"SUCCESS: COMMAND SENT AND RESPONSE RECEIVED",
                        JsonConvert.SerializeObject(new { picob.P }, Formatting.None)), Formatting.None);
                    break;

                default:
                    result = JsonConvert.SerializeObject(new Response(false, 400, false, $"ERROR: UNRECOGNIZED COMMAND"), Formatting.None);
                    break;
            }
            return result;
        }
    }
}
