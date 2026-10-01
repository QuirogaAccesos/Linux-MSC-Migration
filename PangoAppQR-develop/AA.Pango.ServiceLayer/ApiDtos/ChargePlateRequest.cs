using System.Collections.Generic;

namespace AA.Pango.ServiceLayer.ApiDtos
{
    public class ChargePlateRequest
    {
        public string InstallationID { get; set; }
        public string TerminalId { get; set; }
        public string Plate { get; set; }
        public string Phone { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string TransientID { get; set; }
        public string TemplateId { get; set; }
        public List<ApiParameter> ParamList { get; set; }
    }
}
