using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CCI.Globalcom.GlobalcomRetailProtocol;

namespace CCI.Globalcom.GlobalcomRetailProtocol
{

    /// <summary>
    /// Enumeration for different Petroleum prompts
    /// </summary>
    public enum EPetroPromptTags
    {
        //WEX
        WEX_UserID = 1,
        WEX_VehicleID = 2,
        WEX_VehicleTagID = 3,
        WEX_DriverID = 4,
        WEX_Odometer = 5,
        WEX_DriverLicenseNumber = 6,
        WEX_DriverLicenseState = 7,
        WEX_DriverLicenseName = 8,
        WEX_WorkOrder = 9,
        WEX_InvoiceNumber = 10,
        WEX_TripNumber = 11,
        WEX_UnitNumber = 12,
        WEX_TrailerHoursReferHours = 13,
        WEX_DateofBirth = 14,
        WEX_ZIP = 15,
        WEX_Data = 16,
        WEX_EnteredData = 17,
        //WEX Reserved = 18 - 26
        WEX_CashBackAmount = 27, //OOS
        WEX_JobNumber = 28,
        WEX_Maintenance = 29,
        WEX_Department = 30,
        WEX_VIN = 31,
        WEX_TractorNumber = 32,
        WEX_Hubometer = 33,
        WEX_TrailerNumber = 34,
        //35-36 reserved for WEX
        //Voyager
        VOY_VoyagerID = 37,
        VOY_VehicleID = 38,
        VOY_DriverID = 39,
        VOY_Odometer = 40,
        VOY_TrailerID = 41,
        VOY_ReeferHours = 42,
        VOY_TripNumber = 43,
        VOY_ExtendedDriverID = 44,
        VOY_UnitID = 45,
        VOY_Contract_PO = 46,
        VOY_DriverLicense = 47,
        VOY_DriverLicenseState = 48,
        VOY_VehicleLicense = 49,
        VOY_VehicleLicenseState = 50,
        VOY_TrailerLicense = 51,
        VOY_TrailerLicenseState = 52,
        // 53-114 Reserved for Voyager
        //PL
        PL_DriverID = 115,
        PL_VehicleID = 116,
        PL_Odometer = 117,
        //PF
        PF_DriverID = 118,
        PF_VehicleID = 119,
        PF_Odometer = 120,
        //VF
        VF_DriverID = 121,
        VF_VehicleID = 122,
        VF_Odometer = 123,
        //P1
        P1_CardHolderID = 124,
        //MF / VI
        MFVI_DriverID = 125,
        MFVI_VehicleID = 126,
        MFVI_Odometer = 127,
        //FM
        FM_DriverID = 128,
        FM_ID_JobNumber = 129,
        FM_Odometer = 130,

        //COMDATA
        COM_UnitNumber = 131,

        //Visa Fleet2
        VF2_GenericID = 132,
        VF2_WorkOrder = 133,
        VF2_EmployeeNumber = 134,
        VF2_TrailerNumbr = 135,
        VF2_AdditionalData1 = 136,
        VF2_AdditionalData2 = 137,

        //General
        SoftwareVersion = 138,

        //ProductData  (Dont forget to check EPetroProductDetailTags)
        ProductDataPumpNumber = 200,    // 2 N
        ProductDataUnitPrice = 201,     // 8 X (4.3 D)
        ProductDataQuantityNBS = 202,   // 10 X (4.5 D) - NBS Style 
        ProductDataProductCode = 203,   // 2-6 X - NBS   or 2-4 N - CFN
        ProductDataAmount = 204,        // 8 X (4.3 D)
        ProductGSTAmount = 205,         // 8 X (4.3 D)
        ProductPSTAmount = 206,         // 8 X (4.3 D)
        ProductName = 207,              // 10 AN
        ProductDataQuantityCFN = 208,   // 10 X (6.3 D) - CFN Style
        ProductDataAmountChase = 209,   // 10 .2 D) - Chase Style
        ProductDataFleetUniqueId = 210,     // 4 N


        DYNAMICPROMPTSEXIST = 240,      // Not an actual prompt
        DYNAMICPROMPT1 = 241,              // Dynamic Prompt 1 from 0x17 file
        DYNAMICPROMPT2 = 242,              // Dynamic Prompt 2 from 0x17 file
        DYNAMICPROMPT3 = 243,              // Dynamic Prompt 3 from 0x17 file
        DYNAMICPROMPT4 = 244,              // Dynamic Prompt 4 from 0x17 file
        DYNAMICPROMPT5 = 245,              // Dynamic Prompt 5 from 0x17 file
        DYNAMICPROMPT6 = 246,              // Dynamic Prompt 6 from 0x17 file
        DYNAMICPROMPT7 = 247,              // Dynamic Prompt 7 from 0x17 file
        DYNAMICPROMPT8 = 248,              // Dynamic Prompt 8 from 0x17 file
        DYNAMICPROMPT9 = 249               // Dynamic Prompt 9 from 0x17 file
    }



    /// <summary>
    /// Class used to store the required prompts and reply.
    /// </summary>
    public class Prompting
    {
        public EPetroPromptTags Tag { get; set; }
        
        public string Response { get; set; }

        public string PromptString
        {
            get
            {
                if (GBCPromptList.gbcPromptDictionary.FirstOrDefault(t => t.Key == Tag).Value == null)
                    return Tag.ToString();
                return "Enter the:" + GBCPromptList.gbcPromptDictionary.FirstOrDefault(t => t.Key == Tag).Value.PromptText;
            }
        }

        public string PromptSyntax
        {
            get
            {
                if (GBCPromptList.gbcPromptDictionary.FirstOrDefault(t => t.Key == Tag).Value == null) return "";

                return GBCPromptList.gbcPromptDictionary.FirstOrDefault(t => t.Key == Tag).Value.TypeMask;
            }
        }
    }

    /// <summary>
    /// Class that defines the meta data for each prompt
    /// </summary>
    public class GBCPrompt
    {
        /// <summary>
        /// Wex's Code
        /// </summary>
        public string WexCode;

        /// <summary>
        /// Mask for input to restrict the datatype to a specific number of characters and syntax
        /// </summary>
        public string TypeMask;

        /// <summary>
        /// The text to Prompt with
        /// </summary>
        public string PromptText;
    }

    /// <summary>
    /// This class defines the prompting syntax requirements and text for each prompt.
    /// Syntax reference: https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.maskedtextbox.mask?view=netcore-3.1
    /// </summary>
    public static class GBCPromptList
    {
        public static Dictionary<EPetroPromptTags, GBCPrompt> gbcPromptDictionary =
            new Dictionary<EPetroPromptTags, GBCPrompt>()
            {
                //Wex
                {EPetroPromptTags.WEX_UserID,new GBCPrompt(){WexCode = "0",TypeMask = "aaaaaaaaaaaa",PromptText = "User ID"}},
                {EPetroPromptTags.WEX_VehicleID,new GBCPrompt(){WexCode = "1",TypeMask = "aaaaaaaaaaaaaaa",PromptText = "Vehicle ID"}},
                {EPetroPromptTags.WEX_VehicleTagID,new GBCPrompt(){WexCode = "2",TypeMask = "aaaaaaaaaaaa",PromptText = "Vehicle Tag ID"}},
                {EPetroPromptTags.WEX_DriverID,new GBCPrompt(){WexCode = "3",TypeMask = "00009999",PromptText = "Driver ID"}},
                {EPetroPromptTags.WEX_Odometer,new GBCPrompt(){WexCode = "4",TypeMask = "999999999",PromptText = "Odometer"}},
                {EPetroPromptTags.WEX_DriverLicenseNumber,new GBCPrompt(){WexCode = "5",TypeMask = "aaaaaaaaaaaaaaa",PromptText = "Driver License Number"}},
                {EPetroPromptTags.WEX_DriverLicenseState,new GBCPrompt(){WexCode = "6",TypeMask = "aaa",PromptText = "Driver License State"}},
                {EPetroPromptTags.WEX_DriverLicenseName,new GBCPrompt(){WexCode = "7",TypeMask = "aaaaaaaaaaaaaaaaaaaaaa",PromptText = "Driver License Name"}},
                {EPetroPromptTags.WEX_WorkOrder,new GBCPrompt(){WexCode = "8",TypeMask = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",PromptText = "Work Order / PO Number"}},
                {EPetroPromptTags.WEX_InvoiceNumber,new GBCPrompt(){WexCode = "9",TypeMask = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",PromptText = "Invoice Number"}},
                {EPetroPromptTags.WEX_TripNumber,new GBCPrompt(){WexCode = "A",TypeMask = "aaaaaaaaaaaaaaa",PromptText = "Trip Number"}},
                {EPetroPromptTags.WEX_UnitNumber,new GBCPrompt(){WexCode = "B",TypeMask = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",PromptText = "Unit Number"}},
                {EPetroPromptTags.WEX_TrailerHoursReferHours,new GBCPrompt(){WexCode = "C",TypeMask = "999999",PromptText = "Trailer Hours / Refer Hours"}},
                {EPetroPromptTags.WEX_DateofBirth,new GBCPrompt(){WexCode = "D",TypeMask = "99999999",PromptText = "Date of Birth"}},
                {EPetroPromptTags.WEX_ZIP,new GBCPrompt(){WexCode = "E",TypeMask = "aaaaaaaaa",PromptText = "Zip Code"}},
                {EPetroPromptTags.WEX_Data,new GBCPrompt(){WexCode = "F",TypeMask = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",PromptText = "Data"}},
                {EPetroPromptTags.WEX_EnteredData,new GBCPrompt(){WexCode = "G",TypeMask = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",PromptText = "Entered Data"}},
                {EPetroPromptTags.WEX_CashBackAmount,new GBCPrompt(){WexCode = "Q",TypeMask = "",PromptText = "Cash Back Amount"}}, //Not used. Debit is out of scope.
                {EPetroPromptTags.WEX_JobNumber,new GBCPrompt(){WexCode = "R",TypeMask = "aaaaaaaaaaaaaaa",PromptText = "Job Number"}},
                {EPetroPromptTags.WEX_Maintenance,new GBCPrompt(){WexCode = "S",TypeMask = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",PromptText = "Maintenance"}},
                {EPetroPromptTags.WEX_Department,new GBCPrompt(){WexCode = "T",TypeMask = "aaaaaaaaaaaaaaa",PromptText = "Department"}},
                {EPetroPromptTags.WEX_VIN,new GBCPrompt(){WexCode = "U",TypeMask = "aaaaaaaaaaaaaaaaa",PromptText = "VIN"}},
                {EPetroPromptTags.WEX_TractorNumber,new GBCPrompt(){WexCode = "V",TypeMask = "aaaaaaaaaaaaaaa",PromptText = "Tractor Number"}},
                {EPetroPromptTags.WEX_Hubometer,new GBCPrompt(){WexCode = "W",TypeMask = "999999999",PromptText = "Hubometer"}},
                {EPetroPromptTags.WEX_TrailerNumber,new GBCPrompt(){WexCode = "X",TypeMask = "aaaaaaaaaaaaaaa",PromptText = "Trailer Number"}},
                //Voyager
                {EPetroPromptTags.VOY_VoyagerID,new GBCPrompt(){WexCode = "",TypeMask = "999999",PromptText = "Voyager ID"}},
                {EPetroPromptTags.VOY_VehicleID,new GBCPrompt(){WexCode = "",TypeMask = "999999",PromptText = "Vehicle ID"}},
                {EPetroPromptTags.VOY_DriverID,new GBCPrompt(){WexCode = "",TypeMask = "999999",PromptText = "Driver ID"}},
                {EPetroPromptTags.VOY_Odometer,new GBCPrompt(){WexCode = "",TypeMask = "999999",PromptText = "Odometer ID"}},
                {EPetroPromptTags.VOY_TrailerID,new GBCPrompt(){WexCode = "",TypeMask = "aaaaaaaaaa",PromptText = "Trailer ID"}},
                {EPetroPromptTags.VOY_ReeferHours,new GBCPrompt(){WexCode = "",TypeMask = "9999999",PromptText = "Reefer Hours"}},
                {EPetroPromptTags.VOY_TripNumber,new GBCPrompt(){WexCode = "",TypeMask = "aaaaaaaaaa",PromptText = "Trip Number"}},
                {EPetroPromptTags.VOY_ExtendedDriverID,new GBCPrompt(){WexCode = "",TypeMask = "aaaaaaaaaaaaaaaaaaaa",PromptText = "Extended Driver ID"}},
                {EPetroPromptTags.VOY_UnitID,new GBCPrompt(){WexCode = "",TypeMask = "9999999999",PromptText = "Unit ID"}},
                {EPetroPromptTags.VOY_Contract_PO,new GBCPrompt(){WexCode = "",TypeMask = "aaaaaaaaaa",PromptText = "Contract / PO"}},
                {EPetroPromptTags.VOY_DriverLicense,new GBCPrompt(){WexCode = "",TypeMask = "aaaaaaaaaaaaaaaaaaaa",PromptText = "Driver License"}},
                {EPetroPromptTags.VOY_DriverLicenseState,new GBCPrompt(){WexCode = "",TypeMask = "AA",PromptText = "Driver License State"}},
                {EPetroPromptTags.VOY_VehicleLicense,new GBCPrompt(){WexCode = "",TypeMask = "aaaaaaaaaaaa",PromptText = "Vehicle License"}},
                {EPetroPromptTags.VOY_VehicleLicenseState,new GBCPrompt(){WexCode = "",TypeMask = "AA",PromptText = "Vehicle License State"}},
                {EPetroPromptTags.VOY_TrailerLicense,new GBCPrompt(){WexCode = "",TypeMask = "aaaaaaaaaaaa",PromptText = "Trailer License"}},
                {EPetroPromptTags.VOY_TrailerLicenseState,new GBCPrompt(){WexCode = "",TypeMask = "AA",PromptText = "Trailer License State"}},
                //PL
                {EPetroPromptTags.PL_DriverID,new GBCPrompt(){WexCode = "",TypeMask = "999999",PromptText = "Driver ID"}},
                {EPetroPromptTags.PL_VehicleID,new GBCPrompt(){WexCode = "",TypeMask = "99999999",PromptText = "Vehicle ID"}},
                {EPetroPromptTags.PL_Odometer,new GBCPrompt(){WexCode = "",TypeMask = "9999999",PromptText = "Odometer"}},
                //PF
                {EPetroPromptTags.PF_DriverID,new GBCPrompt(){WexCode = "",TypeMask = "99999999",PromptText = "Driver ID"}},
                {EPetroPromptTags.PF_VehicleID,new GBCPrompt(){WexCode = "",TypeMask = "99999999",PromptText = "Vehicle ID"}},
                {EPetroPromptTags.PF_Odometer,new GBCPrompt(){WexCode = "",TypeMask = "99999999",PromptText = "Odometer"}},
                //VF
                {EPetroPromptTags.VF_DriverID,new GBCPrompt(){WexCode = "",TypeMask = "990000",PromptText = "Driver ID"}},
                {EPetroPromptTags.VF_VehicleID,new GBCPrompt(){WexCode = "",TypeMask = "99999999",PromptText = "Vehicle ID"}},
                {EPetroPromptTags.VF_Odometer,new GBCPrompt(){WexCode = "",TypeMask = "9999999",PromptText = "Odometer"}},
                //FM
                {EPetroPromptTags.FM_DriverID,new GBCPrompt(){WexCode = "",TypeMask = "00000",PromptText = "Driver ID"}},
                {EPetroPromptTags.FM_ID_JobNumber,new GBCPrompt(){WexCode = "",TypeMask = "999999",PromptText = "ID / Job Number"}},
                {EPetroPromptTags.FM_Odometer,new GBCPrompt(){WexCode = "",TypeMask = "999999",PromptText = "Odometer"}},
                //P1
                {EPetroPromptTags.P1_CardHolderID,new GBCPrompt(){WexCode = "",TypeMask = "9999",PromptText = "Card Holder ID"}},
                //MF / VI
                {EPetroPromptTags.MFVI_DriverID,new GBCPrompt(){WexCode = "",TypeMask = "######",PromptText = "Driver ID Number"}},
                {EPetroPromptTags.MFVI_VehicleID,new GBCPrompt(){WexCode = "",TypeMask = "######",PromptText = "Vehicle ID"}},
                {EPetroPromptTags.MFVI_Odometer,new GBCPrompt(){WexCode = "",TypeMask = "999999",PromptText = "Odometer"}},
                //COMM
                {EPetroPromptTags.COM_UnitNumber, new GBCPrompt(){WexCode = "",TypeMask = "AAAAaaa",PromptText = "Unit Number"}},

                //        //Visa Fleet2
                {EPetroPromptTags.VF2_GenericID,new GBCPrompt(){WexCode = "",TypeMask = "999999",PromptText = "Generic ID"}},
                {EPetroPromptTags.VF2_WorkOrder,new GBCPrompt(){WexCode = "",TypeMask = "aaaaaaaaaaaaaaaaaaaaaaaaa",PromptText = "Work/Purchase order"}},
                {EPetroPromptTags.VF2_EmployeeNumber,new GBCPrompt(){WexCode = "",TypeMask = "aaaaaaaaaaaaaaaaaaaa",PromptText = "Employee Number"}},
                {EPetroPromptTags.VF2_TrailerNumbr,new GBCPrompt(){WexCode = "",TypeMask = "aaaaaaaaaaaaaaaaaaaa",PromptText = "Trailer Number"}},
                {EPetroPromptTags.VF2_AdditionalData1,new GBCPrompt(){WexCode = "",TypeMask = "aaaaaaaaaaaaaaaaaaaa",PromptText = "Additional Data 1"}},
                {EPetroPromptTags.VF2_AdditionalData2,new GBCPrompt(){WexCode = "",TypeMask = "aaaaaaaaaaaaaaaaaaaa",PromptText = "Additional Data 2"}},
                        
                //General
                {EPetroPromptTags.SoftwareVersion,new GBCPrompt(){WexCode = "",TypeMask = "CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC",PromptText = "Software Version"}},

                //ProductData
                {EPetroPromptTags.ProductDataPumpNumber,new GBCPrompt(){WexCode = "",TypeMask = "09",PromptText = "Pump Number"}},
                {EPetroPromptTags.ProductDataAmount,new GBCPrompt(){WexCode = "",TypeMask = "9999.999",PromptText = "Amount"}},
                {EPetroPromptTags.ProductDataUnitPrice,new GBCPrompt(){WexCode = "",TypeMask = "9999.999",PromptText = "Unit Price"}},
                {EPetroPromptTags.ProductDataProductCode,new GBCPrompt(){WexCode = "",TypeMask = "AAaaaa",PromptText = "Product Code"}},
                {EPetroPromptTags.ProductDataQuantityNBS,new GBCPrompt(){WexCode = "",TypeMask = "9999.99999",PromptText = "Data Quantity"}},
                {EPetroPromptTags.ProductGSTAmount,new GBCPrompt(){WexCode = "",TypeMask = "9999.999",PromptText = "GST Amount"}},
                {EPetroPromptTags.ProductPSTAmount,new GBCPrompt(){WexCode = "",TypeMask = "9999.999",PromptText = "PST Amount"}},
                {EPetroPromptTags.ProductName,new GBCPrompt(){WexCode = "",TypeMask = "AAAAAAAAAAAAAAA",PromptText = "Product Name"}},
                {EPetroPromptTags.ProductDataQuantityCFN,new GBCPrompt(){WexCode = "",TypeMask = "999999.999",PromptText = "Data Quantity"}},
                {EPetroPromptTags.ProductDataAmountChase,new GBCPrompt(){WexCode = "",TypeMask = "99999999.99",PromptText = "Amount (8.2)"}},
                {EPetroPromptTags.ProductDataFleetUniqueId,new GBCPrompt(){WexCode = "",TypeMask = "9999",PromptText = "Fleet Unique Id (4)"}},                

           };

    }

}
