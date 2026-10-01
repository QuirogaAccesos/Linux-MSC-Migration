using System;
using System.Runtime.Serialization;

namespace AA.Pango.ServiceLayer.ApiDto
{
    public class CheckQRCode
    {
        [DataContract]
        public class CheckQRCodeRequest
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

            [DataMember(Name = "date")]
            public DateTime date { get; set; }

            [DataMember(Name = "terminalAccess")]
            public string terminalAccess { get; set; }

            [DataMember(Name = "antiPassBack")]
            public bool antiPassBack { get; set; }
        }

        [DataContract]
        public class CheckQRCodeResponse
        {
            [DataMember(Name = "installationID")]
            public string installationID { get; set; }

            [DataMember(Name = "terminalId")]
            public string terminalId { get; set; }

            [DataMember(Name = "serialNumber")]
            public string serialNumber { get; set; }

            [DataMember(Name = "resultCode")]
            public int resultCode { get; set; }

            [DataMember(Name = "allowAccess")]
            public bool allowAccess { get; set; }

            [DataMember(Name = "description")]
            public string description { get; set; }

            [DataMember(Name = "whiteListId")]
            public int whiteListId { get; set; }
            [DataMember(Name = "amount")]
            public decimal amount { get; set; }
        }
    }
}