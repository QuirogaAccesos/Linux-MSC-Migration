using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AA.Pango.ServiceLayer.ApiDto
{
    public class WhiteListData
    {
        [DataContract]
        public class WhiteListRequest
        {
            [DataMember(Name = "installationID")]
            public string installationID { get; set; }

            [DataMember(Name = "terminalId")]
            public string terminalId { get; set; }

            [DataMember(Name = "serialNumber")]
            public string serialNumber { get; set; }
        }

        [DataContract]
        public class ListCredential
        {
            [DataMember(Name = "code")]
            public string code { get; set; }

            [DataMember(Name = "type")]
            public string type { get; set; }

            [DataMember(Name = "start_time")]
            public DateTime start_time { get; set; }

            [DataMember(Name = "end_time")]
            public DateTime end_time { get; set; }
        }

        [DataContract]
        public class WhiteListResponse
        {
            [DataMember(Name = "installationID")]
            public string installationID { get; set; }

            [DataMember(Name = "terminalId")]
            public string terminalId { get; set; }

            [DataMember(Name = "serialNumber")]
            public string serialNumber { get; set; }

            [DataMember(Name = "listCredentials")]
            public IList<ListCredential> listCredentials { get; set; }
        }
    }
}