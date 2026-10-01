namespace AA.Pango.ServiceLayer.ApiDtos
{
    public class CheckPermitPlateRequest
    {
        public string installationID { get; set; }
        public string terminalId { get; set; }
        public string plate { get; set; }
    }

    public class CheckPermitPlateResponse
    {
        public string installationId { get; set; }
        public string terminalId { get; set; }
        public int resultCode { get; set; }
        public bool allowAccess { get; set; }
        public string description { get; set; }
        public int whiteListId { get; set; }
    }
}
