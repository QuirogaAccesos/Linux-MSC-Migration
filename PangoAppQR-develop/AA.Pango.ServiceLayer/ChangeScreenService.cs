using AA.Pango.Model;
using LiteDB;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AA.Pango.ServiceLayer
{
    public class ChangeScreenService
    {

        private static string _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["LiteDB"].ConnectionString;
        private static ILog Log = LogManager.GetLogger(typeof(PaymentService));
        private static string TerminalId = System.Configuration.ConfigurationManager.AppSettings["TerminalId"];

        public static void Seed()
        {
            try
            {
                using (var db = new LiteDatabase(_connectionString))
                {
                    // Get customer collection
                    var appDb = db.GetCollection<TemplateChangeScreen>("templateChangeScreen");

                    if (appDb.Count() == 0)
                    {
                        var templateScreen = new TemplateChangeScreen
                        {
                            _id = Guid.NewGuid(),
                            Date = DateTime.Now,
                            Processed = true,
                            TerminalId = "1001",
                            TemplateId = "TEST_REGISTER",
                            InstallationId = "100",
                            SecondsToShowOnScreen = 0,
                            RequiresUserResponse = false,
                        };

                        appDb.Insert(templateScreen);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        public static IEnumerable<TemplateChangeScreen> GetAllTemplateChangeScreen()
        {
            // Open database (or create if not exits)
            using (var db = new LiteDatabase(_connectionString))
            {
                // Get tickets collection
                var attemptsDb = db.GetCollection<TemplateChangeScreen>("templateChangeScreen");

                // Use Linq to query documents
                return attemptsDb.FindAll();
            }
        }

        public static IEnumerable<TemplateChangeScreen> GetPendingTemplateChangeScreen(string terminalId)
        {
            // Open database (or create if not exits)
            using (var db = new LiteDatabase(_connectionString))
            {
                // Get tickets collection
                var attemptsDb = db.GetCollection<TemplateChangeScreen>("templateChangeScreen");

                if (!string.IsNullOrEmpty(terminalId))
                {
                    return attemptsDb.Find(x => !x.Processed && x.TerminalId == terminalId);
                }

                // Use Linq to query documents
                return attemptsDb.Find(x => !x.Processed);
            }
        }

        public static bool UpsertTemplateChangeScreen(TemplateChangeScreen changeScreen)
        {
            using (var db = new LiteDatabase(_connectionString))
            {
                // Get customer collection
                var attemptsDb = db.GetCollection<TemplateChangeScreen>("templateChangeScreen");

                return attemptsDb.Upsert(changeScreen);
            }
        }

        public static bool SetProcessedChangeScreen(Guid id)
        {
            try
            {
                using (var db = new LiteDatabase(_connectionString))
                {
                    // Get customer collection
                    var attemptsDb = db.GetCollection<TemplateChangeScreen>("templateChangeScreen");

                    var row = attemptsDb.FindOne(x => x._id == id);

                    if (row == null)
                    {
                        Log.Warn("SetProcessedChangeScreen: no se encontró el changeScreen con id " + id);
                        return false;
                    }

                    row.Processed = true;

                    return attemptsDb.Upsert(row);
                }
            }
            catch (Exception ex)
            {
                Log.Error("SetProcessedChangeScreen error para id " + id, ex);
                return false;
            }
        }
    }
}
