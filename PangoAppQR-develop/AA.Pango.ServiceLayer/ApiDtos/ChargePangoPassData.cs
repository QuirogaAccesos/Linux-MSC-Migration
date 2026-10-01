using System;

namespace AA.Pango.ServiceLayer.ApiDtos
{
    public class ChargePangoPassRequest
    {
        public string terminalId { get; set; }
        public string PangoPassCode { get; set; }
    }

    public class ChargePangoPassResponse
    {
        public string installationId { get; set; }
        public string terminalId { get; set; }
        public int status { get; set; }
        public string message { get; set; }
    }
}
