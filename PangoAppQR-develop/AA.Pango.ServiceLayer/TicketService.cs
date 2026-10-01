using AA.Pango.Model;
using AA.Pango.ServiceLayer;
using AA.Pango.ServiceLayer.ApiDtos;
using LiteDB;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AA.ParkingStation.Service
{
    /// <summary>
    /// </summary>
    public class TicketService
    {
        private static string _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["LiteDB"].ConnectionString;
        private static ILog Log = LogManager.GetLogger(typeof(TicketService));
        private static int MAX_EXPIRED_DAYS = int.Parse(System.Configuration.ConfigurationManager.AppSettings["MAX_EXPIRED_DAYS"]);

        public static void Seed()
        {
            using (var db = new LiteDatabase(_connectionString))
            {
                // Get tickets collection
                var appDb = db.GetCollection<AuthorizedTicket>("tickets");

                if (appDb.Count() == 0)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        var tx = new AuthorizedTicket
                        {
                            _id = i + 1,
                            EventDate = DateTime.Now,
                            Information = string.Empty,
                            IsValid = true,
                            HasBeenUsed = false,
                            TerminalId = 1,
                            CustomerIdReceived = "" + (i + 1),
                            StartDate = DateTime.UtcNow,
                            EndDate = DateTime.UtcNow.AddDays(5),
                        };

                        appDb.Insert(tx);
                    }
                }
            }
        }

        public static AuthorizedTicket GetTicket(int id)
        {
            // Open database (or create if not exits)
            using (var db = new LiteDatabase(_connectionString))
            {
                // Get tickets collection
                var tickets = db.GetCollection<AuthorizedTicket>("tickets");

                // Use Linq to query documents
                return tickets.FindOne(x => x._id == id);
            }
        }

        public static AuthorizedTicket GetTicketByCode(string code)
        {
            // Open database (or create if not exits)
            using (var db = new LiteDatabase(_connectionString))
            {
                // Get tickets collection
                var tickets = db.GetCollection<AuthorizedTicket>("tickets");

                // Use Linq to query documents
                return tickets.FindOne(x => x.CustomerIdReceived == code);
            }
        }

        public static IEnumerable<AuthorizedTicket> GetAllTickets()
        {
            // Open database (or create if not exits)
            using (var db = new LiteDatabase(_connectionString))
            {
                // Get tickets collection
                var tickets = db.GetCollection<AuthorizedTicket>("tickets");

                // Use Linq to query documents
                return tickets.FindAll().ToList();
            }
        }

        public static IEnumerable<AuthorizedTicket> GetTicketsByTerminal(int terminalIssued)
        {
            // Open database (or create if not exits)
            using (var db = new LiteDatabase(_connectionString))
            {
                // Get tickets collection
                var tickets = db.GetCollection<AuthorizedTicket>("tickets");

                // Use Linq to query documents
                return tickets.Find(x => x.TerminalId == terminalIssued).ToList();
            }
        }

        public static IEnumerable<AuthorizedTicket> GetTicketsByTerminal(string terminalNumberIssued)
        {
            var terminal = TerminalService.GetTerminal(terminalNumberIssued);

            if (terminal != null)
            {
                // Open database (or create if not exits)
                using (var db = new LiteDatabase(_connectionString))
                {
                    // Get tickets collection
                    var tickets = db.GetCollection<AuthorizedTicket>("tickets");

                    // Use Linq to query documents
                    return tickets.Find(x => x.TerminalId == terminal._id).ToList();
                }
            }

            return null;
        }

        public static IEnumerable<AuthorizedTicket> GetTicketsByDate(DateTime startDate, DateTime endDate)
        {
            IEnumerable<AuthorizedTicket> transactionsList = null;

            // Open database (or create if not exits)
            using (var db = new LiteDatabase(_connectionString))
            {
                // Get tickets collection
                var tickets = db.GetCollection<AuthorizedTicket>("tickets");

                // Use Linq to query documents
                transactionsList = tickets.Find(x => x.EventDate.CompareTo(startDate) > 0 && x.EventDate.CompareTo(endDate) < 0).ToList();
            }

            return transactionsList;
        }

        /// <summary>
        /// </summary>
        /// <param name="badgeId"></param>
        /// <param name="firstName"></param>
        /// <param name="lastName"></param>
        /// <param name="isActive"></param>
        /// <param name="nationalId"></param>
        /// <returns></returns>
        public static int InsertTicket(string receivedCustomerId, DateTime eventDate, bool approved, int terminalId, string information = "")
        {
            using (var db = new LiteDatabase(_connectionString))
            {
                // Get customer collection
                var tickets = db.GetCollection<AuthorizedTicket>("tickets");

                var trans = new AuthorizedTicket
                {
                    EventDate = eventDate,
                    IsValid = approved,
                    TerminalId = terminalId,
                    CustomerIdReceived = receivedCustomerId,
                    Information = information
                };

                var result = tickets.Insert(trans);
                return result.AsInt32;
            }
        }

        public static int InsertTicket(AuthorizedTicket ticket)
        {
            using (var db = new LiteDatabase(_connectionString))
            {
                // Get customer collection
                var tickets = db.GetCollection<AuthorizedTicket>("tickets");

                var result = tickets.Insert(ticket);
                return result.AsInt32;
            }
        }

        public static bool UpdateTicket(AuthorizedTicket ticket)
        {
            using (var db = new LiteDatabase(_connectionString))
            {
                // Get customer collection
                var tickets = db.GetCollection<AuthorizedTicket>("tickets");

                return tickets.Update(ticket);
            }
        }

        public static bool UpsertTicket(AuthorizedTicket ticket)
        {
            using (var db = new LiteDatabase(_connectionString))
            {
                // Get customer collection
                var tickets = db.GetCollection<AuthorizedTicket>("tickets");

                return tickets.Upsert(ticket);
            }
        }

        public static DeleteTicketInfo DeleteExpiredTickets()
        {
            IEnumerable<AuthorizedTicket> allTickets = GetAllTickets();
            DeleteTicketInfo info = new DeleteTicketInfo(allTickets.Count());
            using (var db = new LiteDatabase(_connectionString))
            {
                DateTime maxTimeExpired = DateTime.Now.AddDays(-1 * MAX_EXPIRED_DAYS);
                var tickets = db.GetCollection<AuthorizedTicket>("tickets");
                foreach (AuthorizedTicket ticket in allTickets)
                {
                    try
                    {
                        if (ticket.EndDate <= maxTimeExpired)
                        {
                            info.Expire();
                            tickets.Delete(ticket._id);
                            info.Delete();
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);
                        info.Error();
                    }
                }
                db.Commit();
            }
            return info;
        }
    }
}