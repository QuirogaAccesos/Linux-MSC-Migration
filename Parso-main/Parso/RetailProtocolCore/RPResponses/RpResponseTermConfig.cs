using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using System.Windows.Forms.VisualStyles;

namespace CCI.Globalcom.GlobalcomRetailProtocol
{
    /// <summary>
    /// Response Class for Terminal Config (RP_GetTerminalConfig)
    /// </summary>
    public class RpResponseTermConfig : RpResponseBase
    {
        public Dictionary<ushort, string> TagList;
        public bool? ChipDisabled;
        public string PinBlockKeyIndex;
        public string PinBlockISOType;
        public bool? DUKPTPinBlock;
        public string P2PEKeyIndex;
        public bool? DUKPTP2PE;
        public bool? ConfirmAmount;
        public bool? FallbackOnNoMatchingApplication;
        public bool? ShowAmountOnPinPad;
        public string MaxNumberOfOfflineTransactionsAllowed;
        public bool? FallbackEnabled;
        public string HashKeyIndex;
        public string GatewayResponseTimeout;
        public bool? CardholderActionDisabled;
        public bool? CommonAIDEnabled;
        public bool? DebitAIDEnabled;
        public bool? DisplayApplicationOnPINReq;
        public bool? UseBankHostIPForCCConnection;

        public RpResponseTermConfig(byte[] response) : base(response)
        {
            TagList = new Dictionary<ushort, string>();


            for (int i = 0; i < (InfoLength - 1); i += 36)
            {
                ushort tag = 0;
                byte[] value = new byte[32];
                tag = (ushort)BitConverter.ToInt16(Info, i);
                Buffer.BlockCopy(Info, i + 3, value, 0, 32);
                
                // If its GatewayResponseTimeout convert it.
                if (tag == 0x00DE)
                    TagList.Add(tag, value[0].ToString());
                else
                    TagList.Add(tag, Encoding.ASCII.GetString(value));
            }

            if (TagList.ContainsKey(0x00D0))
            {
                ChipDisabled = (TagList[(ushort)0x00D0].Substring(0, 1) == "1");
            }
            if (TagList.ContainsKey(0x00D1))
            {
                PinBlockKeyIndex = (TagList[0x00D1]);
            }
            if (TagList.ContainsKey(0x00D2))
            {
                PinBlockISOType = (TagList[0x00D2]);
            }
            if (TagList.ContainsKey(0x00D3))
            {
                DUKPTPinBlock = (TagList[0x00D3].Substring(0, 1) == "1");
            }
            if (TagList.ContainsKey(0x00D4))
            {
                P2PEKeyIndex = (TagList[0x00D4]);
            }
            if (TagList.ContainsKey(0x00D6))
            {
                DUKPTP2PE = (TagList[0x00D6].Substring(0, 1) == "1");
            }
            if (TagList.ContainsKey(0x00D8))
            {
                ConfirmAmount = (TagList[0x00D8].Substring(0, 1) == "1");
            }
            if (TagList.ContainsKey(0x00D9))
            {
                FallbackOnNoMatchingApplication = (TagList[0x00D9].Substring(0, 1) == "1");
            }
            if (TagList.ContainsKey(0x00DA))
            {
                ShowAmountOnPinPad = (TagList[0x00DA].Substring(0, 1) == "1");
            }
            if (TagList.ContainsKey(0x00DB))
            {
                MaxNumberOfOfflineTransactionsAllowed = (TagList[0x00DB]);
            }
            if (TagList.ContainsKey(0x00DC))
            {
                FallbackEnabled = (TagList[0x00DC].Substring(0, 1) == "1");
            }
            if (TagList.ContainsKey(0x00DD))
            {
                HashKeyIndex = (TagList[0x00DD]);
            }
            if (TagList.ContainsKey(0x00DE))
            {
                GatewayResponseTimeout = (TagList[0x00DE]);
            }
            if (TagList.ContainsKey(0x00DF))
            {
                CardholderActionDisabled = (TagList[0x00DF].Substring(0, 1) == "1");
            }
            if (TagList.ContainsKey(0x00E0))
            {
                CommonAIDEnabled = (TagList[0x00E0].Substring(0, 1) == "1");
            }
            if (TagList.ContainsKey(0x00E1))
            {
                DebitAIDEnabled = (TagList[0x00E1].Substring(0, 1) == "1");
            }
            if (TagList.ContainsKey(0x00E2))
            {
                DisplayApplicationOnPINReq = (TagList[0x00E2].Substring(0, 1) == "1");
            }
            if (TagList.ContainsKey(0x00E3))
            {
                UseBankHostIPForCCConnection = (TagList[0x00E3].Substring(0, 1) == "1");
            }
        }

        /// <summary>
        /// Returns a well formatted string of the content of the object
        /// </summary>
        public string PrintDetails()
        {
            StringBuilder sb = new StringBuilder(1000);
            try
            {
                sb.AppendLine("System Config:" + VerifyOutcome().ToString());
                if (VerifyOutcome() != EOutcome.OK)
                {
                    sb.AppendLine(Environment.NewLine);
                    return sb.ToString();
                }

                sb.AppendLine("--> Chip Disabled:" + ((ChipDisabled ?? false) ? "Yes" : "No"));
                sb.AppendLine("--> DUKPT PinBlock:" + ((DUKPTPinBlock ?? false) ? "Yes" : "No"));
                sb.AppendLine("--> DUKPT P2PE/E2EE:" + ((DUKPTP2PE ?? false) ? "Yes" : "No"));
                sb.AppendLine("--> Confirm Amounts:" + ((ConfirmAmount ?? false) ? "Yes" : "No"));
                sb.AppendLine("--> Show Amounts on Pinpad:" + ((ShowAmountOnPinPad ?? false) ? "Yes" : "No"));
                sb.AppendLine("--> Fallback on No Matching AID:" +
                              ((FallbackOnNoMatchingApplication ?? false) ? "Yes" : "No"));
                sb.AppendLine("--> Fallback:" + ((FallbackEnabled ?? false) ? "Yes" : "No"));
                sb.AppendLine("--> Cardholder Action Disabled:" +
                              ((CardholderActionDisabled ?? false) ? "Yes" : "No"));
                sb.AppendLine("--> Common Debit AID Enabled:" + ((CommonAIDEnabled ?? false) ? "Yes" : "No"));
                sb.AppendLine("--> Debit AID Enabled:" + ((DebitAIDEnabled ?? false) ? "Yes" : "No"));
                sb.AppendLine("--> Application Selection:" + ((DisplayApplicationOnPINReq ?? false) ? "Yes" : "No"));
                sb.AppendLine("--> Use Bank Host IP for Host CC Connection:" +
                              ((UseBankHostIPForCCConnection ?? false) ? "Yes" : "No"));
                sb.AppendLine("--> Host/Gateway response Timeout:" + GatewayResponseTimeout);
                sb.AppendLine("--> Card In/Out Hash Key Index:" + HashKeyIndex?.Trim('\0'));
                sb.AppendLine("--> P2PE/E2EE Key Index:" + P2PEKeyIndex?.Trim('\0'));
                sb.AppendLine("--> Pin Block Key Index:" + PinBlockKeyIndex?.Trim('\0'));
                sb.AppendLine("--> PinBlock ISO Type:" + PinBlockISOType?.Trim('\0'));
                sb.AppendLine(
                    "--> Max Number of Offline Transactions:" + MaxNumberOfOfflineTransactionsAllowed?.ToString());
                sb.AppendLine(Environment.NewLine);

                return sb.ToString();
            }
            catch (Exception ex)
            {
                return ("Exception thrown:" + ex.Message + Environment.NewLine + sb.ToString());
            }
        }

        /// <summary>
        /// Returns a well formatted string of the content of the object
        /// </summary>
        public string PrintDetailsAsXml()
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                sb.AppendLine("<SystemConfig>");
                sb.AppendLine("  <Status>" + VerifyOutcome().ToString() + "</Status>");
                if (VerifyOutcome() != EOutcome.OK)
                {
                    sb.AppendLine("</SystemConfig>");
                    return sb.ToString();
                }

                sb.AppendLine("  <Chip>" + ((ChipDisabled ?? false) ? "Disabled" : "Enabled") + "</Chip>");
                sb.AppendLine("  <DUKPTPin>" + ((DUKPTPinBlock ?? false) ? "Enabled" : "Disabled") + "</DUKPTPin>");
                sb.AppendLine("  <DUKPTDataEncryption>" + ((DUKPTP2PE ?? false) ? "Enabled" : "Disabled") +
                              "</DUKPTDataEncryption>");
                sb.AppendLine("  <ConfirmAmounts>" + ((ConfirmAmount ?? false) ? "Enabled" : "Disabled") +
                              "</ConfirmAmounts>");
                sb.AppendLine("  <ShowAmtOnPinpad>" + ((ShowAmountOnPinPad ?? false) ? "Enabled" : "Disabled") +
                              "</ShowAmtOnPinpad>");
                sb.AppendLine("  <FallbackOnNoMatchingAID>" +
                              ((FallbackOnNoMatchingApplication ?? false) ? "Enabled" : "Disabled") +
                              "</FallbackOnNoMatchingAID>");
                sb.AppendLine("  <Fallback>" + ((FallbackEnabled ?? false) ? "Enabled" : "Disabled") + "</Fallback>");
                sb.AppendLine("  <CardholderAction>" + ((CardholderActionDisabled ?? false) ? "Disabled" : "Enabled") +
                              "</CardholderAction>");
                sb.AppendLine("  <CommonDebitAID>" + ((CommonAIDEnabled ?? false) ? "Enabled" : "Disabled") +
                              "</CommonDebitAID>");
                sb.AppendLine("  <DebitAID>" + ((DebitAIDEnabled ?? false) ? "Enabled" : "Disabled") + "</DebitAID>");
                sb.AppendLine("  <ApplicationSelection>" + ((DisplayApplicationOnPINReq ?? false) ? "Enabled" : "Disabled") + "</ApplicationSelection>");
                sb.AppendLine("  <BankHostIPForHostCCConnection>" +
                              ((UseBankHostIPForCCConnection ?? false) ? "Enabled" : "Disabled") +
                              "</BankHostIPForHostCCConnection>");
                sb.AppendLine("  <Host_GatewayResponseTimeout>" + GatewayResponseTimeout?.ToString() +
                              "</Host_GatewayResponseTimeout>");
                sb.AppendLine("  <CardIn_OutHashKeyIndex>" + HashKeyIndex?.Trim('\0') + "</CardIn_OutHashKeyIndex>");
                sb.AppendLine("  <P2PE_E2EEKeyIndex>" + P2PEKeyIndex?.Trim('\0') + "</P2PE_E2EEKeyIndex>");
                sb.AppendLine("  <PinBlockKeyIndex>" + PinBlockKeyIndex?.Trim('\0') + "</PinBlockKeyIndex>");
                sb.AppendLine("  <PinBlockISOType>" + PinBlockISOType?.Trim('\0') + "</PinBlockISOType>");
                sb.AppendLine("  <MaxNumberOfOfflineTransactions>" + MaxNumberOfOfflineTransactionsAllowed?.Trim('\0') +
                              "</MaxNumberOfOfflineTransactions>");
                sb.AppendLine("</SystemConfig>");

                return sb.ToString();
            }
            catch (Exception ex)
            {
                return ("Exception:" + ex.Message + Environment.NewLine + sb.ToString());
            }
        }
    }
}
