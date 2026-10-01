using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AA.PangoApp.Payments.AMP
{
    [DataContract]
    public class ReqPayload
    {

        [DataMember(Name = "AutoPrint")]
        public string AutoPrint { get; set; }

        [DataMember(Name = "UserDefinedEchoData")]
        public string UserDefinedEchoData { get; set; }

        [DataMember(Name = "CardEntryMethod")]
        public string CardEntryMethod { get; set; }

        [DataMember(Name = "BaseAmount")]
        public string BaseAmount { get; set; }
    }

    [DataContract]
    public class AMPRequestPayment
    {

        [DataMember(Name = "EndPoint")]
        public string EndPoint { get; set; }

        [DataMember(Name = "cmdType")]
        public string cmdType { get; set; }

        [DataMember(Name = "ReqPayload")]
        public ReqPayload ReqPayload { get; set; }
    }


}
