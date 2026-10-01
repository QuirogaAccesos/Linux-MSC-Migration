namespace AA.Pango.ServiceLayer.ApiDtos
{


    public class ChargePlateResponse
    {
        public string installationId { get; set; }
        public string terminalId { get; set; }
        public bool plateNotReadByLPR { get; set; }
        public string templateId { get; set; }
        public string transientId { get; set; }
        public string plate { get; set; }
        public string phone { get; set; }
        public decimal? amount { get; set; }
        public ApiParameter[] paramList { get; set; }
    }


}
