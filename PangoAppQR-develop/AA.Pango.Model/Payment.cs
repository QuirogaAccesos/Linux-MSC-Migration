using System;

namespace AA.Pango.Model
{
    public enum PaymentStatus
    {
        Processed,
        DeclinedByBank,
        Voided,
        Failed,
        UnexpectedError,
        Other
    }

    public class Payment
    {
        public int _id { get; set; }
        public bool Success { get; set; }
        public string Msg { get; set; }
        public PaymentStatus Status { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TransactionFeeAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public string AuthNumber { get; set; }
        public string InvoiceConsecutive { get; set; }
        public DateTime PaymentDate { get; set; }

        public string TicketId { get; set; }
        public string TicketVisibleNumber { get; set; }

        public string TerminalId { get; set; }
        public string InstallationId { get; set; }


    }
}
