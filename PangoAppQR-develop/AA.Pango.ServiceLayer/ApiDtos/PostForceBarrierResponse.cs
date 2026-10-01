namespace AA.Pango.ServiceLayer.ApiDtos
{
    public class PostForceBarrierResponse
    {
        public string InstallationId { get; set; }
        public string TerminalId { get; set; }
        public string SerialNumber { get; set; }
        public bool Success { get; set; }
        public string ResultCode { get; set; }
        public string Description { get; set; }

    }
}
