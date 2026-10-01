using CCI.Globalcom.GlobalcomRetailProtocol;

namespace Parso.Utils.Objects
{
    internal class PaymentRequest
    {
        public int AmountInPennies { get; set; }
        public int CurrencyCode { get; set; } = 840;
        public int EMode { get; set; } = 0x30;
        public string ClientTransactionID { get; set; }
        public int Language { get; set; } = 0x00;

        public PaymentRequest(int amountInPennies, string clientTransactionID, int currencyCode, int eMode, int language)
        {
            AmountInPennies = amountInPennies;
            CurrencyCode = currencyCode;
            EMode = eMode;
            ClientTransactionID = clientTransactionID;
            Language = language;
        }

        public PaymentRequest() { }
    }
}
