using System;
using System.Runtime.Serialization;

namespace AA.Pango.ServiceLayer.ApiDto
{
    [DataContract]
    public class NotificationRequest
    {
        [DataMember(Name = "installationID")]
        public string installationID { get; set; }

        [DataMember(Name = "terminalId")]
        public string terminalId { get; set; }

        [DataMember(Name = "serialNumber")]
        public string serialNumber { get; set; }

        [DataMember(Name = "code")]
        public string code { get; set; }

        [DataMember(Name = "type")]
        public string type { get; set; }

        [DataMember(Name = "accessDate")]
        public DateTime accessDate { get; set; }

        [DataMember(Name = "terminalAccess")]
        public string terminalAccess { get; set; }
    }
}