using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AA.Pango.ServiceLayer.ApiDtos
{
    public class TransientPlateOrPhoneRequest
    {
        public string installationID { get; set; }
        public string terminalId { get; set; }
        public string plate { get; set; }
        public string phone { get; set; }
        public bool ticket { get; set; }
    }

    public class TransientPlateOrPhoneResponse
    {
        public string installationId { get; set; }
        public string terminalId { get; set; }
        public int transientId { get; set; }
        public string ticketQR { get; set; }
        public int status { get; set; }
        public string message { get; set; }
    }
}
