using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Parso.Utils;
using Parso.Utils.Objects;
using Serilog;
using System.Globalization;
using System.Reflection.Metadata;

namespace Parso.Classes.Helpers.Testing
{
    internal class TestingHelper
    {
        public string testCommand(string command, int operationNumber)
        {
            try
            {
                switch (operationNumber)
                {
                    case 1:
                        return testPrintCommand(command);
                    case 2:
                        return TestSendCommandToPicob(command);
                    case 3:
                        return testCardPaymentCommand(command);
                    default:
                        Response response = new Response(false, 400, false, $"ERROR: COMMAND NOT RECOGNIZED");

                        return JsonConvert.SerializeObject(response, Formatting.None);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);

                Response response = new Response(false, 500, false, $"FATAL ERROR: EXCEPTION ENCOUNTERED - {ex.Message}");

                return JsonConvert.SerializeObject(response, Formatting.None);
            }
        }

        public string testPrintCommand(string command)
        {
            JObject parsedObject = JObject.Parse(command).Properties().First().Value as JObject;
            string templateKey = parsedObject.Properties().First().Name;
            if (!(templateKey.Length == 1))
            {
                return JsonConvert.SerializeObject(new Response(false, 500, false, $"FATAL ERROR: EXCEPTION ENCOUNTERED - Lorem Ipsum Lorem Ipsum Lorem Ipsum Lorem Ipsum Lorem Ipsum Lorem Ipsum Lorem Ipsum Lorem Ipsum Generic Excemption Message"), Formatting.None);
            }

            if (templateKey.Length == 1 && templateKey[0] >= 'A' && templateKey[0] <= 'E')
            {
                return JsonConvert.SerializeObject(new Response(true, 200, true, $"SUCCESS: PRINTING COMMAND SENT SUCCESSFULLY"), Formatting.None);
            }
            return JsonConvert.SerializeObject(new Response(false, 400, false, $"ERROR: PRINTING TEMPLATE NOT FOUND"), Formatting.None);
        }

        public static bool IsFlatJson(string jsonString)
        {
            JObject jsonObj = JObject.Parse(jsonString);

            foreach (var property in jsonObj.Properties())
            {
                if (property.Value is JObject)
                {
                    return false;
                }
            }
            return true;
        }

        public static string TestSendCommandToPicob(string command)
        {
            ProjectConstants _projectConstants = ProjectConstants.Instance;

            ParsoTester _parsoTester = new ParsoTester();
            A1Tester _A1Tester = new A1Tester();

            string result;
            JProperty operationProperty;
            JProperty detailsProperty;
            string picobCommandKey;
            string picobCommandDetails = null;
            string picobCommandDetails2 = null;
            try
            {
                var picob = PicobResponseParso.Instance;

                operationProperty = JObject.Parse(command).Properties().First();

                if (!IsFlatJson(command))
                {

                    // Access the nested object directly
                    var nestedObject = (JObject)operationProperty.Value;
                    detailsProperty = nestedObject.Properties().First();

                    picobCommandKey = detailsProperty.Name.ToString();
                    if ((detailsProperty.Value.Count() == 2))
                    {
                        picobCommandDetails = detailsProperty.Value[0].ToString();
                        picobCommandDetails2 = detailsProperty.Value[1].ToString();
                    }
                    else {
                        picobCommandDetails = detailsProperty.Value.ToString();
                    }
                }
                else { picobCommandKey = operationProperty.Value.ToString(); }

                if (_projectConstants.CUSTOM_PROCESSOR) {
                    return _parsoTester.testParsoPicobResponses(picobCommandKey, picobCommandDetails, picobCommandDetails2);
                }
                return _A1Tester.testA1PicobResponses(picobCommandKey);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return JsonConvert.SerializeObject(new Response(false, 500, false, $"FATAL ERROR: EXCEPTION ENCOUNTERED - {ex.Message}"), Formatting.None);
            }
        }

        public string testCardPaymentCommand(string command)
        {
            var operationProperty = JObject.Parse(command).Properties().First();

            // Get amount from JSON value
            //var amountString = operationProperty.Value.ToString();

            //if (!int.TryParse(amountString, out int amount))
            //{
            //    return JsonConvert.SerializeObject(new Response(true, 400, false, $"ERROR: INVALID AMOUNT FORMAT"), Formatting.None);
            //}

            //ServerListener.Instance.BroadcastMessage("CARDINSERTED");
            //Log.Information($"Card inserted notifcation sent: CARDINSERTED");

            Thread.Sleep(2000);

            RPResponse response = new RPResponse(true, "lastoperationtest", "lasttransactionstatustest", "logtest");
            return JsonConvert.SerializeObject(new Response(true, 200, false, $"SUCCESS: TRANSACTION SUCCESSFUL BUT INCOMPLETE", JsonConvert.SerializeObject(response, Formatting.None)), Formatting.None);

            //new RPResponse(true, "lastoperationtest", "lasttransactionstatustest", "logtest");

            //return !(amountString == "12345")
            //        ? JsonConvert.SerializeObject(new Response(true, 200, true, $"SUCCESS: TRANSACTION SUCCESSFUL AND COMPLETE", JsonConvert.SerializeObject(response, Formatting.None)), Formatting.None)
            //                : JsonConvert.SerializeObject(new Response(true, 200, false, $"SUCCESS: TRANSACTION SUCCESSFUL BUT INCOMPLETE", JsonConvert.SerializeObject(response, Formatting.None)), Formatting.None);
        }

        public class testData
        {
            public string data = "Data";
        }

        public class CardPaymentTestResultSuccess
        {
            private static readonly Random _random = new Random();

            public bool success = true;
            public long transactionNumber = _random.Next(1_000_000, 10_000_000) * 10_000L + _random.Next(0, 10_000);
            public object data { get; set; } = new testData();
        }

        public class CardPaymentTestResultFailure
        {
            public bool success = false;
            public int? transactionNumber = null;
            public object? data { get; set; } = null;
        }
    }
}
