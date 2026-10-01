using System;

namespace AA.Pango.Model
{
    public class AuthorizedTicket
    {
        public int _id { get; set; }
        public string CustomerIdReceived { get; set; }
        public DateTime EventDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsValid { get; set; }
        public bool HasBeenUsed { get; set; }
        public int TerminalId { get; set; }
        public string Information { get; set; }

    }
}