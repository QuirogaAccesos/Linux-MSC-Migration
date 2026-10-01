namespace AA.Pango.ServiceLayer.ApiDtos
{
    public class CheckPlateRequest
    {
        public string installationID { get; set; }
        public string terminalId { get; set; }
        public string transientId { get; set; }
        public string paymentID { get; set; }
        public decimal amount { get; set; }
        public string result { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorDescription { get; set; }
    }
}
