namespace AA.Pango.Parso
{
    public class ParsoResponse
    {
        public bool Success { get; set; }
        public int HTTPStatus { get; set; }
        public bool OperationResult { get; set; }
        public string Message { get; set; }
        public string DeviceResponse { get; set; }
    }
}