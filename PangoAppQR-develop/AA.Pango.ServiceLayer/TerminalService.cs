using AA.Pango.Model;
using LiteDB;
using log4net;
using System.Configuration;

namespace AA.Pango.ServiceLayer
{
    /// <summary>
    /// </summary>
    public class TerminalService
    {
        private static string _connectionString = ConfigurationManager.ConnectionStrings["LiteDB"].ConnectionString;
        private static ILog Log = LogManager.GetLogger(typeof(TerminalService));

        public static void Seed(string name = "Terminal 1")
        {
            using (var db = new LiteDatabase(_connectionString))
            {
                // Get customer collection
                var parkingDb = db.GetCollection<Terminal>("terminals");

                if (parkingDb.Count() == 0)
                {
                    var entry = new Terminal
                    {
                        Name = name,
                        IsActive = true,
                        Type = TerminalType.Entry,
                        _id = 1,
                        InstallationId = ConfigurationManager.AppSettings["InstallationId"],
                        SerialNumber = ConfigurationManager.AppSettings["Direction"] == "ENTRY" ? ConfigurationManager.AppSettings["SerialNumber"] : "1001",
                        TerminalNumber = ConfigurationManager.AppSettings["Direction"] == "ENTRY" ? ConfigurationManager.AppSettings["TerminalId"] : "1001",
                        ForceOpenBarrier = false,
                        ForceWhitelistUpload = false,
                        ShowWhitelist = false,
                    };

                    var exit = new Terminal
                    {
                        Name = name,
                        IsActive = true,
                        Type = TerminalType.Exit,
                        _id = 2,
                        InstallationId = ConfigurationManager.AppSettings["InstallationId"],
                        SerialNumber = ConfigurationManager.AppSettings["Direction"] == "EXIT" ? ConfigurationManager.AppSettings["SerialNumber"] : "1002",
                        TerminalNumber = ConfigurationManager.AppSettings["Direction"] == "EXIT" ? ConfigurationManager.AppSettings["TerminalId"] : "1002",
                        ForceOpenBarrier = false,
                        ForceWhitelistUpload = false,
                        ShowWhitelist = false,
                    };

                    parkingDb.Insert(entry);
                    parkingDb.Insert(exit);
                }
            }
        }

        /// <summary>
        /// Gets a terminal by its internal database id.
        /// </summary>
        /// <param name="terminalId"></param>
        /// <returns></returns>
        public static Terminal GetTerminal(int terminalId)
        {
            using (var db = new LiteDatabase(_connectionString))
            {
                // Get customer collection
                var terminals = db.GetCollection<Terminal>("terminals");

                // Use Linq to query parking
                return terminals.FindOne(x => x._id == terminalId);
            }
        }

        /// <summary>
        /// Gets a terminal by the terminal number
        /// </summary>
        /// <param name="terminalNumber"></param>
        /// <returns></returns>
        public static Terminal GetTerminal(string terminalNumber)
        {
            using (var db = new LiteDatabase(_connectionString))
            {
                // Get customer collection
                var terminals = db.GetCollection<Terminal>("terminals");

                // Use Linq to query parking
                return terminals.FindOne(x => x.TerminalNumber == terminalNumber);
            }
        }

        /// <summary>
        /// Gets a terminal by the terminal number
        /// </summary>
        /// <param name="terminalNumber"></param>
        /// <returns></returns>
        public static Terminal GetTerminal(string terminalNumber, string serialNumber, string installationId)
        {
            using (var db = new LiteDatabase(_connectionString))
            {
                // Get customer collection
                var terminals = db.GetCollection<Terminal>("terminals");

                // Use Linq to query parking
                return terminals.FindOne(x =>
                    x.TerminalNumber == terminalNumber &&
                    x.InstallationId == installationId &&
                    x.SerialNumber == serialNumber);
            }
        }

        public static bool UpdateTerminal(Terminal terminal)
        {
            using (var db = new LiteDatabase(_connectionString))
            {
                // Get customer collection
                var terminals = db.GetCollection<Terminal>("terminals");

                return terminals.Update(terminal);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="badgeId"></param>
        /// <returns></returns>
        public static bool IsAuthorized(string badgeId)
        {
            //Customer customer = null;

            //// Open database (or create if not exits)
            //using (var db = new LiteDatabase(_connectionString))
            //{
            //    // Get customer collection
            //    var customers = db.GetCollection<Customer>("customers");

            // // Use Linq to query customer customer = customers.FindOne(x => x.BadgeId == badgeId
            // && x.IsActive == "A");

            //}

            //if (customer != null)
            //{
            //    return true;
            //}
            //else
            //{
            //    return false;
            //}
            return false;
        }
    }
}