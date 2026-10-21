namespace AA.LinuxController
{
    public class LinuxControllerResponse
    {
        public bool Success { get; set; }
        public int HTTPStatus { get; set; }
        public bool OperationResult { get; set; }
        public string Message { get; set; }
        public string DeviceResponse { get; set; }
    }
}