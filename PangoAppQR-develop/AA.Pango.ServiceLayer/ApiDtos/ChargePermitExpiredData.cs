using System;

namespace AA.Pango.ServiceLayer.ApiDtos
{
    public class ChargePermitExpiredRequest
    {
        public string terminalId { get; set; }
        public int whileListId { get; set; }
        public int action { get; set; }
    }

    public class ChargePermitExpiredResponse
    {
        public string installationId { get; set; }
        public string terminalId { get; set; }
        public int status { get; set; }
        public string message { get; set; }
    }
}
