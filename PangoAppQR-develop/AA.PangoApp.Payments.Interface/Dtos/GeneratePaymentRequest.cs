namespace AA.PangoApp.Payments.Interface.Dtos
{
    public class GeneratePaymentRequest
    {
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public int ComPort { get; set; }
        public int Bauds { get; set; }
        public int TimeoutInSeconds { get; set; }

    }
}
