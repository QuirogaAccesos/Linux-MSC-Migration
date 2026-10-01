using AA.Pango.Model;
using System.Collections.Generic;

namespace AA.PangoApp.Payments.Interface.Dtos
{
    public class GeneratePaymentResponse
    {
        public bool TransactionApproved { get; set; }

        public int TransactionStatus { get; set; }

        public string TransactionNumber { get; set; }

        public string TransactionId { get; set; }

        public Dictionary<string, string> Data { get; set; }

        public Payment PaymentResult { get; set; }
        public string XmlData { get; set; }
        public bool HasError { get; set; }
        public string ErrorMessage { get; set; }
        public string UIErrorMessage { get; set; }
    }
}
