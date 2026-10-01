using AA.Pango.Model;
using LiteDB;
using log4net;
using System.Collections.Generic;
using System.Linq;

namespace AA.Pango.ServiceLayer
{
    /// <summary>
    /// 
    /// </summary>
    public class TemplateService
    {

        private static string _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["LiteDB"].ConnectionString;
        private static ILog Log = LogManager.GetLogger(typeof(TemplateService));
        private static string TerminalId = System.Configuration.ConfigurationManager.AppSettings["TerminalId"];

        public static void Seed()
        {
            using (var db = new LiteDatabase(_connectionString))
            {
                // Get tickets collection
                var appDb = db.GetCollection<Template>("templates");

                if (appDb.Count() == 0)
                {

                    // StandBy,
                    // VehiclePresent,
                    // ReadingBarcode,
                    // TicketApproved,
                    // TicketDenied,
                    // TicketNotFound,
                    // ReadingPlate,
                    // PlateValidated,
                    // PlateNotValidated,
                    // PlateNotFound,
                    // PaymentInProgress,
                    // PaymentApproved,
                    // PaymentDenied,
                    // OperationValidated,
                    // OutOfService

                    var enterPlateButtonLabel = new Template
                    {
                        Id = "VehiclePresentBtn",
                        Active = true,
                        Name = "Vehicle Present Button Label",
                        State = "VehiclePresent",
                        TerminalId = TerminalId,
                        Value = "ENTER PLATE"
                    };

                    var enterPlateLabel = new Template
                    {
                        Id = "VehiclePresent",
                        Active = true,
                        Name = "Vehicle Present Label",
                        State = "VehiclePresent",
                        TerminalId = TerminalId,
                        Value = "AMOUNT TO PAY: "
                    };

                    var ReadingBarcodeLabel = new Template
                    {
                        Id = "ReadingBarcode",
                        Active = true,
                        Name = "Reading Barcode Label",
                        State = "ReadingBarcode",
                        TerminalId = TerminalId,
                        Value = "PLEASE WAIT"
                    };

                    var TicketApprovedLabel = new Template
                    {
                        Id = "TicketApproved",
                        Active = true,
                        Name = "Ticket Approved Label",
                        State = "TicketApproved",
                        TerminalId = TerminalId,
                        Value = "TICKET APPROVED"
                    };

                    var TicketDeniedLabel = new Template
                    {
                        Id = "TicketDenied",
                        Active = true,
                        Name = "Ticket Denied Label",
                        State = "TicketDenied",
                        TerminalId = TerminalId,
                        Value = "INVALID TICKET"
                    };

                    var TicketNotFoundLabel = new Template
                    {
                        Id = "TicketNotFound",
                        Active = true,
                        Name = "TicketNotFound Label",
                        State = "TicketNotFound",
                        TerminalId = TerminalId,
                        Value = "TICKET NOT FOUND"
                    };

                    var readingPlateLabel = new Template
                    {
                        Id = "ReadingPlate",
                        Active = true,
                        Name = "ReadingPlate Label",
                        State = "ReadingPlate",
                        TerminalId = TerminalId,
                        Value = "PLEASE WAIT"
                    };

                    var plateValidatedLabel = new Template
                    {
                        Id = "PlateValidated",
                        Active = true,
                        Name = "Plate Validated Label",
                        State = "PlateValidated",
                        TerminalId = TerminalId,
                        Value = "PLATE OK"
                    };

                    var plateNotValidatedLabel = new Template
                    {
                        Id = "PlateNotValidated",
                        Active = true,
                        Name = "Plate Not Validated Label",
                        State = "PlateNotValidated",
                        TerminalId = TerminalId,
                        Value = "INVALID PLATE"
                    };

                    var paymentInProgressLabel = new Template
                    {
                        Id = "PaymentInProgress",
                        Active = true,
                        Name = "Payment In Progress Label",
                        State = "PaymentInProgress",
                        TerminalId = TerminalId,
                        Value = "PLEASE INSERT YOUR CREDIT CARD"
                    };

                    var paymentApprovedLabel = new Template
                    {
                        Id = "PaymentApproved",
                        Active = true,
                        Name = "PaymentApproved Label",
                        State = "PaymentApproved",
                        TerminalId = TerminalId,
                        Value = "Payment approved remove card – Thank you!"
                    };

                    var paymentDeniedLabel = new Template
                    {
                        Id = "PaymentDenied",
                        Active = true,
                        Name = "PaymentDenied Label",
                        State = "PaymentDenied",
                        TerminalId = TerminalId,
                        Value = "PAYMENT DECLINED"
                    };


                    var operationValidatedLabel = new Template
                    {
                        Id = "OperationValidated",
                        Active = true,
                        Name = "Operation Validated Label",
                        State = "OperationValidated",
                        TerminalId = TerminalId,
                        Value = "HAVE A NICE DAY, BYE"
                    };


                    var outOfServiceLabel = new Template
                    {
                        Id = "OutOfService",
                        Active = true,
                        Name = "Out Of Service Label",
                        State = "OutOfService",
                        TerminalId = TerminalId,
                        Value = "OUT OF SERVICE"
                    };

                    var paymentReceipt = new Template
                    {
                        Id = "TakeReceipt",
                        Active = true,
                        Name = "TakeReceipt Label",
                        State = "PaymentApproved",
                        TerminalId = TerminalId,
                        Value = "PLEASE TAKE YOUR PRINTED RECEIPT"
                    };

                    var paymentFailed = new Template
                    {
                        Id = "PaymentFailed",
                        Active = true,
                        Name = "Payment Failed",
                        State = "PaymentFailed",
                        TerminalId = TerminalId,
                        Value = "Payment Failed"
                    };

                    var enterPlateButtonLabelExit = new Template
                    {
                        Id = "AmountDueActionButtonLabel",
                        Active = true,
                        Name = "Action Button Label when Showing the Amount due",
                        State = "VehiclePresent",
                        TerminalId = TerminalId,
                        Value = "Enter Plate"
                    };

                    var paymentInProgressInsertCardLabel = new Template
                    {
                        Id = "PaymentInProgressInsertCard",
                        Active = true,
                        Name = "Payment In Progress Label",
                        State = "PaymentInProgress",
                        TerminalId = TerminalId,
                        Value = "PLEASE INSERT YOUR CREDIT CARD"
                    };

                    var paymentInProgressReadingCardLabel = new Template
                    {
                        Id = "PaymentInProgressReadingCard",
                        Active = true,
                        Name = "Payment In Progress Label",
                        State = "PaymentInProgress",
                        TerminalId = TerminalId,
                        Value = "DO NOT REMOVE CARD – PROCESSING!"
                    };

                    var paymentInProgressRemoveCardLabel = new Template
                    {
                        Id = "PaymentInProgressRemoveCard",
                        Active = true,
                        Name = "Payment In Progress Label",
                        State = "PaymentInProgress",
                        TerminalId = TerminalId,
                        Value = "PLEASE REMOVE YOUR CARD"
                    };

                    var paymentInProgressTimeoutLabel = new Template
                    {
                        Id = "PaymentInProgressTimeout",
                        Active = true,
                        Name = "aymentInProgressTimeout",
                        State = "PaymentInProgress",
                        TerminalId = TerminalId,
                        Value = "WE COULD NOT DETECT YOUR CARD."
                    };

                    appDb.Insert(enterPlateButtonLabel);
                    appDb.Insert(enterPlateButtonLabelExit);
                    appDb.Insert(outOfServiceLabel);
                    appDb.Insert(operationValidatedLabel);
                    appDb.Insert(paymentDeniedLabel);
                    appDb.Insert(paymentApprovedLabel);
                    appDb.Insert(paymentInProgressLabel);
                    appDb.Insert(plateNotValidatedLabel);
                    appDb.Insert(plateValidatedLabel);
                    appDb.Insert(readingPlateLabel);
                    appDb.Insert(TicketNotFoundLabel);
                    appDb.Insert(TicketDeniedLabel);
                    appDb.Insert(TicketApprovedLabel);
                    appDb.Insert(ReadingBarcodeLabel);
                    appDb.Insert(enterPlateLabel);
                    appDb.Insert(paymentReceipt);
                    appDb.Insert(paymentFailed);
                    appDb.Insert(paymentInProgressTimeoutLabel);
                    appDb.Insert(paymentInProgressRemoveCardLabel);
                    appDb.Insert(paymentInProgressReadingCardLabel);
                    appDb.Insert(paymentInProgressInsertCardLabel);

                }
            }
        }

        public static IEnumerable<Template> GetAllTemplates()
        {
            // Open database (or create if not exits)
            using (var db = new LiteDatabase(_connectionString))
            {
                // Get tickets collection
                var tickets = db.GetCollection<Template>("templates");

                // Use Linq to query documents
                return tickets.FindAll();
            }
        }

        public static IEnumerable<Template> GetTemplatesToShow(int terminalState, string terminalId)
        {
            // Open database (or create if not exits)
            using (var db = new LiteDatabase(_connectionString))
            {
                // Get tickets collection
                var tickets = db.GetCollection<Template>("templates");

                // Use Linq to query documents
                return tickets.Find(x => x.Show && x.State == terminalState.ToString() && x.TerminalId == terminalId).OrderBy(x => x.LastUpdatedOn);
            }
        }

        public static Template GetTemplateById(string id)
        {
            // Open database (or create if not exits)
            using (var db = new LiteDatabase(_connectionString))
            {
                // Get templates collection
                var templatesDb = db.GetCollection<Template>("templates");

                // Use Linq to query documents
                return templatesDb.FindOne(x => x.Id == id);
            }
        }

        public static IEnumerable<Template> GetTemplatesByTerminal(string terminalId)
        {
            // Open database (or create if not exits)
            using (var db = new LiteDatabase(_connectionString))
            {
                // Get templates collection
                var templatesDb = db.GetCollection<Template>("templates");

                // Use Linq to query documents
                return templatesDb.Find(x => x.TerminalId == terminalId).ToList();
            }
        }

        public static bool UpsertTemplate(Template template)
        {
            using (var db = new LiteDatabase(_connectionString))
            {
                // Get customer collection
                var templatesDb = db.GetCollection<Template>("templates");

                return templatesDb.Upsert(template);
            }
        }
    }
}
