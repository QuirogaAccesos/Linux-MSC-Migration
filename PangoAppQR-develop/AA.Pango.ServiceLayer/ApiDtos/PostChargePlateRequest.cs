namespace AA.Pango.ServiceLayer.ApiDtos
{
    public class PostChargePlateRequest
    {
        public int InstallationId { get; set; }
        public int TerminalId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Plate { get; set; }
    }
}
