namespace AA.Pango.ServiceLayer.ApiDtos
{
    public class CheckCCTransientResponse
    {
        public string installationID { get; set; }
        public string terminalId { get; set; }
        public bool plateNotReadByLPR { get; set; }
        public string templateId { get; set; }
        public string transientId { get; set; }
        public string plate { get; set; }
        public decimal? amount { get; set; }
        public string ticketQR { get; set; }
        public ApiParameter[] paramList { get; set; }
    }
}
