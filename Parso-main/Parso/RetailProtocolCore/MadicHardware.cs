using CCI.Globalcom.GlobalcomRetailProtocol;
using System;
using System.Collections.Generic;
//using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using System.Windows.Forms;

namespace CCI.Globalcom.GlobalcomRetailProtocol
{
    public class MadicHardware
    {
        public EDeviceFamily eFamily;
        public EPCIVersion ePCIVersion;

        public string sParentKey;
        public string sPartNumber;
        public string sPinKey;
        public string sDataKey;
        public string sMACAddress;
        public string sFirmware;
        public string sMasterSerialNumber;
        public string sMasterSerialNumberFull;
        public bool bMD5JournalingEnabled;

        private RpResponseSerialNumbers _rpSerial;
        private RpResponseDeviceFamily _rpFamily;
        private RpResponseKeyInfo _rpKeyInfo;

        private RpResponseExtFirmwareVersion _rpFirmwareExt;
        private RpResponseFirmwareVersion _rpFirmware;

        public MadicHardware() { }

        public string Identify(RetailProtocol rp)
        {
            string sLastLine = "";

            try
            {
                _rpFamily = rp.RP_Ext_GetDeviceFamily();
                eFamily = _rpFamily.DeviceFamily;
                sMACAddress = rp.RP_GetMACAddress(false).Replace(":", "");
                _rpSerial = rp.RP_ReadSerialNumber();
                sMasterSerialNumberFull = _rpSerial.SerialNumbers[0].Replace('\0', ' ').TrimEnd();
                sMasterSerialNumber = sMasterSerialNumberFull.Substring(sMasterSerialNumberFull.Length - 5); //only last 5
                bMD5JournalingEnabled = false;

                sLastLine = "RP_GetKeyInformationV2";
                _rpKeyInfo = rp.RP_GetKeyInformationV2();

                sLastLine = "RP_GetKeyInformationV2.GetFamily";
                sParentKey = _rpKeyInfo.GetFamily();

                sLastLine = "RP_GetKeyInformationV2.GetPinType";
                sPinKey = _rpKeyInfo.GetPinType();

                sLastLine = "RP_GetKeyInformationV2.GetDataType";
                sDataKey = _rpKeyInfo.GetDataType();

                if (eFamily == EDeviceFamily.HRT1000)
                {
                    sLastLine = "RP_Ext_GetFirmwareVersion";
                    _rpFirmwareExt = rp.RP_Ext_GetFirmwareVersion(ExtFirmwareVersionType.ALLREETEELXVersions);
                    ePCIVersion = EPCIVersion.PCI5;
                    // TO DO         sFirmware = _rpFirmwareExt.?
                }
                else
                {
                    sLastLine = "RP_ReadBankingParametersV2";

                    RpResponseBankingParam rpBankingResponseJournal = rp.RP_ReadBankingParametersV2(new BankingParameter((byte)EBankingParamsBV.MD5ControlEnabled, EDeviceFamily.BV1000));
                    if (rpBankingResponseJournal.VerifyOutcome() == EOutcome.OK)
                    {
                        if (rpBankingResponseJournal.InfoLength > 1)
                        {
                            bMD5JournalingEnabled = (rpBankingResponseJournal.Info[0] == '1');
                        }
                    }

                    sLastLine = "RP_FirmwareVersionRequest";
                    _rpFirmware = rp.RP_FirmwareVersionRequest();
                    sFirmware = _rpFirmware.MasterFirmwareVersion.Replace('\0', ' ');
                    string pci = _rpFirmware.getPCIVersion();
                    if (pci.Contains("51"))
                        ePCIVersion = EPCIVersion.PCI5;
                    else
                        ePCIVersion = EPCIVersion.PCI4;

                }

                sPartNumber = decipherPartNumber();
                sLastLine = "";
            }
            catch (Exception ex)
            {
            }

            return sLastLine;
        }

        public string PCIVerAsString()
        {
            if (ePCIVersion == EPCIVersion.PCI5)
                return "PCI V51";
            else if (ePCIVersion == EPCIVersion.PCI4)
                return "PCI V41";

            return "PCI V??";
        }

        private string decipherPartNumber()
        {
            if (sParentKey.Equals("Gilbarco") && _rpFirmware.getMasterType().Equals("Reader"))
            {
                // Globalcom Card Reader - NBS Cenex Key
                if (sPinKey.Equals("Cenex CHS II"))
                    return (ePCIVersion == EPCIVersion.PCI4) ? "GVR#:M17986B003" : "GVR#:M17986B103";

                // Globalcom Card Reader - Generic II Key
                if (sPinKey.Equals("NBS GENERIC II"))
                    return (ePCIVersion == EPCIVersion.PCI4) ? "GVR#:M17986B002" : "GVR#:M17986B102";

                //  Globalcom Card Reader - Canadian Key.
                if (sDataKey.Equals("Apriva Production"))
                    return (ePCIVersion == EPCIVersion.PCI4) ? "GVR#:M17986B004" : "GVR#:M17986B104";

                if (sDataKey.Equals("Apriva Test"))
                    return "GVR#: NA APRIVA-TEST";
            }

            if (sParentKey.Equals("T2") && _rpFirmware.getMasterType().Equals("Reader"))
            {
                //500.0226 - EMV Reader 5V PCI4 Primary NMI(Pay Station)
                //502.0032 - EMV Reader 12V PCI4 Primary NMI(PARCS)
                if (ePCIVersion == EPCIVersion.PCI4 && sDataKey.Equals("NMI Production"))
                    return "T2#: 502.0032 or 500.0226";

                //500.0296 - P2PE EMV Reader 5V NMI PCI5 Primary
                if (ePCIVersion == EPCIVersion.PCI5 && sDataKey.Equals("NMI Production"))
                    return "T2#: 500.0296";
            }

            if (sParentKey.Equals("T2") && _rpFirmware.getMasterType().Equals("Contactless"))
            {
                // 503.0756 - EMVCL Reader PCI5 Primary Apriva Retail(PARCS)
                // 500.0298 - EMVCL Reader PCI5 Primary Apriva MUX(Pay stations)
                return "T2#: 503.0756 or 500.0298";
            }

            return sParentKey+" "+ePCIVersion.ToString()+" P:"+sPinKey+ " D:"+sDataKey;
        }

        /// <summary>
        /// Nice output
        /// </summary>
        /// <returns>string</returns>
        public string PrintString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Device Family : " + eFamily.ToString());
            sb.AppendLine("PCI : " +ePCIVersion.ToString());
            sb.AppendLine("Type : " + _rpFirmware.getMasterType());
            
            sb.Append(System.Environment.NewLine);

            sb.AppendLine("VendorKey : " + sParentKey);
            sb.AppendLine("PinKey : " + sPinKey);
            sb.AppendLine("DataKey : " + sDataKey);

            sb.AppendLine("Serial Number : " + _rpSerial.SerialNumbers[0]);
            sb.AppendLine("Firmware : " + _rpFirmware.MasterFirmwareVersion);

            sb.Append(System.Environment.NewLine);

            sb.AppendLine("Partnumber : " + sPartNumber);


            sb.AppendLine("MAC :"+ sMACAddress);

            sb.AppendLine("MD5Journaling:" + ((bMD5JournalingEnabled) ? "Enabled" : "Disabled"));
        

            return sb.ToString();
        }

    }
}
