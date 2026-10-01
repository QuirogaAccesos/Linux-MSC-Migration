using System.Runtime.Serialization;
using System.Xml.Linq;

namespace AA.PangoApp.Payments.AMP
{

    [DataContract]
    public class Payload
    {

        [DataMember(Name = "response_tid_key")]
        public string response_tid_key { get; set; }

        [DataMember(Name = "response_currency_key")]
        public string response_currency_key { get; set; }

        [DataMember(Name = "response_date_key")]
        public string response_date_key { get; set; }

        [DataMember(Name = "response_resultcode_key")]
        public string response_resultcode_key { get; set; }

        [DataMember(Name = "response_tsi_key")]
        public string response_tsi_key { get; set; }

        [DataMember(Name = "response_aid_key")]
        public string response_aid_key { get; set; }

        [DataMember(Name = "response_tvr_key")]
        public string response_tvr_key { get; set; }

        [DataMember(Name = "response_invoicenum_key")]
        public string response_invoicenum_key { get; set; }

        [DataMember(Name = "response_time_key")]
        public string response_time_key { get; set; }

        [DataMember(Name = "response_pan_key")]
        public string response_pan_key { get; set; }

        [DataMember(Name = "response_arc_key")]
        public string response_arc_key { get; set; }

        [DataMember(Name = "response_cashbkamt_key")]
        public string response_cashbkamt_key { get; set; }

        [DataMember(Name = "response_storenum_key")]
        public string response_storenum_key { get; set; }

        [DataMember(Name = "response_baseamt_key")]
        public string response_baseamt_key { get; set; }

        [DataMember(Name = "response_user_defined_echo_data")]
        public string response_user_defined_echo_data { get; set; }

        [DataMember(Name = "response_code_iso_key")]
        public string response_code_iso_key { get; set; }

        [DataMember(Name = "response_track1_key")]
        public object response_track1_key { get; set; }

        [DataMember(Name = "response_track2_key")]
        public string response_track2_key { get; set; }

        [DataMember(Name = "response_totalamt_key")]
        public string response_totalamt_key { get; set; }

        [DataMember(Name = "response_appprefname_key")]
        public string response_appprefname_key { get; set; }

        [DataMember(Name = "response_text_key")]
        public string response_text_key { get; set; }

        [DataMember(Name = "response_code_key")]
        public string response_code_key { get; set; }

        [DataMember(Name = "response_sequencenum_key")]
        public string response_sequencenum_key { get; set; }

        [DataMember(Name = "response_card_holder_name_key")]
        public string response_card_holder_name_key { get; set; }

        [DataMember(Name = "response_clerkid_key")]
        public string response_clerkid_key { get; set; }

        [DataMember(Name = "response_authcode_key")]
        public string response_authcode_key { get; set; }

        [DataMember(Name = "response_footer1_key")]
        public string response_footer1_key { get; set; }

        [DataMember(Name = "response_footer2_key")]
        public string response_footer2_key { get; set; }

        [DataMember(Name = "response_footer3_key")]
        public string response_footer3_key { get; set; }

        [DataMember(Name = "response_footer4_key")]
        public string response_footer4_key { get; set; }

        [DataMember(Name = "response_footer5_key")]
        public string response_footer5_key { get; set; }

        [DataMember(Name = "response_footer6_key")]
        public string response_footer6_key { get; set; }

        [DataMember(Name = "response_header1_key")]
        public string response_header1_key { get; set; }

        [DataMember(Name = "response_header2_key")]
        public string response_header2_key { get; set; }

        [DataMember(Name = "response_header3_key")]
        public string response_header3_key { get; set; }

        [DataMember(Name = "response_header4_key")]
        public string response_header4_key { get; set; }

        [DataMember(Name = "response_header5_key")]
        public string response_header5_key { get; set; }

        [DataMember(Name = "response_header6_key")]
        public string response_header6_key { get; set; }

        [DataMember(Name = "response_entrymode_key")]
        public string response_entrymode_key { get; set; }

        [DataMember(Name = "response_batch_key")]
        public string response_batch_key { get; set; }

        [DataMember(Name = "response_mid_key")]
        public string response_mid_key { get; set; }

        [DataMember(Name = "response_cvm_key")]
        public string response_cvm_key { get; set; }

        [DataMember(Name = "response_cardlabel_key")]
        public string response_cardlabel_key { get; set; }

        [DataMember(Name = "response_iad_key")]
        public string response_iad_key { get; set; }

        [DataMember(Name = "response_tc_key")]
        public string response_tc_key { get; set; }

        [DataMember(Name = "response_reversal_amt_key")]
        public string response_reversal_amt_key { get; set; }

        [DataMember(Name = "response_cust_serv_phone_key")]
        public string response_cust_serv_phone_key { get; set; }

        [DataMember(Name = "response_swver_key")]
        public string response_swver_key { get; set; }

        [DataMember(Name = "response_transref_key")]
        public string response_transref_key { get; set; }

        [DataMember(Name = "response_tipamt_key")]
        public string response_tipamt_key { get; set; }

        [DataMember(Name = "response_tagvalue_key")]
        public string response_tagvalue_key { get; set; }

        [DataMember(Name = "response_hosttimestamp_key")]
        public string response_hosttimestamp_key { get; set; }

        [DataMember(Name = "response_app_version_key")]
        public string response_app_version_key { get; set; }

        [DataMember(Name = "response_surchargeamt_key")]
        public string response_surchargeamt_key { get; set; }

        [DataMember(Name = "response_transname_key")]
        public string response_transname_key { get; set; }

        [DataMember(Name = "response_trace_key")]
        public object response_trace_key { get; set; }

        [DataMember(Name = "response_xmldata_key")]
        public string response_xmldata_key { get; set; }

        [DataMember(Name = "response_currency_exponent_key")]
        public string response_currency_exponent_key { get; set; }

        [DataMember(Name = "response_demo_mode_key")]
        public string response_demo_mode_key { get; set; }

        [DataMember(Name = "response_jsonrcpt_key")]
        public string response_jsonrcpt_key { get; set; }
    }

    [DataContract]
    public class AMPResponsePayment
    {
        [DataMember(Name = "ecrConnectResponseCode")]
        public string ecrConnectResponseCode { get; set; }

        [DataMember(Name = "ErrorMessage")]
        public string ErrorMessage { get; set; }

        [DataMember(Name = "EndPoint")]
        public string EndPoint { get; set; }

        [DataMember(Name = "CmdType")]
        public string CmdType { get; set; }

        [DataMember(Name = "UserDefinedEchoData")]
        public string UserDefinedEchoData { get; set; }

        [DataMember(Name = "TransactionStatus")]
        public string TransactionStatus { get; set; }

        [DataMember(Name = "Payload")]
        public Payload Payload { get; set; }
    }

}
