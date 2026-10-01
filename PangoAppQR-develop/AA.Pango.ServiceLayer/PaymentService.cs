using AA.Pango.Model;
using LiteDB;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AA.Pango.ServiceLayer
{
    public class PaymentService
    {
        private static string _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["LiteDB"].ConnectionString;
        private static ILog Log = LogManager.GetLogger(typeof(PaymentService));
        private static string TerminalId = System.Configuration.ConfigurationManager.AppSettings["TerminalId"];
        private static int PaymentRetryTimeout = int.Parse(System.Configuration.ConfigurationManager.AppSettings["PaymentRetryTimeout"] ?? "60000");

        public static void Seed()
        {

        }

        public static IEnumerable<PaymentAttempt> GetAllPaymentAttempts()
        {
            // Open database (or create if not exits)
            using (var db = new LiteDatabase(_connectionString))
            {
                // Get tickets collection
                var attemptsDb = db.GetCollection<PaymentAttempt>("paymentAttempts");

                // Use Linq to query documents
                return attemptsDb.FindAll();
            }
        }

        public static IEnumerable<PaymentAttempt> GetPendingPaymentAttempts(string terminalId)
        {
            // Open database (or create if not exits)
            using (var db = new LiteDatabase(_connectionString))
            {
                // Get tickets collection
                var attemptsDb = db.GetCollection<PaymentAttempt>("paymentAttempts");

                if (!string.IsNullOrEmpty(terminalId))
                {
                    return attemptsDb.Find(x => !x.Processed && x.TerminalId == terminalId);
                }

                // Use Linq to query documents
                return attemptsDb.Find(x => !x.Processed);
            }
        }

        public static IEnumerable<PaymentAttempt> GetPendingPaymentAttemptsWithinTime(string terminalId)
        {
            // Open database (or create if not exits)
            using (var db = new LiteDatabase(_connectionString))
            {
                // Get tickets collection
                var attemptsDb = db.GetCollection<PaymentAttempt>("paymentAttempts");

                if (!string.IsNullOrEmpty(terminalId))
                {
                    return attemptsDb.Find(x => !x.Processed && x.TerminalId == terminalId).ToList().
                        Where(x => (DateTime.Now - x.Date.Value).TotalMilliseconds < PaymentRetryTimeout).OrderBy(x => x.Date);
                }

                // Use Linq to query documents
                return attemptsDb.Find(x => !x.Processed).ToList().
                        Where(x => (DateTime.Now - x.Date.Value).TotalMilliseconds < PaymentRetryTimeout).OrderBy(x => x.Date);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="terminalId"></param>
        /// <param name="showValidatedOnly"></param>
        /// <returns></returns>
        public static IEnumerable<Payment> GetPayments(string installationId, string terminalId, bool showValidatedOnly)
        {

            IEnumerable<Payment> result = null;
            // Open database (or create if not exits)
            using (var db = new LiteDatabase(_connectionString))
            {
                // Get tickets collection
                var attemptsDb = db.GetCollection<Payment>("payments");

                if (!string.IsNullOrEmpty(terminalId))
                {
                    result = attemptsDb.Find(x => x.InstallationId == installationId && x.TerminalId == terminalId);
                }
                else
                {
                    result = attemptsDb.Find(x => x.InstallationId == installationId);
                }

                if (showValidatedOnly)
                {
                    result = result.Where(x => x.Success);
                }

                // Use Linq to query documents
                return result;
            }
        }

        public static PaymentAttempt GetPaymentAttemptById(Guid id)
        {
            // Open database (or create if not exits)
            using (var db = new LiteDatabase(_connectionString))
            {
                // Get templates collection
                var attemptsDb = db.GetCollection<PaymentAttempt>("paymentAttempts");

                // Use Linq to query documents
                return attemptsDb.FindOne(x => x._id == id);
            }
        }


        public static bool UpsertPaymentAttempt(PaymentAttempt paymentAttempt)
        {
            using (var db = new LiteDatabase(_connectionString))
            {
                // Get customer collection
                var attemptsDb = db.GetCollection<PaymentAttempt>("paymentAttempts");

                return attemptsDb.Upsert(paymentAttempt);
            }
        }

        public static bool UpsertPayment(Payment payment)
        {
            using (var db = new LiteDatabase(_connectionString))
            {
                // Get customer collection
                var attemptsDb = db.GetCollection<Payment>("payments");

                return attemptsDb.Upsert(payment);
            }
        }

        public static string GetPaymentReceiptNumber(string installationId, string terminalId)
        {
            var receiptNumber = string.Empty;
            using (var db = new LiteDatabase(_connectionString))
            {
                // Get customer collection
                var terminalsDb = db.GetCollection<Terminal>("terminals");

                var terminal = terminalsDb.FindOne(x => x.InstallationId == installationId && x.TerminalNumber == terminalId);

                if (terminal == null)
                {
                    return receiptNumber;
                }

                var terminalPaymentsDb = db.GetCollection<Payment>("payments");

                var terminalPayments = terminalPaymentsDb.Find(x => x.TerminalId == terminalId && x.Success);

                if (terminalPayments.Any())
                {
                    var nextId = terminalPayments.Count() + 1;
                    receiptNumber = nextId.ToString();
                }
                else
                {
                    receiptNumber = "1";
                }

                return receiptNumber;
            }
        }
    }
}
