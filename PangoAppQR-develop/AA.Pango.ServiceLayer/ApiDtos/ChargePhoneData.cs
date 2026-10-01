using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AA.Pango.ServiceLayer.ApiDtos
{
    public class ChargePhoneRequest
    {
        public string installationID { get; set; }
        public string terminalId { get; set; }
        public string phone { get; set; }
    }

    public class ChargePhoneResponse
    {
        public string installationId { get; set; }
        public string terminalId { get; set; }
        public bool plateNotReadByLPR { get; set; }
        public string templateId { get; set; }
        public string transientId { get; set; }
        public string phone { get; set; }
        public decimal? amount { get; set; }
        public ApiParameter[] paramList { get; set; }
    }
}
