using CCI.Globalcom.GlobalcomRetailProtocol;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using static System.Net.Mime.MediaTypeNames;
//using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace CCI.Globalcom.GlobalcomRetailProtocol
{
    public class DynamicPrompt
    {
        public enum PromptFormatType { ALPHA, NUMERIC, HIDDEN, ANY };

        /// <summary>
        ///   Constructor
        /// </summary>
        public DynamicPrompt() { }

        /// <summary>
        ///  Two alpha numeric characters.
        /// </summary>
        public string promptID { get; set; }
        /// <summary>
        ///   defines what the mask should be
        /// </summary>
        public PromptFormatType promptType { get; set; }
        
        /// <summary>
        ///   Given the prompt type as a string set the prompt type in the object
        /// </summary>
        /// <param name="value"></param>
        public void convertToPromptType(string value)
        {
            switch (value[0])
            {
                case 'A':
                    promptType = PromptFormatType.ALPHA;
                    return;
                case 'H':
                    promptType = PromptFormatType.HIDDEN;
                    return;
                case 'N':
                    promptType = PromptFormatType.NUMERIC;
                    return;
                default:
                    promptType = PromptFormatType.ANY;
                    return;
            };
        }

        /// <summary>
        ///  Should the text box use password protection?
        /// </summary>
        /// <returns></returns>
        public bool isHidden()
        {
            return promptType == PromptFormatType.HIDDEN;
        }
        
        /// <summary>
        ///   return the mask for a masked text box.
        /// </summary>
        /// <returns></returns>
        public string typeMask()
        {
            string theMask = "";
            switch (promptType)
            {
                case PromptFormatType.ALPHA:
                    return theMask.PadLeft(promptLength, '?');
                case PromptFormatType.HIDDEN:
                    return theMask.PadLeft(promptLength, 'a');
                case PromptFormatType.NUMERIC:
                    return theMask.PadLeft(promptLength, '9');
                default:
                    return theMask.PadLeft(promptLength, 'a');
            };
        }

        /// <summary>
        ///  maximum length
        /// </summary>
        public int promptLength { get; set; }
        /// <summary>
        ///   Description of what is being prompted
        /// </summary>
        public string promptText { get; set; }
    }


    public class V2FleetPromptDetails
    {
        public enum V2DeviceType { 
            NotUsed,
            MagneticStripeCard,
            ChipCard,
            FRIDNFCTransponder,
            BarCode,
            AutmaticLPR,
            OBD,
            ConexxusRFU,
            ProprietryRFU
        };

        public bool numeric { get; set; }
        public bool alphanumeric{ get; set; }
        public bool mandatory { get; set; }
        public bool allowManualEntry { get; set; }

        public V2DeviceType devicetype1 { get; set; }
        public V2DeviceType devicetype2 { get; set; }

        public bool printOnReceipt { get; set; }
        public bool enterInClear { get; set; }

        public string exptectedValue { get; set; }

        public string PrintDetails()
        {
            StringBuilder sb = new StringBuilder();

            if (devicetype1 != V2DeviceType.NotUsed)
                sb.Append(devicetype1.ToString());

            if (devicetype2 != V2DeviceType.NotUsed)
                sb.Append(devicetype2.ToString());

            if (mandatory) sb.Append(" Mandatory");
            else sb.Append(" Optional");

            if (numeric) sb.Append(" Numeric");
            else if (alphanumeric) sb.Append(" Alphanumeric");

            if (allowManualEntry) sb.Append(" ManualAllowed");
            else sb.Append(" ManualNotAllowed");

            if (printOnReceipt) sb.Append(" PrintOnReceipt");

            if (enterInClear) sb.Append(" EnterInClear");
            else sb.Append(" DontEnterInClear");

            if (!string.IsNullOrEmpty(exptectedValue))
                sb.Append(" ExpectedValue(" + exptectedValue + ")");

            return sb.ToString();
        }

    }

    /// <summary>
    /// Response Class for 0x1E data for VISA FLEET 2.0 
    /// </summary>
    public class RpResponseVisaFleet2List : RpResponseBase 
    {
        /// <summary>
        ///   List of dynamic prompts that should be asked for.
        /// </summary>
        public List<EPetroPromptTags> PromptTags;
        public Dictionary<EPetroPromptTags, V2FleetPromptDetails> PromptTagDetails;
        /// <summary>
        /// TagList
        /// </summary>
        public Dictionary<string, string> TagList;


        /// <summary>
        ///                          9f 0a 08 00 02 05 10 08  .Ý/r*...Ÿ.......
        ///  40 00 80 df 30 06 24 20 c0 29 00 00 df 32 08 c7  @.€ß0.$ À)..ß2.Ç
        ///  80 00 00 00 00 00 00 df 43 08 42 31 32 33 34 35  €......ßC.B12345
        ///  36 37 00 00 00 00 00 00 00 00 00 00 00 00 00 00  67..............
        /// </summary>
        /// <param name="response"></param>
        /// <exception cref="Exception"></exception>
        public RpResponseVisaFleet2List(byte[] response) : base(response)
        {
            if (VerifyOutcome() != EOutcome.OK)
                return;

            TagList = new Dictionary<string, string>();
            PromptTags = new List<EPetroPromptTags>();
            PromptTagDetails = new Dictionary<EPetroPromptTags, V2FleetPromptDetails>();

            string tagName;
            for (int gIndex = 0; gIndex < (InfoLength- 1); gIndex++)
            {
                int tagLen;
                if (checkTLVFirstByte(Info[gIndex]))
                {
                    tagName = Info[gIndex++].ToString("X2");
                    tagName += Info[gIndex++].ToString("X2");
                }
                else
                    tagName = Info[gIndex++].ToString("X2");

                tagLen = Convert.ToInt16(Info[gIndex]); 
                byte[] byteArray = new byte[tagLen];
                System.Buffer.BlockCopy(Info, gIndex+1, byteArray, 0, tagLen);

                // Prompts
                if (tagName.Equals("DF30"))
                {
                    ParseDF30(byteArray);
                    TagList.Add(tagName, BitConverter.ToString(byteArray));
                }
                else if (tagName.Equals("9F0A"))
                    TagList.Add(tagName, BitConverter.ToString(byteArray));
                else if (tagName.Equals("DF32"))
                    TagList.Add(tagName, BitConverter.ToString(byteArray));
                else  // Expected Values
                {
                    string expectedValue = System.Text.Encoding.ASCII.GetString(byteArray);
                    TagList.Add(tagName, expectedValue);

                    EPetroPromptTags tempTag = 0;
                    if (tagName.Equals("DF40"))
                        tempTag = EPetroPromptTags.VF2_GenericID;
                    else if (tagName.Equals("DF41"))
                        tempTag = EPetroPromptTags.VF_VehicleID;
                    else if (tagName.Equals("DF43"))
                        tempTag = EPetroPromptTags.VF_DriverID;
                    else if (tagName.Equals("DF52"))
                        tempTag = EPetroPromptTags.VF2_TrailerNumbr;
                    else if (tagName.Equals("DF53"))
                        tempTag = EPetroPromptTags.VF2_EmployeeNumber;
                    else if (tagName.Equals("DF54"))
                        tempTag = EPetroPromptTags.VF2_WorkOrder;
                    else if (tagName.Equals("DF55"))
                        tempTag = EPetroPromptTags.VF2_AdditionalData1;
                    else if (tagName.Equals("DF56"))
                        tempTag = EPetroPromptTags.VF2_AdditionalData2;

                    if (tempTag != 0)
                    {
                        // Yes im assuming they tell us what prompts are required before the expected value.
                        V2FleetPromptDetails value = PromptTagDetails[tempTag];
                        value.exptectedValue = expectedValue;
                        PromptTagDetails[tempTag] = value;
                    }
                }

                gIndex += tagLen;
            }
        }

        /// <summary>
        ///     df 30 [06] 24 20 c0 29 00 00
        ///               24 - 0010 0100 -- DriverID, ans, Opt, NoManual
        ///               20 - 0010 0000 -- Chip Card
        ///               c0 - 1100 0000 -- Print, Clear
        ///               
        ///               29 - 0010 1001 -- Odometer, num, Opt, ManualOk
        ///               00 - 0000 0000 -- Not Used
        ///               00 - 0000 0000 -- No Print, No Clear
        /// </summary>
        /// <param name="tagvalue"></param>
        public void ParseDF30(byte[] tagvalue)
        {
            for (int gIndex = 0; gIndex < tagvalue.Length; gIndex+=3)
            {
                byte b1 = tagvalue[gIndex];
                byte b2 = tagvalue[gIndex+1];
                byte b3 = tagvalue[gIndex+2];

                V2FleetPromptDetails details = new V2FleetPromptDetails();
                details.numeric = !IsBitSetAtPos(b1, 3);
                details.alphanumeric = IsBitSetAtPos(b1, 3);
                details.mandatory = IsBitSetAtPos(b1, 2);
                details.allowManualEntry = IsBitSetAtPos(b1, 1);

                details.printOnReceipt = IsBitSetAtPos(b3, 8);
                details.enterInClear = IsBitSetAtPos(b3, 7);

                // Device Type 1
                if (!IsBitSetAtPos(b2, 8) && !IsBitSetAtPos(b2, 7) && !IsBitSetAtPos(b2, 6) && !IsBitSetAtPos(b2, 5))
                    details.devicetype1 = V2FleetPromptDetails.V2DeviceType.NotUsed;
                else if (!IsBitSetAtPos(b2, 8) && !IsBitSetAtPos(b2, 7) && !IsBitSetAtPos(b2, 6) && IsBitSetAtPos(b2, 5))
                    details.devicetype1 = V2FleetPromptDetails.V2DeviceType.MagneticStripeCard;
                else if (!IsBitSetAtPos(b2, 8) && !IsBitSetAtPos(b2, 7) && IsBitSetAtPos(b2, 6) && !IsBitSetAtPos(b2, 5))
                    details.devicetype1 = V2FleetPromptDetails.V2DeviceType.ChipCard;
                else if (!IsBitSetAtPos(b2, 8) && IsBitSetAtPos(b2, 7) && !IsBitSetAtPos(b2, 6) && !IsBitSetAtPos(b2, 5))
                    details.devicetype1 = V2FleetPromptDetails.V2DeviceType.BarCode;
                else if (!IsBitSetAtPos(b2, 8) && IsBitSetAtPos(b2, 7) && !IsBitSetAtPos(b2, 6) && IsBitSetAtPos(b2, 5))
                    details.devicetype1 = V2FleetPromptDetails.V2DeviceType.AutmaticLPR;
                else if (!IsBitSetAtPos(b2, 8) && IsBitSetAtPos(b2, 7) && IsBitSetAtPos(b2, 6) && !IsBitSetAtPos(b2, 5))
                    details.devicetype1 = V2FleetPromptDetails.V2DeviceType.OBD;
                else if (!IsBitSetAtPos(b2, 8) && IsBitSetAtPos(b2, 7) && IsBitSetAtPos(b2, 6) && IsBitSetAtPos(b2, 5))
                    details.devicetype1 = V2FleetPromptDetails.V2DeviceType.ConexxusRFU;
                else if (IsBitSetAtPos(b2, 8))
                    if (!IsBitSetAtPos(b2, 7))
                        details.devicetype1 = V2FleetPromptDetails.V2DeviceType.ConexxusRFU;
                    else
                        details.devicetype1 = V2FleetPromptDetails.V2DeviceType.ProprietryRFU;

                // Device Type 2
                if (!IsBitSetAtPos(b2, 4) && !IsBitSetAtPos(b2, 3) && !IsBitSetAtPos(b2, 2) && !IsBitSetAtPos(b2, 1))
                    details.devicetype2 = V2FleetPromptDetails.V2DeviceType.NotUsed;
                else if (!IsBitSetAtPos(b2, 4) && !IsBitSetAtPos(b2, 3) && !IsBitSetAtPos(b2, 2) && IsBitSetAtPos(b2, 1))
                    details.devicetype2 = V2FleetPromptDetails.V2DeviceType.MagneticStripeCard;
                else if (!IsBitSetAtPos(b2, 4) && !IsBitSetAtPos(b2, 3) && IsBitSetAtPos(b2, 2) && !IsBitSetAtPos(b2, 1))
                    details.devicetype2 = V2FleetPromptDetails.V2DeviceType.ChipCard;
                else if (!IsBitSetAtPos(b2, 4) && IsBitSetAtPos(b2, 3) && !IsBitSetAtPos(b2, 2) && !IsBitSetAtPos(b2, 1))
                    details.devicetype2 = V2FleetPromptDetails.V2DeviceType.BarCode;
                else if (!IsBitSetAtPos(b2, 4) && IsBitSetAtPos(b2, 3) && !IsBitSetAtPos(b2, 2) && IsBitSetAtPos(b2, 1))
                    details.devicetype2 = V2FleetPromptDetails.V2DeviceType.AutmaticLPR;
                else if (!IsBitSetAtPos(b2, 4) && IsBitSetAtPos(b2, 3) && IsBitSetAtPos(b2, 2) && !IsBitSetAtPos(b2, 1))
                    details.devicetype2 = V2FleetPromptDetails.V2DeviceType.OBD;
                else if (!IsBitSetAtPos(b2, 4) && IsBitSetAtPos(b2, 3) && IsBitSetAtPos(b2, 2) && IsBitSetAtPos(b2, 1))
                    details.devicetype2 = V2FleetPromptDetails.V2DeviceType.ConexxusRFU;
                else if (IsBitSetAtPos(b2, 4))
                    if (!IsBitSetAtPos(b2, 3))
                        details.devicetype2 = V2FleetPromptDetails.V2DeviceType.ConexxusRFU;
                    else
                        details.devicetype2 = V2FleetPromptDetails.V2DeviceType.ProprietryRFU;

                if (!IsBitSetAtPos(b3, 6) && !IsBitSetAtPos(b3, 5))
                {
                    // Generic ID
                    if (!IsBitSetAtPos(b1, 8) && !IsBitSetAtPos(b1, 7) && !IsBitSetAtPos(b1, 6) && !IsBitSetAtPos(b1, 5) && IsBitSetAtPos(b1, 4))
                    {
                        PromptTags.Add(EPetroPromptTags.VF2_GenericID);
                        PromptTagDetails.Add(EPetroPromptTags.VF2_GenericID, details);
                    }
                    // Vehicle ID
                    else if (!IsBitSetAtPos(b1, 8) && !IsBitSetAtPos(b1, 7) && !IsBitSetAtPos(b1, 6) && IsBitSetAtPos(b1, 5) && !IsBitSetAtPos(b1, 4))
                    {
                        PromptTags.Add(EPetroPromptTags.VF_VehicleID);
                        PromptTagDetails.Add(EPetroPromptTags.VF_VehicleID, details);
                    }
                    // Driver ID
                    else if (!IsBitSetAtPos(b1, 8) && !IsBitSetAtPos(b1, 7) && IsBitSetAtPos(b1, 6) && !IsBitSetAtPos(b1, 5) && !IsBitSetAtPos(b1, 4))
                    {
                        PromptTags.Add(EPetroPromptTags.VF_DriverID);
                        PromptTagDetails.Add(EPetroPromptTags.VF_DriverID, details);
                    }
                    // Odometer ID
                    else if (!IsBitSetAtPos(b1, 8) && !IsBitSetAtPos(b1, 7) && IsBitSetAtPos(b1, 6) && !IsBitSetAtPos(b1, 5) && IsBitSetAtPos(b1, 4))
                    {
                        PromptTags.Add(EPetroPromptTags.VF_Odometer);
                        PromptTagDetails.Add(EPetroPromptTags.VF_Odometer, details);
                    }
                    // Fleet Trailer Number
                    else if (IsBitSetAtPos(b1, 8) && IsBitSetAtPos(b1, 7) && !IsBitSetAtPos(b1, 6) && !IsBitSetAtPos(b1, 5) && !IsBitSetAtPos(b1, 4))
                    {
                        PromptTags.Add(EPetroPromptTags.VF2_TrailerNumbr);
                        PromptTagDetails.Add(EPetroPromptTags.VF2_TrailerNumbr, details);
                    }
                    // Fleet Work Order
                    else if (!IsBitSetAtPos(b1, 8) && IsBitSetAtPos(b1, 7) && !IsBitSetAtPos(b1, 6) && !IsBitSetAtPos(b1, 5) && IsBitSetAtPos(b1, 4))
                    {
                        PromptTags.Add(EPetroPromptTags.VF2_WorkOrder);
                        PromptTagDetails.Add(EPetroPromptTags.VF2_WorkOrder, details);
                    }
                    else { /* Unknown Prompt?? */ }
                }
                else if (!IsBitSetAtPos(b3, 6) && IsBitSetAtPos(b3, 5))
                {
                    // Fleet Employee Number
                    if (!IsBitSetAtPos(b1, 8) && !IsBitSetAtPos(b1, 7) && IsBitSetAtPos(b1, 6) && IsBitSetAtPos(b1, 5) && IsBitSetAtPos(b1, 4))
                    {
                        PromptTags.Add(EPetroPromptTags.VF2_EmployeeNumber);
                        PromptTagDetails.Add(EPetroPromptTags.VF2_EmployeeNumber, details);
                    }
                    else { /* Unknown Prompt?? */ }
                }
                else if (IsBitSetAtPos(b3, 6) && !IsBitSetAtPos(b3, 5))
                {
                    // Fleet Additional Prompt 1
                    if (IsBitSetAtPos(b1, 8) && IsBitSetAtPos(b1, 7) && !IsBitSetAtPos(b1, 6) && IsBitSetAtPos(b1, 5) && IsBitSetAtPos(b1, 4))
                    {
                        PromptTags.Add(EPetroPromptTags.VF2_AdditionalData1);
                        PromptTagDetails.Add(EPetroPromptTags.VF2_AdditionalData1, details);
                    }
                    else { /* Unknown Prompt?? */ }
                }
                else // if (IsBitSetAtPos(b3, 6) && IsBitSetAtPos(b3, 5))
                {
                    // Fleet Additiona Prompt 2
                    if (IsBitSetAtPos(b1, 8) && IsBitSetAtPos(b1, 7) && IsBitSetAtPos(b1, 6) && !IsBitSetAtPos(b1, 5) && !IsBitSetAtPos(b1, 4))
                    {
                        PromptTags.Add(EPetroPromptTags.VF2_AdditionalData2);
                        PromptTagDetails.Add(EPetroPromptTags.VF2_AdditionalData2, details);
                    }
                    else { /* Unknown Prompt?? */ }
                }
            }
        }

        /// <summary>
        ///   df 32 [08] c7 80 00 00 00 00 00 00
        ///         c7 - 1100 0111 -- Host, Fuel Allowed, Negative Transactions, Admin Fees, Bulk
        ///         80 - 1000 0000 -- Dispensed Gasoline
        ///         00 - 0000 0000 -- 
        ///         00 - 0000 0000 -- 
        ///         00 - 0000 0000 -- 
        ///         00 - 0000 0000 -- 
        ///         00 - 0000 0000 -- 
        ///         00 - 0000 0000 -- 
        /// </summary>
        /// <param name="tagvalue"></param>
        public string ParseDF32ToString(byte[] tagvalue)
        {
            StringBuilder sb = new StringBuilder();

            byte b1 = tagvalue[0];
            sb.Append(" Fleet Restrictions:");
            sb.Append(IsBitSetAtPos(b1, 8) ? "  ChipBased" : "  HostBased");
            sb.Append(IsBitSetAtPos(b1, 7) ? "  FuelAllowed" : "");
            sb.Append(IsBitSetAtPos(b1, 6) ? "  FuelCatagory/Gasoline Grades" : "");

            sb.Append(IsBitSetAtPos(b1, 3) ? "  NegativeTransactions" : "");
            sb.Append(IsBitSetAtPos(b1, 2) ? "  Administrative" : "");
            sb.Append(IsBitSetAtPos(b1, 1) ? "  Bulk" : "");

            byte b2 = tagvalue[1];
            sb.Append(IsBitSetAtPos(b2, 8) ? "  Dispensed Gasoline" : "");
            sb.Append(IsBitSetAtPos(b2, 7) ? "  Dispensed Diesel" : "");
            sb.Append(IsBitSetAtPos(b2, 6) ? "  Dispnesed Off-Road Fuel" : "");
            sb.Append(IsBitSetAtPos(b2, 5) ? "  Dispensed Electric" : "");
            sb.Append(IsBitSetAtPos(b2, 4) ? "  Dispensed CNG or LNG" : "");
            sb.Append(IsBitSetAtPos(b2, 3) ? "  Dispensed Kerosene" : "");
            sb.Append(IsBitSetAtPos(b2, 2) ? "  Aviation Fuels" : "");
            sb.Append(IsBitSetAtPos(b2, 1) ? "  Marine Fuels" : "");


            byte b3 = tagvalue[2];
            sb.Append(IsBitSetAtPos(b3, 8) ? "  Vehicles Products/Services" : "");
            sb.Append(IsBitSetAtPos(b3, 7) ? "  Aviation Products/Services" : "");
            sb.Append(IsBitSetAtPos(b3, 6) ? "  Marine Products/Services" : "");
            sb.Append(IsBitSetAtPos(b3, 5) ? "  Mechandise" : "");
            sb.Append(IsBitSetAtPos(b3, 4) ? "  Store Services" : "");
            //sb.Append(IsBitSetAtPos(b3, 3) ? "  " + Environment.NewLine : "");
            //sb.Append(IsBitSetAtPos(b3, 2) ? "  " + Environment.NewLine : "");
            //sb.Append(IsBitSetAtPos(b3, 1) ? "  " + Environment.NewLine : "");

            byte b4 = tagvalue[3];
            sb.Append(IsBitSetAtPos(b4, 8) ? "  Tabacco" : "");
            sb.Append(IsBitSetAtPos(b4, 7) ? "  Alcohol" : "");
            sb.Append(IsBitSetAtPos(b4, 6) ? "  Food" : "");
            sb.Append(IsBitSetAtPos(b4, 5) ? "  Lottery" : "");
            sb.Append(IsBitSetAtPos(b4, 4) ? "  Money Order" : "");
            sb.Append(IsBitSetAtPos(b4, 3) ? "  Health & Beauty" : "");
            sb.Append(IsBitSetAtPos(b4, 2) ? "  General Publications" : "");
            sb.Append(IsBitSetAtPos(b4, 1) ? "  Prepaid and Bill Pay" : "");

            byte b5 = tagvalue[4];
            sb.Append(IsBitSetAtPos(b5, 8) ? "  Cannabinoid" : "");
            //sb.Append(IsBitSetAtPos(b5, 7) ? "  " : "");
            //sb.Append(IsBitSetAtPos(b5, 6) ? "  " : "");
            //sb.Append(IsBitSetAtPos(b5, 5) ? "  " : "");
            //sb.Append(IsBitSetAtPos(b5, 4) ? "  " : "");
            //sb.Append(IsBitSetAtPos(b5, 3) ? "  " : "");
            //sb.Append(IsBitSetAtPos(b5, 2) ? "  " : "");
            //sb.Append(IsBitSetAtPos(b5, 1) ? "  " : "");

            byte b8 = tagvalue[7];
            sb.Append(IsBitSetAtPos(b8, 8) ? "  Regular Gas" : "");
            sb.Append(IsBitSetAtPos(b8, 7) ? "  Plus/Midgrade Gas" : "");
            sb.Append(IsBitSetAtPos(b8, 6) ? "  Super/Premium Gas" : "");
            //sb.Append(IsBitSetAtPos(b8, 5) ? "  " + Environment.NewLine : "");
            //sb.Append(IsBitSetAtPos(b8, 4) ? "  " + Environment.NewLine : "");
            //sb.Append(IsBitSetAtPos(b8, 3) ? "  " + Environment.NewLine : "");
            //sb.Append(IsBitSetAtPos(b8, 2) ? "  " + Environment.NewLine : "");
            //sb.Append(IsBitSetAtPos(b8, 1) ? "  " + Environment.NewLine : "");

            return sb.ToString();
        }

        /// <summary>
        /// Function to determine if the first TAG byte of BER-TLV Data indicates a 2nd tag byte follows
        /// </summary>
        /// <param name="stestByte"></param>
        /// <returns>bool </returns>
        public bool checkTLVFirstByte(byte testByte)
        {
            return (IsBitSet(testByte, 4) && IsBitSet(testByte, 3) && IsBitSet(testByte, 2) &&
                    IsBitSet(testByte, 1) && IsBitSet(testByte, 0));

        }

        /// <summary>
        /// Returns a well formatted string of the content of the object
        /// </summary>
        public string PrintDetails()
        {
            StringBuilder sb = new StringBuilder(1000);
            try
            {
                sb.AppendLine("Visa Fleet 2: " + VerifyOutcome().ToString());
                if (VerifyOutcome() != EOutcome.OK)
                    return sb.ToString();

                foreach (KeyValuePair<EPetroPromptTags, V2FleetPromptDetails> entry in PromptTagDetails)
                    sb.AppendLine(" " + entry.Key.ToString() + " -> " +  entry.Value.PrintDetails());

                foreach (KeyValuePair<string, string> entry in TagList)
                {
                    if (entry.Key.Equals("9F0A"))
                        sb.AppendLine(" " + entry.Key + " -> " + entry.Value);
                    else if (entry.Key.Equals("DF32"))
                    {
                        byte[] result = entry.Value
                            .Split('-')                               // Split into items 
                            .Select(item => Convert.ToByte(item, 16)) // Convert each item into byte
                            .ToArray();

                        sb.AppendLine(ParseDF32ToString(result));
                    }
                }

                return sb.ToString();
            }
            catch (Exception ex)
            {
                return ("Exception thrown:" + ex.Message + Environment.NewLine + sb.ToString());
            }
        }
    }

    /// <summary>
    ///   Response class for 0x1E with type 0x17 containing the Dynamic Prompts
    /// </summary>
    public class RpResponseDynamicPromptList : RpResponseBase
    {
        /// <summary>
        ///   List of dynamic prompts that should be asked for.
        /// </summary>
        public Dictionary<EPetroPromptTags, DynamicPrompt> PromptList;

        /// <summary>
        ///   Decode the response from the dymanic prompt list request
        /// </summary>
        /// <param name="response"></param>
        public RpResponseDynamicPromptList(byte[] response) : base(response)
        {
            PromptList = new Dictionary<EPetroPromptTags, DynamicPrompt>();

            if (VerifyOutcome() != EOutcome.OK)
                return;
               
            int gIndex = 0;
            int labelLen = 0;

            // First two bytes is the total prompts
            int numberOfPrompts = Convert.ToInt32(System.Text.Encoding.UTF8.GetString(Info, gIndex, 2));
            gIndex += 2;

            // Now we loop
            for (int i = 0; i < numberOfPrompts; i++)
            {
                DynamicPrompt prompt = new DynamicPrompt();

                // First the Prompt ID which is of size two (2an)
                prompt.promptID = System.Text.Encoding.UTF8.GetString(Info, gIndex, 2);
                gIndex += 2;

                // Next is the Prompt Type which is size one alpha
                prompt.convertToPromptType(System.Text.Encoding.UTF8.GetString(Info, gIndex, 1));
                gIndex += 1;

                // Next is max length of the data to be entered (2n)
                prompt.promptLength = Convert.ToInt32(System.Text.Encoding.UTF8.GetString(Info, gIndex, 2));
                gIndex += 2;

                // Next is max length of the data to be entered (2n)
                labelLen = Convert.ToInt32(System.Text.Encoding.UTF8.GetString(Info, gIndex, 2));
                gIndex += 2;

                // Bad response from host???
                if (gIndex + labelLen > Info.Length)
                    continue;

                prompt.promptText = System.Text.Encoding.UTF8.GetString(Info, gIndex, labelLen);
                gIndex += labelLen;

                PromptList.Add(EPetroPromptTags.DYNAMICPROMPTSEXIST + 1 + i, prompt);
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
                sb.AppendLine("Dynamic Prompts:" + VerifyOutcome().ToString());
                if (VerifyOutcome() != EOutcome.OK)
                {
                    sb.AppendLine(Environment.NewLine);
                    return sb.ToString();
                }

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
