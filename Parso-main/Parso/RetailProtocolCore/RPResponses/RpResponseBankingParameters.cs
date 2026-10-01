using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CCI.Globalcom.GlobalcomRetailProtocol
{
    /// <summary>
    /// Response Class for configuration parameters request
    /// </summary>
    public class RpResponseBankingParameters
    {
        /// <summary> Dictionary to hold all the responses </summary>
        private Dictionary<byte, RpResponseBase> Parameters;
        EDeviceFamily Family;

        /// <summary> Constructor </summary>
        public RpResponseBankingParameters(EDeviceFamily eFamily)
        {
            Family = eFamily;
            Parameters = new Dictionary<byte, RpResponseBase>();
        }

        /// <summary>
        ///   Populate the Dictionary with results.
        /// </summary>
        /// <param name="parm"> Banking parameter</param>
        /// <param name="rpResponse">Result from the terminal</param>
        public void Add(byte parm, RpResponseBase rpResponse)
        {
            Parameters.Add(parm, rpResponse);
        }

 /*
  *     /// <summary> Checks to see if the banking parameter value is a single byte </summary>
        /// <param name="tag"></param>
        /// <returns> if its a byte</returns>
        private bool IsByteBankingConfigurationParmValue(EBankingParams tag)
        {
            if (EBankingParams.ExtractTimeout== tag) return true;
            if (EBankingParams.TD2_ExtractTimeout == tag) return true;
            return false;
        }
*/

        /// <summary> return all the parameters as an xml string </summary>
        /// <returns>string of all the parameters in xml</returns>
        public string PrintDetailsAsXml()
        {
            StringBuilder sb = new StringBuilder(4096000);
            sb.AppendLine("<Configuration>");

            try
            {
                foreach (KeyValuePair<byte, RpResponseBase> data in Parameters)
                {
                    RpResponseBase rpBankParamResponse = data.Value;
                    BankingParameter parm = new BankingParameter(data.Key, Family);

                    if (rpBankParamResponse.VerifyOutcome() != EOutcome.OK)
                    {
                        sb.AppendLine("  <TAG_" + parm.GetTagAsHexString() + ">" + rpBankParamResponse.Outcome.ToString() + "</TAG_" + parm.GetTagAsHexString() + ">");
                        continue;
                    }

                    sb.AppendLine(parm.AsXml(rpBankParamResponse.Info));
                }
            }
            catch (Exception exception)
            {
                sb.AppendLine("<Error Type='Exception'>" + exception.ToString() + "</Error>");
            }

            sb.AppendLine("</Configuration>");
            return sb.ToString();
        }

        /// <summary> return all the parameters as a string </summary>
        /// <returns>string of all the parameters</returns>
        public string PrintDetails()
        {
            StringBuilder sb = new StringBuilder(4096000);

            try
            {
                foreach (KeyValuePair<byte, RpResponseBase> data in Parameters)
                {
                    RpResponseBase rpBankParamResponse = data.Value;
                    BankingParameter parm = new BankingParameter(data.Key, Family);

                    if (rpBankParamResponse.VerifyOutcome() != EOutcome.OK)
                    {
                        sb.AppendLine("-->{" + parm.GetTagAsHexString() + "}" + parm.GetTagDescription() +
                                      ":" + rpBankParamResponse.VerifyOutcome());
                        continue;
                    }

                    sb.AppendLine("-->{" + parm.GetTagAsHexString() + "}" + parm.GetTagDescription() +  ":" + parm.SupportInfoString(rpBankParamResponse.Info) );

                }
            }
            catch (Exception exception)
            {
                sb.AppendLine("Exception: " + exception.ToString());
            }

            sb.AppendLine();
            return sb.ToString();
        }


        /// <summary> return some of the parameters as an xml string </summary>
        /// <returns>string of some of the parameters in xml</returns>
        public string PrintSummaryAsXml()
        {
            StringBuilder sb = new StringBuilder(4096000);
            sb.AppendLine("<Configuration>");

            try
            {
                foreach (KeyValuePair<byte, RpResponseBase> data in Parameters)
                {
                    RpResponseBase rpBankParamResponse = data.Value;
                    BankingParameter parm = new BankingParameter(data.Key, Family);

                    switch (parm.getBVParam())
                    {
                        case EBankingParamsBV.TerminalID:
                        case EBankingParamsBV.BankHostIPAddress:
                        case EBankingParamsBV.BankHostIPPort:
                        case EBankingParamsBV.BankHostConnectionProtocol:
                        case EBankingParamsBV.TerminalIPAddress:
                        case EBankingParamsBV.TerminalIPNetmask:
                        case EBankingParamsBV.TerminalIPGateway:
                        case EBankingParamsBV.Merchant:
                        case EBankingParamsBV.ExtractTimeout:
                        case EBankingParamsBV.TransactionKey:
                        case EBankingParamsBV.TerminalPeripheralConfig:
                        case EBankingParamsBV.BankHostName:
                        case EBankingParamsBV.DNS1:
                        case EBankingParamsBV.DNS2:
                        case EBankingParamsBV.SyslogPort:
                        case EBankingParamsBV.SyslogIP:
                        case EBankingParamsBV.EnableEarlyTLSConnection:
                        case EBankingParamsBV.MD5ControlEnabled:
                        case EBankingParamsBV.DeviceAddress:
                        case EBankingParamsBV.NetworkId:
                        case EBankingParamsBV.APIKey:
                        case EBankingParamsBV.APIKey2:
                        case EBankingParamsBV.AppName:
                        case EBankingParamsBV.AppVer:
                        case EBankingParamsBV.TimeOffset:
                        case EBankingParamsBV.CAPValue:
                        case EBankingParamsBV.BankHostCAID:
                        case EBankingParamsBV.BankHostCertID:
                        case EBankingParamsBV.XMLReceiptWithSWRelInfoV2:
                            break;
                        default:
                            continue;
                    }

                    if (rpBankParamResponse.VerifyOutcome() != EOutcome.OK)
                    {
                        sb.AppendLine("  <TAG_" + parm.GetTagAsHexString() + ">" + rpBankParamResponse.Outcome.ToString() + "</TAG_" + parm.GetTagAsHexString() + ">");
                        continue;
                    }

                    sb.AppendLine(parm.AsXml(rpBankParamResponse.Info));
                }
            }
            catch (Exception exception)
            {
                sb.AppendLine("<Error Type='Exception'>" + exception.ToString() + "</Error>");
            }

            sb.AppendLine("</Configuration>");
            return sb.ToString();
        }
    }
}