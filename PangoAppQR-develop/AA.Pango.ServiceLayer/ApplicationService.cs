using AA.Pango.Model;
using LiteDB;
using log4net;
using System;

namespace AA.ParkingStation.Service
{
    public static class ApplicationService
    {
        private static ILog log = LogManager.GetLogger(typeof(ApplicationService));
        private static string _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["LiteDB"].ConnectionString;

        public static void Seed(string name, bool isActive, DateTime? expirationDate)
        {
            try
            {
                using (var db = new LiteDatabase(_connectionString))
                {
                    // Get customer collection
                    var appDb = db.GetCollection<Application>("applications");

                    if (appDb.Count() == 0)
                    {
                        var app = new Application
                        {
                            Name = name,
                            IsActive = isActive,
                            LastClientsSynchronizationDate = null,
                            LicenseKey = string.Empty
                        };

                        appDb.Insert(app);
                    }
                }
            } catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        public static Application GetApplication(int id = 1)
        {
            Application app = null;

            // Open database (or create if not exits)
            using (var db = new LiteDatabase(_connectionString))
            {
                // Get customer collection
                var applicationsDb = db.GetCollection<Application>("applications");

                // Use Linq to query documents
                app = applicationsDb.FindOne(x => x._id == id);
            }

            return app;
        }

        public static bool UpdateApplication(string name, bool isActive, string license, DateTime? expirationDate, DateTime? lastSyncDate, int _id = 1)
        {
            using (var db = new LiteDatabase(_connectionString))
            {
                // Get customer collection
                var appDb = db.GetCollection<Application>("applications");

                var app = new Application
                {
                    _id = _id,
                    Name = name,
                    IsActive = isActive,
                    ExpirationDate = expirationDate,
                    LastClientsSynchronizationDate = lastSyncDate
                };

                return appDb.Update(app);
            }
        }
    }
}