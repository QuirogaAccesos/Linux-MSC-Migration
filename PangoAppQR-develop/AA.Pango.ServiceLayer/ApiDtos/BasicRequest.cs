using System.Runtime.Serialization;

namespace AA.Pango.ServiceLayer.ApiDtos
{
    [DataContract]
    internal class BasicRequest
    {
        [DataMember(Name = "installationID")]
        public string installationID { get; set; }

        [DataMember(Name = "terminalId")]
        public string terminalId { get; set; }

        [DataMember(Name = "serialNumber")]
        public string serialNumber { get; set; }
    }
}