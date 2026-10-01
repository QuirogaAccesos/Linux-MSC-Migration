using System.Collections.Generic;

namespace AA.Pango.ServiceLayer.ApiDtos
{
    public class ChargePlateResultRequest
    {
        public string installationID { get; set; }
        public string terminalId { get; set; }
        public string result { get; set; }
        public string errorCode { get; set; }
        public string errorDescription { get; set; }
        public string paymentId { get; set; }
        public decimal amount { get; set; }
        public string transientID { get; set; }
        public string transactionId { get; set; }
        public string plate { get; set; }
        public string phone { get; set; }
        public List<ApiParameter> param_list { get; set; }
    }
}
