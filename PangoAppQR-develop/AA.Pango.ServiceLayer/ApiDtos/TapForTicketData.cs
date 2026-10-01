using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AA.Pango.ServiceLayer.ApiDtos
{
    public class TapForTicketRequest
    {
        public string installationID { get; set; }
        public string terminalId { get; set; }
        public int transientId { get; set; }
        public string ticketQR { get; set; }
    }
}
