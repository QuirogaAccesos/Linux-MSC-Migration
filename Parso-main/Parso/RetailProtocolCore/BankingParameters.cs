using CCI.Globalcom.GlobalcomRetailProtocol;
using System;
using System.Text;
//using static GlobalcomRetailProtocol.BankingParameter;
using System.Xml.Linq;
using System.Text.RegularExpressions;


namespace CCI.Globalcom.GlobalcomRetailProtocol
{
    /*
        *  Banking Parameter class to abstract all the differences between the two
        *  families of devices.  BV and HRT
        */
    public class BankingParameter
    {
        public EDeviceFamily deviceFamily;
        public byte tag { get; set; }

        /// <summary>
        ///   Constructor
        /// </summary>
        /// <param name="param"> </param>
        /// <param name="eFamily"></param>
        public BankingParameter(byte param, EDeviceFamily ?eFamily) 
        {
            if (eFamily == null || eFamily == EDeviceFamily.PaymentDevice)
                deviceFamily = EDeviceFamily.BV1000;
            else if (eFamily == EDeviceFamily.BV1000)
                deviceFamily = EDeviceFamily.BV1000;
            else
                deviceFamily = EDeviceFamily.HRT1000;
            tag = param; 
        }

        /// <summary>
        ///    Given a combo box and a string intialize it based on the enumerated Banking parameter.
        /// </summary>
        /// <param name="s">Value to select by default in the combo box</param>
        /// <param name="combo"></param>
        //public void InitializeComboBox(string s, DataGridViewComboBoxCell combo)
        //{
        //    Type eType = getEnumType();
        //    if (eType == typeof(ECurrencies))
        //    {
        //        combo.DataSource = Enum.GetValues(typeof(ECurrencies));
        //        combo.ValueType = typeof(ECurrencies);

        //        ECurrencies cb = ECurrencies.USD;
        //        if (!Enum.TryParse(s, true, out cb))
        //            cb = ECurrencies.USD;
        //        combo.Value = cb;
        //    }
        //    else if (eType == typeof(ETls))
        //    {
        //        combo.DataSource = Enum.GetValues(typeof(ETls));
        //        combo.ValueType = typeof(ETls);

        //        ETls cb = ETls.None;
        //        if (!Enum.TryParse(s, true, out cb))
        //            cb = ETls.None;
        //        combo.Value = cb;
        //    }
        //    else if (eType == typeof(ECertSlot))
        //    {
        //        combo.DataSource = Enum.GetValues(typeof(ECertSlot));
        //        combo.ValueType = typeof(ECertSlot);

        //        ECertSlot cb = ECertSlot.unassigned;
        //        if (!Enum.TryParse(s, true, out cb))
        //            cb = ECertSlot.unassigned;
        //        combo.Value = cb;
        //    }
        //    else if (eType == typeof(EBankHostSelection))
        //    {
        //        combo.DataSource = Enum.GetValues(typeof(EBankHostSelection));
        //        combo.ValueType = typeof(EBankHostSelection);

        //        EBankHostSelection cb = EBankHostSelection.unassigned;
        //        if (!Enum.TryParse(s, true, out cb))
        //            cb = EBankHostSelection.unassigned;
        //        combo.Value = cb;
        //    }
        //    else if (eType == typeof(EHRTGatewayType))
        //    {
        //        combo.DataSource = Enum.GetValues(typeof(EHRTGatewayType));
        //        combo.ValueType = typeof(EHRTGatewayType);

        //        EHRTGatewayType cb = EHRTGatewayType.Unknown;
        //        if (!Enum.TryParse(s, true, out cb))
        //            cb = EHRTGatewayType.Unknown;
        //        combo.Value = cb;
        //    }
        //    else if (eType == typeof(EBINFilteringApp1))
        //    {
        //        combo.DataSource = Enum.GetValues(typeof(EBINFilteringApp1));
        //        combo.ValueType = typeof(EBINFilteringApp1);

        //        EBINFilteringApp1 cb = EBINFilteringApp1.Default;
        //        if (!Enum.TryParse(s, true, out cb))
        //            cb = EBINFilteringApp1.Default;
        //        combo.Value = cb;
        //    }
        //    else if (eType == typeof(EBINFilteringApp2))
        //    {
        //        combo.DataSource = Enum.GetValues(typeof(EBINFilteringApp2));
        //        combo.ValueType = typeof(EBINFilteringApp2);

        //        EBINFilteringApp2 cb = EBINFilteringApp2.Default;
        //        if (!Enum.TryParse(s, true, out cb))
        //            cb = EBINFilteringApp2.Default;
        //        combo.Value = cb;
        //    }
        //}

        /// <summary>
        ///    Get the BV banking parameter as an enumeration
        /// </summary>
        /// <returns></returns>
        public EBankingParamsBV getBVParam()
        {
            if (deviceFamily == EDeviceFamily.HRT1000)
                return (EBankingParamsBV)0x00;
            return (EBankingParamsBV)tag;
        }

        /// <summary>
        ///  Get the HRT banking parameter as an enumeration
        /// </summary>
        /// <returns></returns>
        public EBankingParamsHRT getHRTParam()
        {
            if (deviceFamily == EDeviceFamily.HRT1000)
                return (EBankingParamsHRT)tag;
            return (EBankingParamsHRT)0x00;
        }

        /// <summary>
        ///   Based on the banking parameter turn if it is configured as a byte.
        /// </summary>
        /// <returns></returns>
        public bool IsByteBankingConfigurationParmValue()
        {
            if (deviceFamily == EDeviceFamily.BV1000)
            {
                if (EBankingParamsBV.ExtractTimeout == (EBankingParamsBV)tag) return true;
                if (EBankingParamsBV.TD2_ExtractTimeout == (EBankingParamsBV)tag) return true;
            }

            return false;
        }

        /// <summary>
        ///   the other one has assumptions its not an enumeration
        /// </summary>
        /// <returns></returns>
        public bool IsEnumeratedByte()
        {
            if (deviceFamily == EDeviceFamily.BV1000)
            {
                if (EBankingParamsBV.BINManagement == (EBankingParamsBV)tag) return true;
                if (EBankingParamsBV.TD2_FleetBinManagement == (EBankingParamsBV)tag) return true;
            }

            return false;
        }


        /// <summary>
        ///   If this banking paramter is for App1.
        /// </summary>
        /// <returns> true if it is an App 1 parameter</returns>
        public bool IsApp1Param()
        {
            if (deviceFamily == EDeviceFamily.BV1000)
            {
                if ((EBankingParamsBV)tag < EBankingParamsBV.GatewayType)
                    return true;
            }
            else
                return true;

            return false;
        }

        /// <summary>
        ///    Get the byte value of the App2 banking param
        /// </summary>
        /// <returns> Byte equivelent tag for parameter 2</returns>
        public byte GetApp2Tag()
        {
            return Convert.ToByte( Convert.ToInt32((byte)EBankingParamsBV.TD2_RFU0) + Convert.ToInt32(tag) );
        }

        /// <summary>
        ///   If this banking paramter is for App2.
        /// </summary>
        /// <returns> true if it is an App 2 parameter</returns>
        public bool IsApp2Param()
        {
            if (deviceFamily == EDeviceFamily.BV1000)
                if ((EBankingParamsBV)tag >= EBankingParamsBV.TD2_RFU0)
                    return true;

            return false;
        }

        /// <summary>
        ///   Validation of the banking parameter value for its type.
        /// </summary>
        /// <param name="data">value of the banking parameter field</param>
        /// <returns>if it is valid</returns>
        public bool IsDataValueValid(string data)
        {
            if (deviceFamily != EDeviceFamily.HRT1000)
            {
                switch ((EBankingParamsBV)tag) 
                {
                    case EBankingParamsBV.AppVer:
                    case EBankingParamsBV.TD2_AppVer:
                    case EBankingParamsBV.TimeOffset:
                    case EBankingParamsBV.TD2_TimeOffset:

                        Regex NumberCheck = new Regex(@"^[0-9]*$");
                        if (!(NumberCheck.IsMatch(data)))
                            return false;
                        break;
                    case EBankingParamsBV.CAPValue:
                    case EBankingParamsBV.TD2_CAPValue:
                        Regex AlphaNumberCheck = new Regex(@"^[a-zA-Z0-9|]*$");
                        if (!(AlphaNumberCheck.IsMatch(data)))
                            return false;
                        break;
                    case EBankingParamsBV.BankHostName:
                    case EBankingParamsBV.BankHostName2:
                    case EBankingParamsBV.TD2_BankHostName:
                    case EBankingParamsBV.TD2_BankHostName2:
                        // Limit on the field size
                        if (data.Length > 64)
                            return false;
                        
                        // URL is valid
                        Uri uriResult;

                        bool result = Uri.IsWellFormedUriString(data, UriKind.RelativeOrAbsolute);

                        if (result) return result;

                        // Or Just Hostname
                        if (Uri.CheckHostName(data) == UriHostNameType.Unknown)
                           return false;
                        break;
                    default:
                        break;
                }
            }

            return true;
        }

        /// <summary>
        ///   Get the banking parameter tag as a string.  e.g. 01 or 0D
        /// </summary>
        /// <returns></returns>
        public string GetTagAsHexString()
        {
            return tag.ToString("X2");
        }

        /// <summary>
        ///  Get the banking parameter description as a string.  e.g. 01 would return TerminalID
        /// </summary>
        /// <returns></returns>
        public string GetTagDescription()
        {
            if (deviceFamily == EDeviceFamily.HRT1000)
            {
                EBankingParamsHRT b = (EBankingParamsHRT)tag;
                return b.ToString();
            }
            else
            {
                EBankingParamsBV b = (EBankingParamsBV)tag;
                return b.ToString();
            }
        }


        /// <summary>
        ///    Given a string object convert the value to byte array
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public byte[] ConvertValueToByteArray(object info)
        {
            byte[] result = new byte[1];

            if (IsByteBankingConfigurationParmValue()) //never an enumerated type
            {
                result[0] = (info == null ||
                            String.IsNullOrEmpty(info.ToString()))
                    ? (byte)0x00
                    : (byte.Parse(info.ToString()));
            }
            else
            {
                if (deviceFamily == EDeviceFamily.HRT1000)
                {
                    switch (getEnumType())
                    {
                        case Type testType when (testType == typeof(EHRTGatewayType)):
                            string tls = info.ToString();
                            EHRTGatewayType tlsValue;
                            if (EHRTGatewayType.TryParse(tls, true, out tlsValue))
                            {
                                result[0] = (info == null ||
                                            String.IsNullOrEmpty(info.ToString()))
                                    ? (byte)EHRTGatewayType.Unknown
                                    : (byte)tlsValue;
                            }
                            else
                            {
                                result[0] = (byte)EHRTGatewayType.Unknown;
                            }

                            break;

                        default:
                            result = (info == null ||
                                String.IsNullOrEmpty(info.ToString()))
                                ? null
                                : Encoding.ASCII.GetBytes(info.ToString());
                            break;
                    };

                }
                else
                {
                    switch (getEnumType())
                    {
                        case Type testType when (testType == typeof(ETls)):
                            string tls = info.ToString();
                            ETls tlsValue;
                            if (ETls.TryParse(tls, true, out tlsValue))
                            {
                                result[0] = (info == null ||
                                            String.IsNullOrEmpty(info.ToString()))
                                    ? (byte)ETls.TLSNoAuth
                                    : (byte)tlsValue;
                            }
                            else
                            {
                                result[0] = (byte)ETls.TLSNoAuth;
                            }

                            break;
                        case Type testType when (testType == typeof(ECertSlot)):
                            string certSlot = info.ToString();
                            ECertSlot certSlotValue;
                            if (ECertSlot.TryParse(certSlot, true, out certSlotValue))
                            {
                                if (certSlotValue == 0x00)
                                {
                                    result = null;
                                }
                                else
                                {
                                    result[0] = (info == null ||
                                                String.IsNullOrEmpty(info.ToString()))
                                        ? (byte)ECertSlot.unassigned
                                        : (byte)certSlotValue;
                                }
                            }
                            else
                            {
                                result = null; // (byte) ECertSlot.unassigned;
                            }


                            break;
                        case Type testType when (testType == typeof(EBankHostSelection)):
                            string bankHostSelectionString = info.ToString();
                            EBankHostSelection bankHostSelection;
                            if (EBankHostSelection.TryParse(bankHostSelectionString, true, out bankHostSelection))
                            {
                                if (bankHostSelection == 0x00)
                                {
                                    result = null;
                                }
                                else
                                {
                                    result[0] = (info == null ||
                                                String.IsNullOrEmpty(info.ToString()))
                                        ? (byte)EBankHostSelection.unassigned
                                        : (byte)bankHostSelection;
                                }
                            }
                            else
                            {
                                result = null; //(byte) EBankHostSelection.unassigned;
                            }


                            break;

                        case Type testType when (testType == typeof(EBINFilteringApp1)):
                            string binString = info.ToString();
                            EBINFilteringApp1 binSelection;
                            if (EBINFilteringApp1.TryParse(binString, true, out binSelection))
                            {
                                result[0] = (info == null ||
                                            String.IsNullOrEmpty(info.ToString()))
                                    ? (byte)EBINFilteringApp1.Default
                                    : (byte)binSelection;
                            }
                            else
                            {
                                result = null; 
                            }


                            break;

                        case Type testType when (testType == typeof(EBINFilteringApp2)):
                            string binString2 = info.ToString();
                            EBINFilteringApp2 binSelection2;
                            if (EBINFilteringApp2.TryParse(binString2, true, out binSelection2))
                            {
                                result[0] = (info == null ||
                                            String.IsNullOrEmpty(info.ToString()))
                                    ? (byte)EBINFilteringApp2.Default
                                    : (byte)binSelection2;
                            }
                            else
                            {
                                result = null;
                            }


                            break;

                        default:
                            result = (info == null ||
                                     String.IsNullOrEmpty(info.ToString()))
                                ? null
                                : Encoding.ASCII.GetBytes(info.ToString());

                            break;
                    }
                }
                }

            return result;
        }

        /// <summary>
        ///   Get the type of banking parameter as a generic enumeration.
        /// </summary>
        /// <returns></returns>
        public Type getEnumType()
        {
            if (deviceFamily == EDeviceFamily.HRT1000)
            {
                switch ((EBankingParamsHRT)tag)
                {
                    case EBankingParamsHRT.BankHostConnectionProtocol:
                        return typeof(ETls);
                    case EBankingParamsHRT.GatewayType:
                        return typeof(EHRTGatewayType);
                }
            }

            else
            {
                switch ((EBankingParamsBV)tag)
                {
                    case EBankingParamsBV.Default_Currency:
                        return typeof(ECurrencies);
                    case EBankingParamsBV.BankHostConnectionProtocol: //fallthrough is desired.
                    case EBankingParamsBV.BankHostConnectionProtocol2:
                    case EBankingParamsBV.TD2_BankHostConnectionProtocol:
                    case EBankingParamsBV.TD2_BankHostConnectionProtocol2:
                        return typeof(ETls);
                    case EBankingParamsBV.BankHostCertID2: // Fallthrough is desired.
                    case EBankingParamsBV.BankHostCAID2:
                    case EBankingParamsBV.BankHostCAID:
                    case EBankingParamsBV.BankHostCertID:
                    case EBankingParamsBV.TD2_BankHostCertID2:
                    case EBankingParamsBV.TD2_BankHostCAID2:
                    case EBankingParamsBV.TD2_BankHostCAID:
                    case EBankingParamsBV.TD2_BankHostCertID:
                        return typeof(ECertSlot);
                    case EBankingParamsBV.BankHostSelection:
                    case EBankingParamsBV.TD2_BankHostSelection:
                        return typeof(EBankHostSelection);
                    case EBankingParamsBV.BINManagement:
                        return typeof(EBINFilteringApp1);
                    case EBankingParamsBV.TD2_FleetBinManagement:
                        return typeof(EBINFilteringApp2);
                }
            }

            return null;
        }

        /// <summary>
        ///   Create an xml string out of the tag and the value passed in
        /// </summary>
        /// <param name="Info"></param>
        /// <returns></returns>
        public string AsXml(byte[] Info)
        {
            StringBuilder sb = new StringBuilder();

            if (deviceFamily == EDeviceFamily.HRT1000)
            {
                switch ((EBankingParamsHRT)tag)
                {
                    case EBankingParamsHRT.BankHostConnectionProtocol:
                        ETls tlsValue;

                        string tls = "0";
                        if (Info.Length > 1)
                        {
                            tls = Encoding.ASCII.GetString(Info, 0, 1);

                            if (Enum.TryParse<ETls>(tls, true, out tlsValue))
                            {
                                sb.AppendLine("  <" + GetTagDescription() + ">" + (ETls)(Info[0]) + "</" + GetTagDescription() + ">");
                            }
                        }
                        break;
                    case EBankingParamsHRT.GatewayType:
                        EHRTGatewayType gwValue;

                        string gw = "0";
                        if (Info.Length > 1)
                        {
                            tls = Encoding.ASCII.GetString(Info, 0, 1);

                            if (Enum.TryParse<EHRTGatewayType>(gw, true, out gwValue))
                            {
                                sb.AppendLine("  <" + GetTagDescription() + ">" + (EHRTGatewayType)(Info[0]) + "</" + GetTagDescription() + ">");
                            }
                        }
                        break;
                    default:
                        string sValue = (Encoding.ASCII.GetString(Info, 0, (Info.Length)) ?? "{Null}");
                        if (sValue.Length == 1 && (sValue[0] < 32 || sValue[0] > 126))
                            sb.AppendLine("  <" + GetTagDescription() + ">" + (int)(Info[0]) + "</" + GetTagDescription() + ">");
                        else
                        {
                            // if they went crazy setting values this will escape the xml
                            XElement node = new XElement(GetTagDescription(), sValue);
                            sb.AppendLine(node.ToString());
                        }
                        break;
                }
            }

            else
            {
                switch ((EBankingParamsBV)tag)
                {
                    case EBankingParamsBV.Default_Currency:

                        ECurrencies curValue;

                        string curr = "0";
                        if (Info.Length > 3)
                        {
                            curr = Encoding.ASCII.GetString(Info, 0, 3);

                            if (Enum.TryParse<ECurrencies>(curr, true, out curValue))
                            {
                                sb.AppendLine("  <" + GetTagDescription() + ">" + curr + "</" + GetTagDescription() + ">");
                            }
                        }

                        break;
                    case EBankingParamsBV.BankHostConnectionProtocol: //fallthrough is desired.
                    case EBankingParamsBV.BankHostConnectionProtocol2:
                    case EBankingParamsBV.TD2_BankHostConnectionProtocol:
                    case EBankingParamsBV.TD2_BankHostConnectionProtocol2:

                        ETls tlsValue;

                        string tls = "0";
                        if (Info.Length > 1)
                        {
                            tls = Encoding.ASCII.GetString(Info, 0, 1);

                            if (Enum.TryParse<ETls>(tls, true, out tlsValue))
                            {
                                sb.AppendLine("  <" + GetTagDescription() + ">" + (ETls)(Info[0]) + "</" + GetTagDescription() + ">");
                            }
                        }

                        break;
                    case EBankingParamsBV.BankHostCertID2: // Fallthrough is desired.
                    case EBankingParamsBV.BankHostCAID2:
                    case EBankingParamsBV.BankHostCAID:
                    case EBankingParamsBV.BankHostCertID:
                    case EBankingParamsBV.TD2_BankHostCertID2:
                    case EBankingParamsBV.TD2_BankHostCAID2:
                    case EBankingParamsBV.TD2_BankHostCAID:
                    case EBankingParamsBV.TD2_BankHostCertID:


                        if (Info.Length > 1)
                        {
                            string slot = Encoding.ASCII.GetString(Info, 0, 1);

                            ECertSlot certSlot;
                            if (Enum.TryParse(slot, true, out certSlot))
                            {
                                sb.AppendLine("  <" + GetTagDescription() + ">" + (ECertSlot)(Info[0]) + "</" + GetTagDescription() + ">");
                            }
                        }


                        break;
                    case EBankingParamsBV.BankHostSelection:
                    case EBankingParamsBV.TD2_BankHostSelection:

                        if (Info.Length > 1)
                        {
                            string bankHostSelectonString =
                                Encoding.ASCII.GetString(Info, 0, 1);

                            EBankHostSelection bankHostSelection;
                            if (Enum.TryParse(bankHostSelectonString, true, out bankHostSelection))
                            {
                                sb.AppendLine("  <" + GetTagDescription() + ">" + (EBankHostSelection)(Info[0]) + "</" + GetTagDescription() + ">");
                            }
                        }

                        break;
                    case EBankingParamsBV.BINManagement:
                        {
                            if (Info.Length > 1)
                            {
                                string binSelectonString =
                                    Encoding.ASCII.GetString(Info, 0, 1);

                                EBINFilteringApp1 binSelection;
                                if (Enum.TryParse(binSelectonString, true, out binSelection))
                                {
                                    sb.AppendLine("  <" + GetTagDescription() + ">" + (EBINFilteringApp1)(Info[0]) + "</" + GetTagDescription() + ">");
                                }
                            }

                            break;
                        }
                    case EBankingParamsBV.TD2_FleetBinManagement:
                        {
                            if (Info.Length > 1)
                            {
                                string binSelectonString =
                                    Encoding.ASCII.GetString(Info, 0, 1);

                                EBINFilteringApp2 binSelection;
                                if (Enum.TryParse(binSelectonString, true, out binSelection))
                                {
                                    sb.AppendLine("  <" + GetTagDescription() + ">" + (EBINFilteringApp2)(Info[0]) + "</" + GetTagDescription() + ">");
                                }
                            }

                            break;
                        }
                    default:
                        if (IsByteBankingConfigurationParmValue())
                        {
                            if (Info.Length > 0)  // if there is data
                                sb.AppendLine("  <" + GetTagDescription() + ">" + (int)(Info[0]) + "</" + GetTagDescription() + ">");
                            else
                                sb.AppendLine("  <" + GetTagDescription() + "/>");
                        }
                        else
                        {
                            string sValue = (Encoding.ASCII.GetString(Info, 0, (Info.Length)) ?? "{Null}");
                            if (sValue.Length == 1 && (sValue[0] < 32 || sValue[0] > 126))
                                sb.AppendLine("  <" + GetTagDescription() + ">" + (int)(Info[0]) + "</" + GetTagDescription() + ">");
                            else
                            {
                                // if they went crazy setting values this will escape the xml
                                XElement node = new XElement(GetTagDescription(), sValue);
                                sb.AppendLine(node.ToString());
                            }
                        }

                        break;
                }
            }

            return sb.ToString();
        }

        /// <summary>
        ///    Given a byte array of data convert to a printable string.
        /// </summary>
        /// <param name="Info"></param>
        /// <returns></returns>
        public string SupportInfoString(byte[] Info)
        {
            string sValue = ConvertValueToString(Info);

            if (deviceFamily != EDeviceFamily.HRT1000)
                return sValue;

            int extra = 1;
            switch ((EBankingParamsHRT)tag)
            {
                case EBankingParamsHRT.Fallback:
                case EBankingParamsHRT.FallbackOnNoChipAID:
                    if (Info[0] == 0x30)
                        sValue += " (Off)";
                    else
                        sValue += " (On)";
                    break;
                case EBankingParamsHRT.CardholderAuthentication:
                    byte bAuth = Convert.ToByte(Encoding.ASCII.GetString(Info, 0, Info.Length - extra), 16);

                    // "Cardholder Authentication - Bit mask",
                    if (bAuth != 0x00)
                    {
                        sValue += " (";
                        if ((bAuth & (1 << 6)) != 0)
                            sValue += ("Signature, ");
                        if ((bAuth & (1 << 5)) != 0)
                            sValue += ("Online PIN, ");
                        if ((bAuth & (1 << 4)) != 0)
                            sValue += ("Pin Retry, ");
                        if ((bAuth & (1 << 3)) != 0)
                            sValue += ("Offline PIN, ");
                        if ((bAuth & (1 << 2)) != 0)
                            sValue += ("PIN By Pass, ");
                        if ((bAuth & (1 << 1)) != 0)
                            sValue += ("CDCVM, ");
                        sValue = sValue.Substring(0, sValue.Length - 2) + ")";
                    }
                    break;

                default:
                    sValue = Encoding.ASCII.GetString(Info, 0, Info.Length - extra);
                    break;
            }

            return sValue;
        }

        /// <summary>
        ///    Given a byte array of data convert to a printable string.
        /// </summary>
        /// <param name="Info"></param>
        /// <returns></returns>
        public string ConvertValueToString(byte[] Info)
        {
            string sValue = null;
            try
            {
                if (deviceFamily == EDeviceFamily.HRT1000)
                {
                    int extra = 1;
                    switch ((EBankingParamsHRT)tag)
                    {
                        case EBankingParamsHRT.BankHostConnectionProtocol:
                            if (Info.Length >= 1)
                                sValue = ((ETls)Info[0]).ToString();
                            break;
                        default:
                            sValue = Encoding.ASCII.GetString(Info, 0, Info.Length - extra);
                            break;
                    }
                }

                else
                {
                    switch ((EBankingParamsBV)tag)
                    {
                        case EBankingParamsBV.Default_Currency:

                            if (Info.Length >= 3)
                            {
                                sValue = Encoding.ASCII.GetString(Info, 0, Info.Length);
                            }


                            break;
                        case EBankingParamsBV.BankHostConnectionProtocol: //fallthrough is desired.
                        case EBankingParamsBV.BankHostConnectionProtocol2:
                        case EBankingParamsBV.TD2_BankHostConnectionProtocol:
                        case EBankingParamsBV.TD2_BankHostConnectionProtocol2:

                            if (Info.Length >= 1)
                            {
                                sValue = ((ETls)Info[0]).ToString();
                            }


                            break;
                        case EBankingParamsBV.BankHostCertID2: // Fallthrough is desired.
                        case EBankingParamsBV.BankHostCAID2:
                        case EBankingParamsBV.BankHostCAID:
                        case EBankingParamsBV.BankHostCertID:
                        case EBankingParamsBV.TD2_BankHostCertID2:
                        case EBankingParamsBV.TD2_BankHostCAID2:
                        case EBankingParamsBV.TD2_BankHostCAID:
                        case EBankingParamsBV.TD2_BankHostCertID:

                            if (Info.Length >= 1)
                            {
                                sValue = ((ECertSlot)Info[0]).ToString();
                            }

                            break;
                        case EBankingParamsBV.BankHostSelection:
                        case EBankingParamsBV.TD2_BankHostSelection:


                            if (Info.Length >= 1)
                            {
                                sValue = ((EBankHostSelection)Info[0]).ToString();
                            }


                            break;
                        case EBankingParamsBV.BINManagement:
                            if (Info.Length >= 1)
                                sValue = ((EBINFilteringApp1)Info[0]).ToString();
                            break;

                        case EBankingParamsBV.TD2_FleetBinManagement:
                            if (Info.Length >= 1)
                                sValue = ((EBINFilteringApp2)Info[0]).ToString();
                            break;

                        default:
                            if (IsByteBankingConfigurationParmValue())
                            {
                                if (Info.Length > 0)
                                    sValue = ((int)(Info[0])).ToString();
                                else
                                    sValue = "";
                            }
                            else
                            {
                                sValue = Encoding.ASCII.GetString(Info, 0, Info.Length);
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
            }

            return sValue;
        }
    }

    /// <summary>
    ///   HRT Banking Parameter Header Gateway Type
    /// </summary>
    public enum EHRTGatewayType
    {
        Unknown = 0,
        NMI = 1,
        Apriva = 2,
        Other = 3
    }

    /// <summary>
    ///   HRT Banking Parameter Header fields
    /// </summary>
    public enum EBankingParamsHRTHeaders
    {
        Type,
        Author,
        Date,
        Version
    }

    /// <summary>
    ///   Available Banking Parameters for HRT
    /// </summary>
    public enum EBankingParamsHRT
    {
        TerminalID = 0x01,
        BankHostIPAddress = 0x02,
        BankHostIPPort = 0x03,
        BankHostConnectionProtocol = 0x04,
        Merchant = 0x08,
        TransactionKey = 0x0D,
        BankHostName = 0x11,
        DeviceAddress = 0x1D,
        NetworkId = 0x1E,
        APIKey = 0x1F,

        AppName = 0x21,
        AppVer = 0x22,

        FallbackOnNoChipAID = 0xD9,
        Fallback = 0xDC,

        GatewayType = 0x50,
        CardholderAuthentication = 0x51,
        MerchantNameLocation = 0x52,
        XMLReceiptWithSWRelInfo = 0x53,
        Default_Currency = 0x54,

        TMS_URL = 0x57,
        TMS_Port = 0x58,
        Log_Level = 0x59
    }

    /// <summary>
    ///   BIN range configurations
    /// </summary>
    public enum EBINFilteringApp2
    {
        Default = 0x00,
        App2CFNOFF = 0x01,
        App2FleetcorOnly = 0x02,
        App2CFNFullFleetRange = 0x03
    }

    public enum EBINFilteringApp1
    {
        Default = 0x00
    }


    /// <summary>
    /// Enumerations of the banking parameter configuration tags
    /// </summary>
    public enum EBankingParamsBV
    {
        TerminalID = 0x01,
        BankHostIPAddress = 0x02,
        BankHostIPPort = 0x03,
        BankHostConnectionProtocol = 0x04,
        TerminalIPAddress = 0x05,
        TerminalIPNetmask = 0x06,
        TerminalIPGateway = 0x07,
        Merchant = 0x08,
        ExtractTimeout = 0x09,
        FloorLimitAll = 0x10,
        GPRSAPN = 0x0A,
        GPRSLogin = 0x0B,
        GPRSPassword = 0x0C,
        TransactionKey = 0x0D,
        TerminalPeripheralConfig = 0x0E,
        FloorLimitOne = 0x0F,
        BankHostName = 0x11,
        DNS1 = 0x12,
        DNS2 = 0x13,
        // UNDEFINED14                 = 0x14,  Private not to be used but reserved.
        HostStatusCheckPort = 0x15,
        HostStatusCheckFrequency = 0x16,
        SyslogPort = 0x17,
        SyslogIP = 0x18,
        DisableFullDataOutcome = 0x19,
        EnableEarlyTLSConnection = 0x1A,
        QuickChipEnable = 0x1B,
        MD5ControlEnabled = 0x1C,
        DeviceAddress = 0x1D,
        NetworkId = 0x1E,
        APIKey = 0x1F,
        APIKey2 = 0x20,
        AppName = 0x21,
        AppVer = 0x22,
        TimeOffset = 0x23,
        CAPValue = 0x24,
        BankHostCAID = 0x25,
        BankHostCertID = 0x26,
        BankHostName2 = 0x27,
        // UNDEFINED28                 = 0x28,  Private not to be used but reserved
        BankHostIPAddress2 = 0x29,
        BankHostIPPort2 = 0x2A,
        BankHostConnectionProtocol2 = 0x2B,
        BankHostCAID2 = 0x2C,
        BankHostCertID2 = 0x2D,
        BankHostSelection = 0x2E,
        XMLReceiptWithSWRelInfoV2 = 0x2F,
        LineOffsetDisplay = 0x30,
        BINManagement = 0x31,
        ProcessorFlag = 0x32,
        Syslog_Child_CL = 0x33,
        TD1_RFU_4 = 0x34,
        TD1_RFU_5 = 0x35,
        TD1_RFU_6 = 0x36,
        TD1_RFU_7 = 0x37,
        TD1_RFU_8 = 0x38,
        TD1_RFU_9 = 0x39,
        TD1_RFU_10 = 0x3A,
        TD1_RFU_11 = 0x3B,
        TD1_RFU_12 = 0x3C,
        TD1_RFU_13 = 0x3D,
        TD1_RFU_14 = 0x3E,
        TD1_RFU_15 = 0x3F,
        TD1_RFU_16 = 0x40,
        TD1_RFU_17 = 0x41,
        TD1_RFU_18 = 0x42,
        TD1_RFU_19 = 0x43,
        TD1_RFU_20 = 0x44,
        TD1_RFU_21 = 0x45,
        TD1_RFU_22 = 0x46,
        TD1_RFU_23 = 0x47,
        TD1_RFU_24 = 0x48,
        TD1_RFU_25 = 0x49,
        TD1_RFU_26 = 0x4A,
        TD1_RFU_27 = 0x4B,
        TD1_RFU_28 = 0x4C,
        TD1_RFU_29 = 0x4D,
        TD1_RFU_30 = 0x4E,
        TD1_RFU_31 = 0x4F,

        // Special GTV
        GatewayType = 0x50,
        CardholderAuthentication = 0x51,
        MerchantNameLocation = 0x52,
        XMLReceiptWithSWRelInfo = 0x53,
        Default_Currency = 0x54,
        TimeZone = 0x55,
        DayLightSavingsTime = 0x56,
        RFU_5 = 0x57,
        RFU_6 = 0x58,
        RFU_7 = 0x59,
        RFU_8 = 0x5A,
        RFU_9 = 0x5B,
        RFU_10 = 0x5C,
        RFU_11 = 0x5D,
        RFU_12 = 0x5E,
        RFU_13 = 0x5F,

        // App 2
        TD2_RFU0 = 0x60,
        TD2_TerminalID = 0x61,
        TD2_BankHostIPAddress = 0x62,
        TD2_BankHostIPPort = 0x63,
        TD2_BankHostConnectionProtocol = 0x64,
        TD2_TerminalIPAddress = 0x65,
        TD2_TerminalIPNetmask = 0x66,
        TD2_TerminalIPGateway = 0x67,
        TD2_Merchant = 0x68,
        TD2_ExtractTimeout = 0x69,
        TD2_FloorLimitAll = 0x6A,
        TD2_GPRSAPN = 0x6B,
        TD2_GPRSLogin = 0x6C,
        TD2_GPRSPassword = 0x6D,
        TD2_TransactionKey = 0x6E,
        TD2_TerminalPeripheralConfig = 0x6F,
        TD2_FloorLimitOne = 0x70,
        TD2_BankHostName = 0x71,
        TD2_DNS1 = 0x72,
        TD2_DNS2 = 0x73,
        // TD2_UNDEFINED14             = 0x74,   Private not to be used but reserved
        TD2_HostStatusCheckPort = 0x75,
        TD2_HostStatusCheckFrequency = 0x76,
        TD2_SyslogPort = 0x77,
        TD2_SyslogIP = 0x78,
        TD2_DisableFullDataOutcome = 0x79,
        TD2_EnableEarlyTLSConnection = 0x7A,
        TD2_QuickChipEnable = 0x7B,
        TD2_MD5ControlEnabled = 0x7C,
        TD2_DeviceAddress = 0x7D,
        TD2_NetworkId = 0x7E,
        TD2_APIKey = 0x7F,
        TD2_APIKey2 = 0x80,
        TD2_AppName = 0x81,
        TD2_AppVer = 0x82,
        TD2_TimeOffset = 0x83,
        TD2_CAPValue = 0x84,
        TD2_BankHostCAID = 0x85,
        TD2_BankHostCertID = 0x86,
        TD2_BankHostName2 = 0x87,
        // TD2_UNDEFINED28             = 0x88,   Private not to be used but reserved
        TD2_BankHostIPAddress2 = 0x89,
        TD2_BankHostIPPort2 = 0x8A,
        TD2_BankHostConnectionProtocol2 = 0x8B,
        TD2_BankHostCAID2 = 0x8C,
        TD2_BankHostCertID2 = 0x8D,
        TD2_BankHostSelection = 0x8E,
        TD2_XMLReceiptWithSWRelInfoV2 = 0x8F,
        TD2_RFU_0 = 0x90,
        TD2_FleetBinManagement = 0x91,
        TD2_RFU_2 = 0x92,
        TD2_RFU_3 = 0x93,
        TD2_RFU_4 = 0x94,
        TD2_RFU_5 = 0x95,
        TD2_RFU_6 = 0x96,
        TD2_RFU_7 = 0x97,
        TD2_RFU_8 = 0x98,
        TD2_RFU_9 = 0x99,
        TD2_RFU_10 = 0x9A,
        TD2_RFU_11 = 0x9B,
        TD2_RFU_12 = 0x9C,
        TD2_RFU_13 = 0x9D,
        TD2_RFU_14 = 0x9E,
        TD2_RFU_15 = 0x9F,
        TD2_RFU_16 = 0xA0,
        TD2_RFU_17 = 0xA1,
        TD2_RFU_18 = 0xA2,
        TD2_RFU_19 = 0xA3,
        TD2_RFU_20 = 0xA4,
        TD2_RFU_21 = 0xA5,
        TD2_RFU_22 = 0xA6,
        TD2_RFU_23 = 0xA7,
        TD2_RFU_24 = 0xA8,
        TD2_RFU_25 = 0xA9,
        TD2_RFU_26 = 0xAA,
        TD2_RFU_27 = 0xAB,
        TD2_RFU_28 = 0xAC,
        TD2_RFU_29 = 0xAD,
        TD2_RFU_30 = 0xAE,
        TD2_RFU_31 = 0xAF
    }
}
