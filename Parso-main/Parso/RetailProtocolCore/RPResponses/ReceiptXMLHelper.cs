using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace CCI.Globalcom.GlobalcomRetailProtocol
{
    /// <summary>
    /// Class to help with the Receipt XML Object
    /// </summary>
    public class ReceiptXMLHelper
    {
        public bool IsValid;
        public XmlDocument xdoc;

        private byte[] TsiData;
        private string TsiInnerText;
        private byte[] EMVTagsTvrData;
        private string EMVTagsInnerText;
        private byte[] EMVOflineTagsTvrData;
        private string EMVOflineInnerText;

        /// <summary>
        /// Empty Object Constructor
        /// </summary>
        public ReceiptXMLHelper()
        {
            IsValid = false;
        }

        /// <summary>
        /// Load the XML string into the object
        /// </summary>
        public bool Load(string receiptXml, out string result)
        {
            result = "Success";

            if (receiptXml.Length == 0)
            {
                result = "Exception: No Data Available";
                IsValid = false;
                return IsValid;
            }

            xdoc = new XmlDocument();

            try
            {
                xdoc.LoadXml(Regex.Unescape(receiptXml).Normalize());

                XmlNode node = xdoc.SelectSingleNode("/GlobalcomReceipt/TransactionData/EMVTags/Tag[@TagID='95']");
                if (node?.InnerText.Length == 10)  //TVR is always 5 bytes - Hex encoded
                {
                    EMVTagsTvrData = new byte[5];
                    EMVTagsInnerText = node?.InnerText;

                    int b = 0;
                    for (int i = 0; i < 10; i += 2)
                    {
                        EMVTagsTvrData[b++] = byte.Parse(node?.InnerText.Substring(i, 2), System.Globalization.NumberStyles.HexNumber); ;
                    }
                }

                node = xdoc.SelectSingleNode("/GlobalcomReceipt/TransactionData/EMVTags/Tag[@TagID='9B']");
                if (node?.InnerText.Length == 4) //TSI is always 2 bytes - Hex encoded
                {
                    TsiData = new byte[2];
                    TsiInnerText = node?.InnerText;

                    TsiData[0] = byte.Parse(node?.InnerText.Substring(0, 2), System.Globalization.NumberStyles.HexNumber); ;
                    TsiData[1] = byte.Parse(node?.InnerText.Substring(2, 2), System.Globalization.NumberStyles.HexNumber); ;
                }

                node = xdoc.SelectSingleNode("/GlobalcomReceipt/TransactionData/EMVOfflineTags/Tag[@TagID='95']");
                if (node?.InnerText.Length == 10)  //TVR is always 5 bytes - Hex encoded
                {
                    EMVOflineTagsTvrData = new byte[5];
                    EMVOflineInnerText = node?.InnerText;

                    int b = 0;
                    for (int i = 0; i < 10; i += 2)
                    {
                        EMVOflineTagsTvrData[b++] = byte.Parse(node?.InnerText.Substring(i, 2), System.Globalization.NumberStyles.HexNumber); ;
                    }
                }

            }
            catch (Exception e)
            {
                result = "Exception: " + e.Message;
                IsValid = false;
                return IsValid;
            }

            IsValid = true;
            return IsValid;
        }


        public string GetSummary()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Last XML Summary:");
            try
            {
                // Grab GlobalcomReceipt
                XmlElement root = xdoc.DocumentElement;

                // Grab the TransactionData
                XmlNode transactionData = root.SelectSingleNode("TransactionData");

                string gbcCode = transactionData["Error"].InnerText;
                sb.Append("  GBCCode : " + gbcCode + Environment.NewLine);

                sb.Append("  Timestamp : " + transactionData["Date"].InnerText + " " + transactionData["Time"].InnerText + Environment.NewLine);

                string sTemp = transactionData["GBCResult"]?.InnerText;
                if (!string.IsNullOrEmpty(sTemp))
                {
                    if (sTemp.CompareTo("1") == 0)
                        sb.Append("  GBCResult : APPROVED : ");
                    else if (sTemp.CompareTo("2") == 0)
                        sb.Append("  GBCResult : PARTIAL APPROVED : ");
                    else if (sTemp.CompareTo("3") == 0)
                        sb.Append("  GBCResult : DECLINED : ");
                    else if (sTemp.CompareTo("4") == 0)
                        sb.Append("  GBCResult : APPROVED OFFLINE : ");
                    else
                        sb.Append("  GBCResult : DECLINED OFFLINE : ");
                }
                sb.Append(GetGBCTranslation(gbcCode) + Environment.NewLine);

                if (root.SelectSingleNode("Gateway") == null)
                {
                    sb.Append("  Didnt Connect to Host" + Environment.NewLine);
                    return sb.ToString();
                }

                // Is it Apriva?
                XmlNode NodeGateway = root.SelectSingleNode("Gateway")?.SelectSingleNode("AprivaPosXml");
                if (NodeGateway != null)
                {
                    sb.Append("  Apriva #");
                    XmlNode node = NodeGateway.SelectSingleNode("Credit");
                    if (node == null)
                        node = NodeGateway.SelectSingleNode("Debit");

                    if (node != null)
                    {
                        sb.Append(node["ResponseCode"].InnerText + " -> ");
                        sb.Append(node["ResponseText"].InnerText + Environment.NewLine);
                    }

                    return sb.ToString();
                }

                // Is it NBS?
                NodeGateway = root.SelectSingleNode("Gateway").SelectSingleNode("HostMessage");
                if (NodeGateway != null)
                {
                    sb.Append("  NBS -> ");
                    sb.Append(NodeGateway.InnerText + Environment.NewLine);
                    return sb.ToString();
                }

                // Is it CFN?
                NodeGateway = root.SelectSingleNode("Gateway").SelectSingleNode("CFN-Response-PreAuthRequestAcknowledgment");
                if (NodeGateway != null)
                {
                    sb.Append("  CFN #");
                    sb.Append(NodeGateway["rxDE39"]?.InnerText + " --> ");
                    
                    string sText = "Unknown";

                    switch (Convert.ToInt32(NodeGateway["rxDE39"]?.InnerText))
                    {
                        case 80: sText = "Re-prompt.  Likely Canceled"; break;
                        case 100: sText = "Do not honor"; break;
                        case 101: sText = "Expired Card"; break;
                        case 107: sText = "Refere to Card Issuer"; break;
                        case 111: sText = "No Card Record"; break;
                        case 116: sText = "No Sufficient funds"; break;
                        case 117: sText = "Bad Driver ID"; break;
                        case 119: sText = "Transaction Not Permitted"; break;
                        case 120: sText = "Site not permitted"; break;
                        case 121: sText = "Exceeds Amount LImi"; break;
                        case 123: sText = "Exceeds Frequency Limit"; break;
                        case 180: sText = "Invalid Odometer"; break;
                        case 181: sText = "Invalid Product"; break;
                        case 202: sText = "Suspected Fraud"; break;
                        case 923: sText = "Transaction in Process"; break;

                        case 902: sText = "Invalid Transaction"; break;
                        case 903: sText = "Re-enter"; break;
                        case 904: sText = "Format Error"; break;
                        case 906: sText = "Cut over in Process"; break;
                        case 909: sText = "System Malfunction"; break;
                        case 913: sText = "Dulicate Transmission"; break;
                        case 940: sText = "Host Flooded"; break;
                    }

                    sb.Append(sText + Environment.NewLine);
                    return sb.ToString();
                }

                // Is it NMI?
                NodeGateway = root.SelectSingleNode("Gateway").SelectSingleNode("Response");
                if (NodeGateway != null)
                {
                    sb.Append("  NMI #");
                    XmlNode node = NodeGateway.SelectSingleNode("Result");
                    if (node != null)
                    {
                        sb.Append(node["LocalResult"]?.InnerText + " --> " + Environment.NewLine);
                        if (node["AcquirerResponseCode"] != null)
                            sb.Append("  AcquirerResponseCode = " + node["AcquirerResponseCode"]?.InnerText + Environment.NewLine);
                        if (node["Errors"] != null)
                        {
                            XmlNode errNode = node["Errors"].SelectSingleNode("Error");
                            sb.Append("  HostError " + errNode.Attributes["code"].Value + " = " + errNode.InnerText + Environment.NewLine);
                        }
                    }
                }

                return sb.ToString() ;

            }
            catch (Exception)
            {
            }
            return sb.ToString();
        }

        /// <summary>
        /// Returns a well formatted string of the content of the object
        /// </summary>
        public string PrintDetails()
        {
            StringBuilder sb = new StringBuilder(1000);
            sb.AppendLine("Last XML:");

            if (xdoc == null)
            {
                sb.AppendLine("--> no data loaded");
                return sb.ToString();
            }

            sb.AppendLine("-->");

            using (var sw = new StringWriter())
            {
                XmlWriterSettings xmltws = new XmlWriterSettings();
                xmltws.Indent = true;
                xmltws.IndentChars = "..";

                using (var xmltw = XmlWriter.Create(sw, xmltws))
                {
                    xdoc.WriteTo(xmltw);
                    xmltw.Flush();
                    sb.Append((sw.GetStringBuilder().ToString().Replace("<", "{").Replace(">", "}")));
                }
            }

            sb.AppendLine(Environment.NewLine);
           
            return sb.ToString();
        }

        /// <summary>
        /// Returns a well formatted string of the content of the object
        /// </summary>
        public string PrintDetailsAsXml()
        {
            StringBuilder sb = new StringBuilder(1000);
            if (xdoc == null)
            {
                sb.AppendLine("<LastReceipt/>");
                return sb.ToString();
            }

            sb.AppendLine("<LastReceipt>");
            XmlNode node = xdoc.SelectSingleNode("/GlobalcomReceipt");
            sb.AppendLine(node.InnerXml);
            sb.AppendLine("</LastReceipt>");
            return sb.ToString();
        }

        /// <summary>
        /// private Method to check if a bit is set in a byte
        /// </summary>
        /// <param name="b">Byte</param>
        /// <param name="pos">Position</param>
        /// <returns></returns>
        private bool IsBitSet(byte b, int pos)
        {
            return (b & (1 << pos)) != 0;
        }

        /// <summary>
        /// private Method to turn the TVR bits into readable text
        /// </summary>
        /// <param name="data">Byte[]</param>
        private string PrintTVR(byte[] data)
        {
            StringBuilder sb = new StringBuilder(1000);
            try
            {
                //if (IsBitSet(data[0], 0)) sb.Append("RFU");
                if (IsBitSet(EMVTagsTvrData[0], 1)) sb.Append("SDA Selected." + Environment.NewLine);
                if (IsBitSet(EMVTagsTvrData[0], 2)) sb.Append("CDA Failed." + Environment.NewLine);
                if (IsBitSet(EMVTagsTvrData[0], 3)) sb.Append("DDA Failed." + Environment.NewLine);
                if (IsBitSet(EMVTagsTvrData[0], 4)) sb.Append("Card appears on terminal exception file." + Environment.NewLine);
                if (IsBitSet(EMVTagsTvrData[0], 5)) sb.Append("ICC Data Missing." + Environment.NewLine);
                if (IsBitSet(EMVTagsTvrData[0], 6)) sb.Append("SDA Failed." + Environment.NewLine);
                if (IsBitSet(EMVTagsTvrData[0], 7)) sb.Append("Offline data authentication was not performed." + Environment.NewLine);

                //if (IsBitSet(data[1], 0)) sb.Append("RFU");
                //if (IsBitSet(data[1], 1)) sb.Append("RFU");
                //if (IsBitSet(data[1], 2)) sb.Append("RFU");
                if (IsBitSet(EMVTagsTvrData[1], 3)) sb.Append("New card." + Environment.NewLine);
                if (IsBitSet(EMVTagsTvrData[1], 4)) sb.Append("Requested service not allow for card product." + Environment.NewLine);
                if (IsBitSet(EMVTagsTvrData[1], 5)) sb.Append("Application not yet effective." + Environment.NewLine);
                if (IsBitSet(EMVTagsTvrData[1], 6)) sb.Append("Expired Application." + Environment.NewLine);
                if (IsBitSet(EMVTagsTvrData[1], 7)) sb.Append("ICC and terminal have different application versions." + Environment.NewLine);

                //if (IsBitSet(data[2], 0)) sb.Append("RFU");
                //if (IsBitSet(data[2], 1)) sb.Append("RFU");
                if (IsBitSet(EMVTagsTvrData[2], 2)) sb.Append("Online PIN entered." + Environment.NewLine);
                if (IsBitSet(EMVTagsTvrData[2], 3)) sb.Append("PIN entry required, PIN pad present, but PIN was not entered." + Environment.NewLine);
                if (IsBitSet(EMVTagsTvrData[2], 4)) sb.Append("PIN entry required and PIN pad not present or not working." + Environment.NewLine);
                if (IsBitSet(EMVTagsTvrData[2], 5)) sb.Append("PIN Try Limit Exceeded." + Environment.NewLine);
                if (IsBitSet(EMVTagsTvrData[2], 6)) sb.Append("unrecognized CVM." + Environment.NewLine);
                if (IsBitSet(EMVTagsTvrData[2], 7)) sb.Append("Cardholder verification was not successful." + Environment.NewLine);

                //if (IsBitSet(data[3], 0)) sb.Append("RFU");
                //if (IsBitSet(data[3], 1)) sb.Append("RFU");
                //if (IsBitSet(data[3], 2)) sb.Append("RFU");
                if (IsBitSet(EMVTagsTvrData[3], 3)) sb.Append("Merchant forced transaction online." + Environment.NewLine);
                if (IsBitSet(EMVTagsTvrData[3], 4)) sb.Append("Transaction selected randomly for online processing." + Environment.NewLine);
                if (IsBitSet(EMVTagsTvrData[3], 5)) sb.Append("Upper consecutive offline limit exceeded." + Environment.NewLine);
                if (IsBitSet(EMVTagsTvrData[3], 6)) sb.Append("Lower consecutive offline limit exceeded." + Environment.NewLine);
                if (IsBitSet(EMVTagsTvrData[3], 7)) sb.Append("Transaction exceeds floor limit." + Environment.NewLine);

                //if (IsBitSet(data[4], 0)) sb.Append("RFU");
                //if (IsBitSet(data[4], 1)) sb.Append("RFU");
                //if (IsBitSet(data[4], 2)) sb.Append("RFU");
                //if (IsBitSet(data[4], 3)) sb.Append("RFU.");
                if (IsBitSet(EMVTagsTvrData[4], 4)) sb.Append("Script processing failed after final GENERATE AC." + Environment.NewLine);
                if (IsBitSet(EMVTagsTvrData[4], 5)) sb.Append("Script processing failed before final GENERATE AC." + Environment.NewLine);
                if (IsBitSet(EMVTagsTvrData[4], 6)) sb.Append("Issuer authentication failed." + Environment.NewLine);
                if (IsBitSet(EMVTagsTvrData[4], 7)) sb.Append("Default TDOL used." + Environment.NewLine);
            }
            catch (Exception ex)
            {
                sb.Append("Parsing Failed (" + ex.Message + ")" + Environment.NewLine);
            }

            return sb.ToString();
        }

        static public string GetGBCTranslation(string sCode)
        {
            int errorCode = Convert.ToInt32(sCode);

            switch (errorCode)
            {
                case 0: return "OK"; //TE_NO_ERROR
                case -1: return "TE_GENERAL_ERROR"; // TE_GENERAL_ERROR
                case -2: return "Communication issue talking to host"; // TE_COMMUNICATION_ERROR
                case -3: return "TE_EMV_INIT_ERROR"; //TE_EMV_INIT_ERROR				
                case -4: return "TE_NO_CARD_AVAILABLE_1"; //TE_NO_CARD_AVAILABLE_1			
                case -5: return "TE_NO_CARD_AVAILABLE_2"; //TE_NO_CARD_AVAILABLE_2 
                case -6: return "TE_NO_CARD_AVAILABLE_3"; // TE_NO_CARD_AVAILABLE_3
                case -7: return "TE_NO_CARD_AVAILABLE_4"; // TE_NO_CARD_AVAILABLE_4
                case -8: return "Swipe for the card not allowed"; // TE_UNKNOWN_TRACK
                case -9: return "Expired Card"; // TE_EXPIRED_TRACK
                case -10: return "TE_MAG_FILE_NOT_PRESENT"; // TE_MAG_FILE_NOT_PRESENT
                case -11: return "TE_MAG_FILE_ERROR_1"; // TE_MAG_FILE_ERROR_1
                case -12: return "TE_MAG_FILE_ERROR_2"; // TE_MAG_FILE_ERROR_2
                case -13: return "TE_MAG_FILE_ERROR_3"; // TE_MAG_FILE_ERROR_3
                case -14: return "TE_MAG_FILE_ERROR_4"; // TE_MAG_FILE_ERROR_4
                case -15: return "TE_MAG_FILE_ERROR_5"; // TE_MAG_FILE_ERROR_5
                case -16: return "TE_MAG_FILE_ERROR_6"; // TE_MAG_FILE_ERROR_6
                case -17: return "TE_MAG_FILE_ERROR_7"; // TE_MAG_FILE_ERROR_7
                case -18: return "TE_MAG_FILE_ERROR_8"; // TE_MAG_FILE_ERROR_8
                case -19: return "TE_REFUND_UNAVAILABLE"; // TE_REFUND_UNAVAILABLE
                case -20: return "TE_FALLBACK"; // TE_FALLBACK
                case -21: return "Abort during Interac Acct Prompt"; // TE_ABORTED
                case -22: return "TE_AUTHENTICATION"; // TE_AUTHENTICATION
                case -23: return "TE_PROCESSING_1"; // TE_PROCESSING_1
                case -24: return "Pin Entry Issue"; // TE_PROCESSING_2
                case -25: return "TE_PROCESSING_3"; // TE_PROCESSING_3
                case -26: return "TE_PROCESSING_4"; // TE_PROCESSING_4
                case -27: return "TE_PROCESSING_5"; // TE_PROCESSING_5
                case -28: return "TE_PROCESSING_6"; // TE_PROCESSING_6
                case -29: return "TE_PROCESSING_7"; // TE_PROCESSING_7
                case -30: return "TE_PROCESSING_8"; // TE_PROCESSING_8
                case -31: return "Card Pulled Early is one condition"; // TE_CARD
                case -32: return "(CFN) Host Decline see DE39 for Specifics"; // TE_NOT_ACCEPTED
                case -33: return "Card Declined Result";  // TE_REJECTED_BY_CARD
                case -34: return "TE_SERVICE_NOT_ALLOWED"; // TE_SERVICE_NOT_ALLOWED
                case -35: return "TE_KEY_SLOT_NOT_SET"; // TE_KEY_SLOT_NOT_SET
                case -36: return "TE_PINPAD_NOT_PRESENT_1"; // TE_PINPAD_NOT_PRESENT_1
                case -37: return "TE_PINPAD_NOT_PRESENT_2"; // TE_PINPAD_NOT_PRESENT_2
                case -38: return "TE_PINPAD_NOT_PRESENT_3"; // TE_PINPAD_NOT_PRESENT_3
                case -39: return "TE_ALARM_BOARD_1"; // TE_ALARM_BOARD_1
                case -40: return "TE_ALARM_BOARD_2"; // TE_ALARM_BOARD_2
                case -41: return "TE_ALARM_BOARD_3"; // TE_ALARM_BOARD_3
                case -42: return "TE_PROMPT_MESSAGE_NOT_FOUND_1"; // TE_PROMPT_MESSAGE_NOT_FOUND_1
                case -43: return "TE_PROMPT_MESSAGE_NOT_FOUND_2"; // TE_PROMPT_MESSAGE_NOT_FOUND_2
                case -44: return "TE_PROMPT_MESSAGE_NOT_FOUND_3"; // TE_PROMPT_MESSAGE_NOT_FOUND_3
                case -45: return "TE_PROMPT_GENERAL_ERROR_1"; // TE_PROMPT_GENERAL_ERROR_1
                case -46: return "TE_PROMPT_GENERAL_ERROR_2"; // TE_PROMPT_GENERAL_ERROR_2
                case -47: return "TE_PROMPT_GENERAL_ERROR_3"; // TE_PROMPT_GENERAL_ERROR_3
                case -48: return "TE_PROMPT_ABORT_BY_USER_1"; // TE_PROMPT_ABORT_BY_USER_1
                case -49: return "Abort during PIN"; // TE_PROMPT_ABORT_BY_USER_2
                case -50: return "TE_PROMPT_ABORT_BY_USER_3"; // TE_PROMPT_ABORT_BY_USER_3
                case -51: return "TE_PROMPT_ABORT_BY_USER_4"; // TE_PROMPT_ABORT_BY_USER_4
                case -52: return "TE_PROMPT_TIMEOUT_1"; // TE_PROMPT_TIMEOUT_1
                case -53: return "TE_PROMPT_TIMEOUT_2"; // TE_PROMPT_TIMEOUT_2
                case -54: return "TE_PROMPT_TIMEOUT_3"; // TE_PROMPT_TIMEOUT_3
                case -55: return "TE_PROMPT_TIMEOUT_4"; // TE_PROMPT_TIMEOUT_4
                case -56: return "TE_PIN_TRY_EXCEEDED_1"; // TE_PIN_TRY_EXCEEDED_1
                case -57: return "TE_PIN_TRY_EXCEEDED_2"; // TE_PIN_TRY_EXCEEDED_2
                case -58: return "TE_UNMATCHING_APPLICATION_1"; // TE_UNMATCHING_APPLICATION_1
                case -59: return "TE_UNMATCHING_APPLICATION_2"; // TE_UNMATCHING_APPLICATION_2
                case -60: return "TE_UNMATCHING_APPLICATION_3"; // TE_UNMATCHING_APPLICATION_3
                case -61: return "TE_UNMATCHING_APPLICATION_4"; // TE_UNMATCHING_APPLICATION_4
                case -62: return "TE_PINBLOCK_KEY_NOT_FOUND"; // TE_PINBLOCK_KEY_NOT_FOUND
                case -63: return "TE_PINBLOCK_KEY_INVALID"; // TE_PINBLOCK_KEY_INVALID
                case -64: return "Host Timeout"; // TE_HOST_RESPONSE_TIMEOUT
                case -65: return "TE_NO_TRACK_AVAILABLE"; // TE_NO_TRACK_AVAILABLE
                case -66: return "TE_TRACK_LENGTH_ERROR"; // TE_TRACK_LENGTH_ERROR
                case -67: return "TE_KEY_DATA_SLOT_NOT_SET"; // TE_KEY_DATA_SLOT_NOT_SET
                case -68: return "TE_KEY_DATA_SLOT_INVALID"; // TE_KEY_DATA_SLOT_INVALID
                case -69: return "Missing Data Encryption Key"; // TE_DATA_KEY_NOT_FOUND
                case -70: return "TE_DUKPT_CYPHER_ERROR"; // TE_DUKPT_CYPHER_ERROR
                case -71: return "TE_LENGTH_PAN_ERROR"; // TE_LENGTH_PAN_ERROR
                case -72: return "TE_FALLBACK_NOT_ENABLED"; // TE_FALLBACK_NOT_ENABLED
                case -73: return "Configuration Missing"; // TE_INVALID_ARGUMENT
                case -74: return "TE_INTERNAL_ERROR"; // TE_INTERNAL_ERROR
                case -75: return "TE_OUT_OF_MEMORY"; // TE_OUT_OF_MEMORY
                case -76: return "TE_PREAUTH_UNAVAILABLE"; // TE_PREAUTH_UNAVAILABLE
                case -77: return "TE_INVALID_RESPONSE"; // TE_INVALID_RESPONSE
                case -78: return "TE_PARSING_RESPONSE"; // TE_PARSING_RESPONSE
                case -79: return "TE_SET_STAN_ERROR"; // TE_SET_STAN_ERROR
                case -80: return "TE_SINGLE_FLOOR_LIMIT"; // TE_SINGLE_FLOOR_LIMIT
                case -81: return "TE_TOTAL_FLOOR_LIMIT"; // TE_TOTAL_FLOOR_LIMIT
                case -82: return "TE_OFFLINE_FILE_HANDLE"; // TE_OFFLINE_FILE_HANDLE
                case -83: return "TE_PAN_LENGTH_ERROR"; // TE_PAN_LENGTH_ERROR
                case -84: return "TE_AMOUNT_ERROR"; // TE_AMOUNT_ERROR
                case -85: return "TE_PROMPT_ABORT_BY_USER_5"; // TE_PROMPT_ABORT_BY_USER_5
                case -86: return "TE_WRONG_CL_CARD"; // TE_WRONG_CL_CARD
                case -87: return "TE_CARD_TIMEOUT"; // TE_CARD_TIMEOUT
                case -88: return "TE_MAG_CASH_ATM"; // TE_MAG_CASH_ATM
                case -89: return "TE_MAG_SERV_COD_ERROR"; // TE_MAG_SERV_COD_ERROR
                case -90: return "TE_ENC_MALLOC_ERROR"; // TE_ENC_MALLOC_ERROR
                case -91: return "TE_INIT_APP_PROCESS"; // TE_INIT_APP_PROCESS
                case -92: return "TE_MAG_PAN_ERROR"; // TE_MAG_PAN_ERROR
                case -93: return "TE_MAG_KSN_ERROR"; // TE_MAG_KSN_ERROR
                case -94: return "TE_CL_DATA_ERROR"; // TE_CL_DATA_ERROR
                case -95: return "TE_CL_DATA_TRACK_ERROR"; // TE_CL_DATA_TRACK_ERROR
                case -96: return "TE_CHIP_DATA_TRACK_ERROR"; // TE_CHIP_DATA_TRACK_ERROR
                case -97: return "TE_EMV_CARD_EXPIRED"; // TE_EMV_CARD_EXPIRED
                case -98: return "TE_TRACK_ERROR"; // TE_TRACK_ERROR
                case -99: return "TE_TRACK_1_ERROR"; // TE_TRACK_1_ERROR
                case -100: return "TE_P2PE_INTEGRITY_ERROR"; // TE_P2PE_INTEGRITY_ERROR
                case -101: return "TE_SCA_TRY_ANOTHER_IF"; // TE_SCA_TRY_ANOTHER_IF
                case -102: return "TE_SCA_TRY_AGAIN_CVM_LMT0"; // TE_SCA_TRY_AGAIN_CVM_LMT0
                case -103: return "TE_SCA_DECLINED"; // TE_SCA_DECLINED
                case -104: return "Could not resolve DNS"; // TE_DNS_NOT_RESOLVED
                case -105: return "TE_TRY_ANOTHER_IF_ERROR"; // TE_TRY_ANOTHER_IF_ERROR
                case -106: return "Notify Failures due to amount over original PreAuth"; // TE_INVALID_NOTIFY_AMT
                case -107: return "Petroleum Fleet Prompts";  // TE_THIRD_PROMPT_REQUESTED
                case -108: return "TE_THIRD_PROMPT_COMMUNICATION_ERROR"; // TE_THIRD_PROMPT_COMMUNICATION_ERROR
                case -109: return "Invalid Driver ID (Petroleum)"; // TE_INVALID_DRIVER_ID
                case -110: return "Invalid Terminal ID (CFN Petro)"; // TE_INVALID_TERMINAL
            };

            return "Host Error";
        }
    }
}
