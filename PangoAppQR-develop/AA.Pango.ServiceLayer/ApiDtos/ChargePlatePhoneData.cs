using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AA.Pango.ServiceLayer.ApiDtos
{
    public class ChargePlatePhoneRequest
    {
        public string installationID { get; set; }
        public string terminalId { get; set; }
        public string plate { get; set; }
        public string phone { get; set; }
    }
}
