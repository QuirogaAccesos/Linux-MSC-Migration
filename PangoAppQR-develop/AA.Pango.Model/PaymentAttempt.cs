using System;

namespace AA.Pango.Model
{
    public class PaymentAttempt
    {
        public Guid _id { get; set; }
        public DateTime? Date { get; set; }
        public decimal AmountToCharge { get; set; }
        public string Currency { get; set; }
        public string Plate { get; set; }
        public string Phone { get; set; }
        public string TerminalId { get; set; }
        public bool Processed { get; set; }
        public string InstallationId { get; set; }
        public string TransientId { get; set; }
        public string ParkingFee { get; set; }
        public string TransactionFee { get; set; }
        public string TransactionPercentage { get; set; }
        public string EntryDate { get; set; }
        public string EntryTime { get; set; }
        public int? ShowTime { get; set; }
        public string PriceDetail { get; set; }
    }
}
