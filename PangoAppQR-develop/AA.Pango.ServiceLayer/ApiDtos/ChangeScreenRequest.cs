using System.Collections.Generic;

namespace AA.Pango.ServiceLayer.ApiDtos
{
    public class ChangeScreenRequest
    {
        public int InstallationId { get; set; }
        public string TerminalId { get; set; }
        public string TemplateId { get; set; }
        public List<ApiParameter> ParamList { get; set; }
    }

    public class ApiParameter
    {
        public string param_name { get; set; }
        public string param_value { get; set; }
        public string param_type { get; set; }
    }
}
