using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
//using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.IO;
using System.Linq;
//using System.Security.Policy;
using System.Text;


namespace CCI.Globalcom.GlobalcomRetailProtocol
{
    /// <summary>
    /// Retail Protocol Commands
    /// 
    /// (This is currently used only for reference)
    /// </summary>
    public enum ERetailProtocolCommands
    {
        ExtendedCommand         = 0x00,
        InfoErase               = 0x01,
        FirmwareVersionRequest  = 0x02,
        TestEthernet            = 0x03,
        CardHash                = 0x04,
        PaymentStart            = 0x05,
        ReadCardEnable          = 0x06,
        CardReadDisable         = 0x07,
        CardDataRequest         = 0x08,
        StatusRequest           = 0x0A,
        LastOperationDataRequest = 0x0B,
        VoidTransaction         = 0x0C,
        PaymentOutcomeRequest   = 0x0D,
        RefundLastOperation     = 0x0E,
        ResultOfLastRefundOperation = 0x0F,
        TlsCaCertRequest        = 0x10,
        TlsClientCertRequest    = 0x11,
        DisplayAndKeyboardMessage = 0x12,
        RequestKeyboardInsertedData = 0x13,
        OutcomeVoidTransaction = 0x16,
        ReadTracksTypeRequest = 0x17,
        // LocalTotalsRequest = 0x18,  Depricated
        EnableWithPayment = 0x19,
        ActivateMainMenu = 0x20,
        GSMStatusRequest = 0x21,
        GetGatewayRefLastTranasction = 0x22,
        DeleteGatewayRefLastTransaction = 0x23,
        GetReceiptXML           = 0x1C,
        GetReceiptText          = 0x1D,
        TerminalReset           = 0x27,
        OfflineTransactionStatus = 0x30,
        SettlementOfflineTransaction = 0x31,
        SettlementOutcomeRequest = 0x32,
        DeleteOfflineTransaction = 0x33,
        ReaderPowerOff          = 0x39,
        FileSystemTypeRequest   = 0x40,
        PreAuthorization        = 0x48,
        PreAuthorizationOutcomeRequest = 0x49,
        Notification            = 0x4A,
        NotificationResult      = 0x4B,
        PreAuthorizationStatus  = 0x4C,
        DeleteNotification      = 0x4D,
        GetSlotDetails          = 0x51,
        GetNotify               = 0x52,
        ReadSerialNumber = 0x53,
        CommissioningGetCode     = 0x54,
        CommissioningUnlock     = 0x55,
        MasterGetInfo           = 0x56,
        NewDeviceGetCode        = 0x57,
        NewDevicePairing        = 0x58,
        AntiRemovalMasterViolationTime = 0x59,
        AntiRemovalEnable       = 0x60,
        GetDeviceInformation    = 0x61,
        SetTimeMaster           = 0x62,
        GetTimeMaster           = 0x63,
        AntiRemovalDeviceViolationTime = 0x64,
        GetTimeDevice           = 0x65,
        OneStepGetCode          = 0x66,
        OneStepUnlockCode       = 0x67,
        UpgradePackage          = 0x68,
        InstallPackage          = 0x69,
        GetUpgradeResult        = 0x6A,
        Device2GetInfo          = 0x6B,
        GetTimeDevice2          = 0x6C,
        GetKeysFingerprint      = 0x6D,
        SetLogType              = 0x70,
        DeleteLog               = 0x71,
        GetLog                  = 0x72,
        EnableStatistics        = 0x73,
        EraseStatistics         = 0x74,
        GetStatistics           = 0x75,
        TMSAddress              = 0x76,
        TMSStatus               = 0x77,
        TMSInstallAndReboot     = 0x78,
        LEDManage               = 0x79,
        JournalKeyWrites        = 0x80,
        CloseBatch              = 0x82,
        BatchOutcome            = 0x83,
        RetrieveTotals          = 0x84,
        BankingParametersConfiguration = 0xAD,
        IPConfigurationForRetailProtocolOnEthernet = 0xAE,
        // ConfigurationOfMdbParameters = 0xAF,  Depricated
        ReadBankingParamters    = 0xB0,
        ReadIPConfiguration     = 0xB1,
        // ReadTheConfigurationOfTheMdbParameters = 0xB2, Depricated
        ETH_handoff             = 0xC2,
        GetPCI5ParingCACert     = 0xC3,
        GetPCI5ParingCert       = 0xC4,
        ECR_PaymentStart        = 0xE0,
        ECR_PaymentOutcome      = 0xE1,
        ECR_PreAuthorization    = 0xE2,
        ECR_PreAuthorizationOutcome = 0xE3,
        ECR_Notification        = 0xE4,
        ECR_NotificationResult  = 0xE5,
        ECR_RefundLastOperation = 0xE6,
        ECR_ResultOfLastRefundOperation = 0xE7,
        ECR_SettlementOfflineTransaction = 0xE8,
        ECR_SettlementOutcome   = 0xE9,
        ECR_UnsolicitedMessage  = 0xEA,
        ECR_VoidTransaction     = 0xEB,
        ECR_OutcomeVoidTransaction = 0xEC,
        GetConfigurationFile = 0x1E
        
    }

    /// <summary>
    /// Extended Retail Protocol Commands
    /// </summary>
    public enum EExtRetailProtocolCommands
    {
        GetDeviceFamily = 0xD0,
        SetParmLevel2File = 0xAA
    }

    /// <summary>
    /// Types of filesystems for the BV1000 (only)
    /// </summary>
    public enum EFileSystemType
    {
        /// <summary>
        /// Chan is FAT32
        /// </summary>
        Chan = 0x00,
        /// <summary>
        /// HCC is journaled (secure) filesystem.
        /// </summary>
        HCC = 0x01
    }

    /// <summary>
    /// PCI Version of Hardware
    /// </summary>
    public enum EPCIVersion
    {
        PCI4 = 0x41,
        PCI5 = 0x51,
        PCI6 = 0x60
    }
    /// <summary>
    /// Device Family Type
    /// </summary>
    public enum EDeviceFamily
    {
        BV1000 = 0x00,
        HRT1000 = 0x01,
        PaymentDevice= 0xff
    }

    /// <summary>
    /// Enum of Pre Auth status responses (see RP 0x4C)
    /// </summary>
    public enum EPreAuthStatus
    {
        Empty = 0,
        Pre_Authorized = 1,
        Notified = 2,
        Data_Corrupted = 3
    }




    //public List<PetroProductDetailSyntax> PetroProductDetailSyntaxList;

    /// <summary>
    /// Enum for TCP communication Methods
    /// </summary>
    public enum ETls
    {
        None =      '0',
        SSLNoAuth=  '1',
        SSLAuth=    '2',
        TLSNoAuth=  '3',
        TLSAuth=    '4'
    }

    /// <summary>
    /// Enumeration of TMS Status Conditions
    /// </summary>
    public enum ETmsStatus
    {
        TmsIdle = 0x00,
        TmsConnection = 0x01,
        TmsDownload = 0x02,
        TmsWaitInstall = 0x03,
        TmsInstalling = 0x04
    }

    /// <summary>
    /// Enumeration for modes of Payment command
    /// </summary>
    public enum EMode
    {
        Online = 0x30,
        Offline = 0x31,
        OnlineWithBackup = 0x32
    }

    /// <summary>
    /// Enumeration of Lanaguage Selection
    /// </summary>
    public enum ELanguage
    {
        Italian = 0x01, //English and Italian reversed in G041
        English = 0x00,
        French = 0x02,
        German = 0x03,
        Spanish = 0x04,
        // Forced language selections are used when the user doesn't want the terminal language (presented on the pinpad) to follow
        // the language defined on the EMV card.
        English_Forced = 0x80,
        Italian_Forced = 0x81,
        French_Forced = 0x82,
        German_Forced = 0x83,
        Spanish_Forced = 0x84
    }

    /// <summary>
    /// Enumeration of the bank host selection options (tag 0x2E of banking params).
    /// </summary>
    public enum EBankHostSelection
    {
        unassigned = 0x00,
        Bank1 = '1',
        Bank2 = '2',
        Alternate = '3'
    }

    /// <summary>
    /// Enumeration of the Certificate Slot indexes
    /// </summary>
    public enum ECertSlot
    {
        unassigned = 0x00,  // Null is not really valid
        CertSlot0 = '0',  // Default of Not Used
        CertSlot1 = '1',
        CertSlot2 = '2',
        CertSlot3 = '3',
        CertSlot4 = '4',
        CertSlot5 = '5',
        CertSlot6 = '6',
        CertSlot7 = '7',
        CertSlot8 = '8',
        CertSlot9 = '9'
    }

    /// <summary>
    /// Enumeration of the Baud rates
    /// </summary>
    public enum EBaudRate
    {
        Baud1200 = '0',
        Baud2400 = '1',
        Baud4800 = '2',
        Baud9600 = '3',
        Baud19200 = '4',
        Baud38400 = '5',
        Baud57600 = '6',
        Baud115200 = '7',
        Baud1200FC = 'A',
        Baud2400FC = 'B',
        Baud4800FC = 'C',
        Baud9600FC = 'D',
        Baud19200FC = 'E',
        Baud38400FC = 'F',
        Baud57600FC = 'G',
        Baud115200FC = 'H',
    }

    /// <summary>
    /// Enumeration of the terminal ports
    /// </summary>
    public enum EPort
    {
        GPRS = 'G',
        MUX = 'M',
        USB = 'U',
        COM1 = '1',
        COM2 = '2',
        COM3 = '3',
        COM4 = '4',
        COM5 = '5',
        COM6 = '6',
        COM7 = '7',
        COM8 = '8',
        COM9 = '9',
        Ethernet = 'E'
    }

    /// <summary>
    /// Enum of ports when ETH isn't available.
    /// </summary>
    public enum EPortComOnly
    {
        GPRS = 'G',
        MUX = 'M',
        USB = 'U',
        COM1 = '1',
        COM2 = '2',
        COM3 = '3',
        COM4 = '4',
        COM5 = '5',
        COM6 = '6',
        COM7 = '7',
        COM8 = '8',
        COM9 = '9'
    }


    /// <summary>
    /// Enumeration of the Terminal Applications
    /// </summary>
    public enum EAppBinding
    {
        PaymentApp = 'A',
        RetailProtocol = 'R',
        MDB = 'M',
        MUX = 'X',
        GPRS = 'G',
        TestApp = 'T',
        Logging = 'L',
        Contactless = 'C',
        PinPad = 'I'
    }
    /// <summary>
    /// Enumeration of Currency Codes from ISO 4217
    /// </summary>
    public enum ECurrencies
    {
        USD = 840,
        CAD = 124,
        EUR = 978,
        GBP = 826
    }

    /// <summary>
    /// Enumeration of the insertion type
    /// for keypad entry requests
    /// <remarks> This is a bitmask</remarks>
    /// </summary>
    [FlagsAttribute]
    public enum EKeyRequestType
    {
        AllDigitsMandatory = 1,
        PasswordMasq = 2,
        AlphaNumericEntry = 4
    }

    /// <summary>
    /// Enumeration of Status byte 0
    /// <remarks> This is a bitmask</remarks>
    /// </summary>
    [FlagsAttribute]
    public enum EStatusB0
    {
        FutureUse = 1,
        OpenCash = 2,
        CardPresent = 4,
        CardReaderEnabled = 8,
        PreauthorizationOutcomeAvailable = 16,
        TransactionOutcomeAvailable = 32,
        NotifyOutcomeAvailable = 64,
        SettlementAvailable = 128
    }

    /// <summary>
    /// Enumeration of Status byte 1
    /// </summary>
    public enum EStatusB1
    {
        Idle = 0x00,
        FutureUse01 = 0x01,
        FutureUse02 = 0x02,
        FutureUse03 = 0x03,
        Busy = 0x04,
        CardInserted = 0x05,
        FutureUse06 = 0x06,
        PinEntry = 0x07,
        FutureUse08 = 0x08,
        FutureUse09 = 0x09,
        FutureUse10 = 0x10,
        FutureUse11 = 0x11,
        FutureUse12 = 0x12,
        KeyboardDataAvailable = 0x13,
        FutureUse14 = 0x14,
        HostTotalsAvailable = 0x15,
        CardDataAvailable = 0x16,
        ClosingOutcomeAvailable = 0x17,
        CancellationOutcomeAvailable = 0x18,
        FutureUse19 = 0x19,
        ExpiredTimeoutOfCardRead = 0x20,
        DllOutcomeAvailable = 0x21,
        VoidOutcomeAvailable = 0x22
    }

    /// <summary>
    /// Enumeration of Status byte 2
    /// <remarks>This is a bitmask</remarks>
    /// </summary>
    [FlagsAttribute]
    public enum EStatusB2
    {
        CardPresentAtGate = 1,
        VoidPending = 2,
        SensorViolation=4,
        AntiRemovalViolation=8,
        PairingViolation=16,
        LogEnabled=32,
        StatisticsEnabled=64,
        HostStatus=128
    }

    /// <summary>
    /// Enumeration of Status Byte 3
    /// </summary>
    public enum EStatusB3
    {
        Key0 = 0x30,
        Key1 = 0x31,
        Key2 = 0x32,
        Key3 = 0x33,
        Key4 = 0x34,
        Key5 = 0x35,
        Key6 = 0x36,
        Key7 = 0x37,
        Key8 = 0x38,
        Key9 = 0x39,
        Keyf = 0x46,
        KeyMenu = 0x66,
        Keycanc = 0x63,
        Keyclr =  0x64,
        Keyent =  0x0D,
        Keyperiod = 0x2E,
        Keyup =  0x75,
        KeyNONE = 0x00
    }

    /// <summary>
    /// Enumeration of Status Byte 4
    /// </summary>
    public enum EStatusB4
    {
        Idle = 0x00,
        InsertCard = 0x01,
        ExtractCard = 0x02,
        ConfirmAmount = 0x03,
        SelectApplication = 0x04,
        InsertPIN = 0x05,
        TransactionInProgress = 0x06,
        TransactionOutcomeGood = 0x07,
        TransactionOutcomeBad = 0x08,
        TransactionOutcomePartial = 0x09,
        TransactionOverCtlsFloorLimit = 0x0A,
        CheckPhoneAndTapAgain = 0x0B
    }


    /// <summary>
    /// Enumeration of BV Status Byte
    /// </summary>
    public enum EBvStatus
    {
        CorrectOutcome = 0xE0, 
        CommandNotRecognized = 0xE1,
        WrongPacketFormat = 0xE2,
        WrongChecksum = 0xE3,
        CommandNotAllowedInCurrentState = 0xE4,
        TracesNotAvailable = 0xE5,
        UptNotConfigured = 0xE6,
        ErrorOutcome = 0xE7,
        AlarmBoard = 0xE8,
        WrongLength = 0xE9,
        PacketNumberError = 0xEA
    }

    /// <summary>
    /// Enumeration of Upgrade Status Messages
    /// </summary>
    public enum EUpgradeStatus
    {
        Unknown = 0xFF,
        Ok = 0x00,
        Error = 0x01,
        NoFile = 0x02,
        ReadError = 0x03,
        WrongHeader = 0x04,
        ReadFlashError = 0x05,
        KccError = 0x06,
        AtmelError = 0x07,
        DecryptionError = 0x08,
        HexError = 0x09,
        PinPadError = 0x0A,
        ImError = 0x0B,
        SignatureError = 0x0C,
        I_EraseError = 0x0D,
        E_EraseError = 0x0E,
        Violation = 0x0F,
        AtmelUpgradeError = 0x10,
        CLUploadError=0x11
    }
    /// <summary>
    /// Enumeration of Sensor Status
    /// </summary>
    public enum ESensorStatus
    {
        NotActive = 0,
        Active = 1,
        Violated = 2,
    }

    /// <summary>
    /// Enumeration of Paring Status
    /// </summary>
    public enum EParingStatus
    {
        ToBePaired = 0,
        Paired = 1,
        Violated = 2,
    }

    /// <summary>
    /// Enumeration of MDB Cashless number 
    /// </summary>
    public enum ECashlessNumber
    {
        Cashless1 = 0x01,
        Cashless2 = 0x02,
    }

    /// <summary>
    /// Enumeration of MDB Sale Type
    /// </summary>
    public enum ESaleType
    {
        SingleSale = 0x01,
        MultipleSale = 0x02,
    }

    /// <summary>
    /// Enumeration of PPP status
    /// </summary>
    public enum EPPP
    {
        Disabled = 0x00,
        Enabled = 0x01,
    }

    /// <summary>
    /// Enumeration of Selection
    /// </summary>
    public enum ESelection
    {
        Unknown = 0x00,
        Multifrequency = 0x30,
        Decadic = 0x31,
    }

    /// <summary>
    /// Enumeration of Connection Type
    /// </summary>
    public enum EConnectionType
    {
        Unknown = 0x00,
        Modem = 0x30,
        Serial = 0x31,
        GSMAnalog = 0x32,
        GSMDigital = 0x33,
        GPRS = 0x34,
        Ethernet = 0x35,
    }
    /// <summary>
    /// Enumeration of Protocol connecting to the gateway/processor
    /// </summary>
    public enum EProtocol
    {
        Unknown = 0x00,
        None = 0x30,
        SslNoAuth = 0x34,
        SslAuth = 0x36,
    }

    /// <summary>
    /// Enumeration of the baud rate for the gateway/processor
    /// </summary>
    public enum ESpeed
    {
        Unknown = 0x00,
        Baud1200 = 0x30,
        Baud2400 = 0x31,
        Baud4800 = 0x32,
        Baud9600 = 0x33,
        Baud19200 = 0x34,
    }

    /// <summary>
    /// Enumeration of the personalize
    /// </summary>
    public enum EPersonalize
    {
        Unknown = 0x00,
        Default = 0x30,
        Triveneto = 0x31,
    }

    /// <summary>
    /// Enumeration of LAN type
    /// </summary>
    public enum ELanType
    {
        Unknown = 0x00,
        FixedIP = 0x30,
        DHCP = 0x31,
    }

    /// <summary>
    /// Enumeration of TypeBT
    /// </summary>
    public enum ETypeBT
    {
        Error = 0x00,
        Classic = 0x30,
        Header = 0x31,
    }

    /// <summary>
    /// Enumeration of DLL Line Data
    /// </summary>
    public enum EDllLineData
    {
        Error = 0x00,
        NO = 0x30,
        YES = 0x31,
    }

    /// <summary>
    /// Enumeration of the Outcome generated by each request
    /// </summary>
    public enum EOutcome
    {
        OK = 0xE0,
        UnrecognizedCommand = 0xE1,
        WrongPacketFormat = 0xE2,
        CommandNotPermitted = 0xE3,
        Busy = 0xE4,
        NoTracksAvailable = 0xE5,
        PartialApproval = 0xE6,
        ErrorOutcome = 0xE7,
        AlarmBoard = 0xE8,
        WrongLength = 0xE9,
        PacketNumberError=0xEA,
        PreAuthorizationSlotsFull = 0xEB,
        EncryptionKeyNotPresent = 0xEC,
        OperationAborted = 0xEE,
        TimeoutExpired = 0xED
    }

    /// <summary>
    /// Enumeration of the colors available for the LED
    /// </summary>
    public enum ELEDColor
    {
        undefined = 0x00,
        Blue = 0x10,
        Green = 0x20,
        Red = 0x40,
        Yellow = 0x60,
        Cyan = 0x30,
        Magenta = 0x50,
        White = 0x70
    }

    /// <summary>
    /// Enumeration of the available status options for the LED
    /// </summary>
    public enum ELEDStatus
    {
        Off = 0x00,
        On = 0x01,
        Blink = 0x55
    }

    /// <summary>
    /// Enumeration of the configuration files on the BV1000 Filesystem
    /// </summary>
    public enum EConfigFiles
    {
        TermConfig = 0x00,      // termconfig_dat 
        AID = 0x01,                 // applist_pp
        CommonAID = 0x02,            // applist_common
        DebitAID = 0x03,            // applist_debit 
        MAG_Config = 0x04,          // mag_config 
        EMV_Config  = 0x05,         // ConfigE1_pr 
        EMV_Dynamic_Config = 0x06,  // DynamicF1_pr 
        CAKeys_Files = 0x07,        // CAKeys_Files 
        LoadedFilesystem = 0x08,    // LoadedFS 
        Whitelist = 0x09,           // White_List 
        HRT_L2Params = 0x0A,        // HRTParam2 
        VisaFleet2 = 0x0B,          // VisaFleet2 
        Dynamic_Prompts = 0x17,     // dynamic_prompts 
        FS_List = 0x18,             // FS_List 
        FS_List_MD5 = 0x19,         // FS_List_MD5 
        Specific_File = 0x20,     
    }

    public enum ETLVTags : long
    {
        interface_device_serial_number = 0x9f1e,
        terminal_identificatio = 0x9f1c,
        transaction_currency_code =  0x5f2a,
        terminal_AID = 0x9f06,
        application_version_number = 0x9f09,
        Terminal_floor_limit = 0x9F1B,
        CA_Public_key_index = 0x9f22,
        terminal_capabilities = 0x9f33,
        POS_entry_mode = 0x9f39,
        Magstripe_application_version = 0x9f6d,
        terminal_country_code = 0x9f1a,
        terminal_type = 0x9f35,
        terminal_languages_supported = 0xdf10,
        target_percentage_for_random_transaction_selection = 0xdf18, 
        maximum_target_percentage_for_random_transaction_selection = 0xdf19,
        threshold_value_for_biased_random_selection = 0xdf17,
        tac_denial = 0xdf14, 
        tac_online = 0xdf15, 
        tac_default = 0xdf13,
        default_DDOL = 0xdf25, 
        //UDKmac = 0xdf43,
        terminal_floor_limit = 0x9f1b, 
        additional_terminal_capabilities = 0x9f40, 
        online_DOL = 0xdf30, 
        transaction_currency_exponent = 0x5f36, 
        language = 0xdf10, 
        call_your_bank_message = 0xdf43,
        Transaction_Certificate_Data_Object_List_TDOL = 0x97,
        Merchant_Category_Code_MCC = 0x9F15,
        Merchant_Identifier = 0x9F16,
        Terminal_Country_Code = 0x9F1A,
        Terminal_Floor_Limit = 0x9F1B,
        Terminal_Identification = 0x9F1C,
        Terminal_Risk_Management_Data = 0x9F1D,
        Default_UDOL = 0xdf811a,
        Contactless_Floor_Limit = 0x9F92810F,
        Contactless_CVM_Required_Limit = 0x9F92810E,
        Contactless_Transaction_Limit__No_On_Device = 0x9F92810D,
        Contactless_Transaction_Limit_On_Device = 0xDF8125,
        Kernel_ID = 0x9F928101,
        EntryPoint_Options__Partial_AID_and_Zero_amount_forbiden = 0x9F928100,
        Capabilities_CVM_Required = 0x9F918504,
        Capabilities_no_CVM_Required = 0x9F918505,
        MagStripe_Capabilities_CVM_Required = 0xDF811E,
        MagStripe_Capabilities_no_CVM_Required = 0xDF812C,
        Kernel_Configuration = 0xDF811B,
        TAC_Default = 0x9F918709,
        TAC_Denial = 0x9F91870A,
        TAC_Online = 0x9F91870B,
        Terminal_Risk_Management = 0x9F1D,
        Reader_Contactless_Transaction_Limit = 0x9F92810D,
        Reader_Contactless_CVM_Required_Limit = 0x9F92810E,
        Reader_Contactless_Floor_Limit  = 0x9F92810F,
        Status_check = 0x5F828103,
        Zero_amount_allowed = 0x5F828104,
        Extended_selection_supported = 0x5F82810A,
        Terminal_Transaction_Qualifiers = 0x9F66,
        Program_ID = 0x9F5A,
        CRL = 0x9F91841F,

        // TAG ICS Parameters
        Card_Detection_Type = 0x9F928210,
        Card_Detection_Timeout = 0x9F928212,
        Number_of_Cards_to_Detect = 0x9F928214,
        Acquirer_Identifier = 0x9F01,
        Merchant_Name_and_Location = 0x9F4E,
        Mobile_Support_Indicator = 0x9F7E,
        Account_Type = 0x5F57,
        Terminal_Entry_Capability = 0x5F828201,
        DRL_support = 0x5F828203,

        // Kernel Subset Parameters
        Card_Data_Input_Capability = 0xDF8117,
        Security_Capability = 0xDF811F,
        Max_Lifetime_of_Torn_Transaction_Log_Record = 0xDF811C,
        Max_Number_of_Torn_Transaction_Log_Records = 0xDF811D,
        DS_AC_Type = 0xDF8108,
        DS_Input_Card = 0xDF60,
        DS_Input_Term = 0xDF8109,
        DS_ODS_Info = 0xDF62,
        DS_ODS_Info_For_Reader = 0xDF810A,
        DS_ODS_Term = 0xDF63,
        DSVN_Term = 0xDF810D,

        contactless_receipt_required_limit = 0x9F5D,

        Unknown_Tag = 0x00
    }

/// <summary>
/// Base Class for parsing Retail Protocol Responses
/// Many Specific messages use Response classes inherited from this base Class.
/// </summary>
public class RpResponseBase
    {
        //ASCII
        public const byte STX = 0x02;
        public const byte ETX = 0x03;

        //Object Properties
        public byte Checksum;
        public bool ChecksumIsValid;
        public byte CommandId;
        public byte[] Info;
        public short InfoLength; //this value is the info length plus 1 byte for the outcome
        public byte Outcome;
        public byte[] Response;
        public byte UptAddress;

        /// <summary>
        /// Response Object Creator
        /// </summary>
        /// <param name="response">Byte Array generated from the Terminal after a request</param>
        public RpResponseBase(byte[] response)
        {
            Response = response;
            //stx = Response[0];
            UptAddress = response[1];
            CommandId = response[2];
            InfoLength = EndianBitConverter.Big.ToInt16(response, 3); //make a 16 bit int from byte 3 and 4 
            if (response.Length < (6 + InfoLength)) { throw new Exception("Data too short for a response packet.");}
            Info = new byte[InfoLength - 1]; //because outcome is parsed by itself
            
            Outcome = response[5];
            Buffer.BlockCopy(response, 6, Info, 0, (InfoLength - 1) ); // no //We parse the outcome alone, so we subtract 1
            Checksum = response[InfoLength + 6];
            ChecksumIsValid = VerifyChecksum();
            if (!ChecksumIsValid)
            {
               string responseString = Encoding.ASCII.GetString(response);
               throw new Exception("Checksum Failure in response message."+Environment.NewLine+"Data:"+Environment.NewLine+responseString);
            }

            VerifyOutcome();
        }

        //Method used to return the Response Object's outcome
        public EOutcome VerifyOutcome()
        {
            switch (Outcome)
            {
                case 0xE0:
                    // good
                    return EOutcome.OK;
                case 0xE1:
                    return EOutcome.UnrecognizedCommand;
                case 0xE2:
                    return EOutcome.WrongPacketFormat;
                case 0xE3:
                    return EOutcome.CommandNotPermitted;
                case 0xE4:
                    return EOutcome.Busy;
                case 0xE5:
                    return EOutcome.NoTracksAvailable;
                case 0xE6:
                    return EOutcome.PartialApproval;
                case 0xE7:
                    return EOutcome.ErrorOutcome;
                case 0xE8:
                    return EOutcome.AlarmBoard;
                case 0xE9:
                    return EOutcome.WrongLength;
                case 0xEA:
                    return EOutcome.PacketNumberError;
                case 0xEB:
                    return EOutcome.PreAuthorizationSlotsFull;
                case 0xEC:
                    return EOutcome.EncryptionKeyNotPresent;
                case 0xED:
                    return EOutcome.TimeoutExpired;
                case 0xEE:
                    return EOutcome.OperationAborted;
            }

            return EOutcome.ErrorOutcome;
        }

        //Method to Verify the checksum of the response
        public bool VerifyChecksum()
        {
            byte computeCheckSum = 0x7F;

            for (var i = 0; i <= (InfoLength+5); i++)
            {
                computeCheckSum = (byte) (computeCheckSum ^ Response[i]);
            }
            return (computeCheckSum == Checksum);
        }

        /// <summary>
        /// Method to check if a bit is set in a byte (bits 0-7)
        /// </summary>
        /// <param name="b">Byte</param>
        /// <param name="pos">Position</param>
        /// <returns></returns>
        public bool IsBitSet(byte b, int pos)
        {
            return (b & (1 << pos)) != 0;
        }

        /// <summary>
        /// Another Method to check if a bit is set in a byte (bits 1-8)
        /// </summary>
        /// <param name="b">Byte</param>
        /// <param name="pos">Position</param>
        /// <returns></returns>
        public bool IsBitSetAtPos(byte b, int pos)
        {
            pos--;
            return (b & (1 << pos)) != 0;
        }

    }




    /// <summary>
    /// Response Class for Status Requests
    /// </summary>
    public class RpResponseStatus : RpResponseBase
    {
        public EStatusB0 EBlock0;
        public EStatusB1 EBlock1;
        public EStatusB2 EBlock2;
        public EStatusB3? EBlock3; //sometimes this guy is null
        public EStatusB4? EBlock4; //sometimes this guy is also null
        

        public byte Block0;
        public byte Block1;
        public byte Block2;
        public byte? Block3;
        public byte? Block4;

        public RpResponseStatus(byte[] response)
            : base(response)
        {
            //if (VerifyOutcome() == EOutcome.OK) //busy,etc. is ok too - Comment this out.
            {

                if (response[2] != 0x0A) //Sanity check for garbage
                {
                    StringBuilder displayText = new StringBuilder();
                    foreach (var hexchar in response)
                    {
                        displayText.Append(hexchar.ToString("X2") + " ");
                    }
                    throw new Exception("Illegal Status Retail Protocol Packet. Packet"+displayText);
                }
                if (VerifyOutcome() == EOutcome.UnrecognizedCommand)
                {
                    //BV1000K 0x0A just returns 0xE1
                    return;
                }
                //EBlock0 = (EStatusB0)response[6];
                EBlock0 = (EStatusB0) Enum.ToObject(typeof(EStatusB0), response[6]);
                Block0 = response[6];
                EBlock1 = (EStatusB1)response[7];
                Block1 = response[7];
                EBlock2 = (EStatusB2)response[8];
                Block2 = response[8];
                if (5 <= InfoLength)
                {
                    EBlock3 = (EStatusB3)response[9];
                    Block3 = response[9];
                }
                else
                {
                    EBlock3 = null;
                    Block3 = null;
                }
                if (6 <= InfoLength)
                {
                    EBlock4 = (EStatusB4) response[10];
                    Block4 = response[10];
                }
                else
                {
                    EBlock4 = null;
                    Block4 = null;
                }
            }
        }

        public bool IsIdle()
        {
            if ((EStatusB1)Block1 != EStatusB1.Idle)
                return false;

            if (Block0 != 0x00)
                return false;

            if (Block2 != 0x00)
                return false;

            return true;
        }

        public string B0Status()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(EBlock0.HasFlag(EStatusB0.CardPresent) ? "CardPresent " : "");
            sb.Append(EBlock0.HasFlag(EStatusB0.CardReaderEnabled) ? "ReaderEnabled " : "");
            sb.Append(EBlock0.HasFlag(EStatusB0.PreauthorizationOutcomeAvailable) ? "PreauthOutcome" : "");
            sb.Append(EBlock0.HasFlag(EStatusB0.TransactionOutcomeAvailable) ? "TranOutcome" : "");
            sb.Append(EBlock0.HasFlag(EStatusB0.NotifyOutcomeAvailable) ? "NotifyOutcome" : "");
            sb.Append(EBlock0.HasFlag(EStatusB0.SettlementAvailable) ? "SettleOutcome" : "");

            return sb.ToString();
        }

        public string B1Status()
        {
            return EBlock1.ToString();
        }

        public string B2Status()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(EBlock2.HasFlag(EStatusB2.CardPresentAtGate) ? "CardAtGate " : "");
            sb.Append(EBlock2.HasFlag(EStatusB2.VoidPending) ? "VoidPending " : "");
            sb.Append(EBlock2.HasFlag(EStatusB2.SensorViolation) ? "SensorViolation " : "");
            sb.Append(EBlock2.HasFlag(EStatusB2.AntiRemovalViolation) ? "AntiRemovalViolation" : "");
            sb.Append(EBlock2.HasFlag(EStatusB2.PairingViolation) ? "PairingViolation" : "");

            return sb.ToString();
        }


        public string ToXmlString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(EBlock0.HasFlag(EStatusB0.FutureUse) ? "<FutureUse>" : "");
            sb.Append(EBlock0.HasFlag(EStatusB0.OpenCash) ? "<OpenCash>" : "");
            sb.Append(EBlock0.HasFlag(EStatusB0.CardPresent) ? "<CardPresent>" : "");
            sb.Append(EBlock0.HasFlag(EStatusB0.CardReaderEnabled) ? "<CardReaderEnabled>" : "");
            sb.Append(EBlock0.HasFlag(EStatusB0.PreauthorizationOutcomeAvailable)
                ? "<PreauthorizationOutcomeAvailable>"
                : "");
            sb.Append(EBlock0.HasFlag(EStatusB0.TransactionOutcomeAvailable)
                ? "<TransactionOutcomeAvailable>"
                : "");
            sb.Append(EBlock0.HasFlag(EStatusB0.NotifyOutcomeAvailable) ? "<NotifyOutcomeAvailable>" : "");
            sb.Append(EBlock0.HasFlag(EStatusB0.SettlementAvailable) ? "<SettlementAvailable>" : "");

            if (EBlock1 == EStatusB1.ExpiredTimeoutOfCardRead)
                sb.Append("<ExpiredTimeoutOrBadRead>");
            else
                sb.Append("<" + EBlock1.ToString() + ">");

            sb.Append(EBlock2.HasFlag(EStatusB2.CardPresentAtGate) ? "<CardPresentAtGate>" : "");
            sb.Append(EBlock2.HasFlag(EStatusB2.VoidPending) ? "<VoidPending>" : "");
            sb.Append(EBlock2.HasFlag(EStatusB2.SensorViolation) ? "<SensorViolation>" : "");
            sb.Append(EBlock2.HasFlag(EStatusB2.AntiRemovalViolation) ? "<AntiRemovalViolation>" : "");
            sb.Append(EBlock2.HasFlag(EStatusB2.PairingViolation) ? "<PairingViolation>" : "");
            sb.Append(EBlock2.HasFlag(EStatusB2.LogEnabled) ? "<LogEnabled>" : "");
            sb.Append(EBlock2.HasFlag(EStatusB2.StatisticsEnabled) ? "<StatisticsEnabled>" : "");
            sb.Append(EBlock2.HasFlag(EStatusB2.HostStatus) ? "<HostStatus>" : "");

            sb.Append("<StatusMsg-" + EBlock4.ToString() + ">");
            return sb.ToString();
        }
    }

    /// <summary>
    /// Response Class for Notify Outcome
    /// </summary>
    public class RpResponseNotifyOutcome : RpResponseBase
    {
        public byte[] ErrorCode;

        public RpResponseNotifyOutcome(byte[] response)
            : base(response)
        {
            ErrorCode = new byte[4];
            Buffer.BlockCopy(Info, 0, ErrorCode, 0, 4);
        }
    }

    /// <summary>
    /// Class to parse the Gateway Reference from a response
    /// </summary>
    public class RpResponseGatewayReference : RpResponseBase
    {
        //Reference number/ID
        public string GatewayReference;

        public RpResponseGatewayReference(byte[] response) : base(response)
        {
            if (InfoLength > 1)
            {
                GatewayReference = Encoding.UTF8.GetString(Info, 0, (InfoLength - 1));
            }
        }
    }

    /// <summary>
    /// Response class to key erase command.
    /// This class parses the random number returned.
    /// </summary>
    public class RpResponseEraseKeys : RpResponseBase
    {
        public byte[] RandomNumber;

        public RpResponseEraseKeys(byte[] response) : base(response)
        {
            if (InfoLength > 1)
            {
                RandomNumber = new byte[4];
                Buffer.BlockCopy(Info, 0, RandomNumber, 0, 4);
            }

        }
    }


    
    
    /// <summary>
    /// Response class for Authorization Status (slot) requests
    /// </summary>
    public class RpResponseAuthStatus : RpResponseBase
    {
        public List<byte> Slot = new List<byte>();

        public RpResponseAuthStatus(byte[] response)
            : base(response)
        {
            if (CommandId != 0x4C)
                return;

            if (VerifyOutcome() == EOutcome.OK)
            {
                for (int i = 0; i < 10;  i++)
                {
                    Slot.Add(Info[i]); // info[0] is outcome
                }
            }
        }

        public string PrintDetails()
        {
            StringBuilder sb = new StringBuilder();

            if (VerifyOutcome() == EOutcome.OK)
            {
                sb.AppendLine("PreAuth Status:");

                int slotsAvailable = 0;
                for (int slotNumber = 1; slotNumber <= (Slot.Count); slotNumber++)
                {
                    EPreAuthStatus slotStat = (EPreAuthStatus)Slot[(slotNumber - 1)];

                    sb.AppendLine("Slot " + slotNumber.ToString() + ":" + slotStat.ToString());
                    if (slotStat == EPreAuthStatus.Empty)
                        slotsAvailable++;
                }

                sb.AppendLine("You have " + slotsAvailable.ToString() + " slots available for new pre-authorizations." + Environment.NewLine);
            }
            else
                sb.AppendLine("Unable to retrieve PreAuthorization Slot Status. \nOutcome:" + VerifyOutcome());

            return sb.ToString();
        }

        public string PrintDetailsAsXml(bool bOnOneLine = false)
        {
            StringBuilder sb = new StringBuilder();

            if (VerifyOutcome() == EOutcome.OK)
            {
                int slotsAvailable = 0;
                for (int slotNumber = 1; slotNumber <= (Slot.Count); slotNumber++)
                {
                    EPreAuthStatus slotStat = (EPreAuthStatus)Slot[(slotNumber - 1)];

                    if (bOnOneLine)
                        sb.Append(" <Slot" + slotNumber.ToString() + ">" + slotStat.ToString());
                    else
                        sb.AppendLine("<Slot" + slotNumber.ToString() + ">" + slotStat.ToString());
                    if (slotStat == EPreAuthStatus.Empty)
                        slotsAvailable++;
                }
            }

            return sb.ToString();
        }
    }

    /// <summary>
    /// Response class for the print receipt
    /// </summary>
    public class RpResponseReceipt : RpResponseBase
    {
        public string FullReceipt;
        public int ReceiptLength;

        public RpResponseReceipt(byte[] response) : base(response)
        {
            if (InfoLength > 1)
            {
                ReceiptLength = EndianBitConverter.Big.ToInt16(Info, 0);
                FullReceipt = Encoding.UTF8.GetString(Info, 2, InfoLength - 3);
            }
            else
            {
                ReceiptLength = 0;
                FullReceipt = "";
            }
        }
    }


    /// <summary>
    /// Response class for the Statistics
    /// </summary>
    public class RpResponseStatistics : RpResponseBase
    {
        public string Log;
        public List<int> Counters;

        public RpResponseStatistics(byte[] response) : base(response)
        {
            Counters = new List<int>();
            if (InfoLength < 5) return;

            for (int i = 0; i < (InfoLength - 1); i += 4)
            {
                Counters.Add(EndianBitConverter.Big.ToInt32(Info,i));
            }
        }
    }

    /// <summary>
    /// Response class for parsing files for AID lists
    /// </summary>
    public class RpResponseAIDList : RpResponseBase
    {
        public Dictionary<string, string> AIDList;
        public RpResponseAIDList(byte[] response) : base(response)
        {
            string FullAscii = Encoding.ASCII.GetString(Info).Replace("\r\n", "");

            AIDList = new Dictionary<string, string>();

            for (int i = 0; i < (FullAscii.Length - 3); i += 36)
            {
                int aIDLength;
                
                if (!(Int32.TryParse(FullAscii.Substring(i,2), System.Globalization.NumberStyles.HexNumber, CultureInfo.CurrentCulture, out aIDLength))) throw new Exception("Unable to parse AID Length. :"+ FullAscii.Substring(i, 2)) ;

                string aid = FullAscii.Substring(i + 2, (aIDLength * 2));
                string desc = LookupAID(aid);
                if (desc != null)
                    aid += "(" + desc +")";
                AIDList.Add(aid, FullAscii.Substring((i+34),2));
            }
        }
        
        static public string LookupAID(string aid)
        {
            if (aid.Contains("A0000000032010"))
                return "Visa Electron";
            else if (aid.Contains("A0000000031010"))
                return "Visa";
            else if (aid.Contains("A0000000041010"))
                return "Mastercard";
            else if (aid.Contains("A000000025"))
                return "Amex";
            else if (aid.Contains("A0000001523010"))
                return "Discover";
            else if (aid.Contains("A00000006510"))
                return "JCB";
            else if (aid.Contains("A0000002771010"))
                return "Interac";
            return null;
        }

        public string PrintDetails()
        {
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, string> entry in AIDList)
            {
                sb.AppendLine(entry.Key + ":" + entry.Value);
            }

            return sb.ToString();
        }
    }

    /// <summary>
    /// Response class for parsing the mag config file mag_config.txt from the terminal's filesystem
    /// </summary>
    public class RpResponseMagConfig : RpResponseBase
    {
        /// <summary>
        /// Data structure to hold schema/data
        /// </summary>
        public List<Tuple<string, string>> SchemeList;
        public int SchemeCount;
        public string FullConfig;

        public RpResponseMagConfig(byte[] response) : base(response)
        {
            SchemeList = new List<Tuple<string, string>>();
            FullConfig = Encoding.ASCII.GetString(Info, 0, (InfoLength-1));

            string[] lines = FullConfig.Split(new string[] {"\r\n", "\n"}, StringSplitOptions.None);

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Length < 1) break;
                if (i == 0)
                {
                    Int32.TryParse(lines[0], out SchemeCount);
                    continue;
                }
                if ((i % 2) != 0)
                {
                    SchemeList.Add(new Tuple<string, string>(lines[i], lines[++i]));
                }
            }
        }
        public string PrintDetails()
        {
            StringBuilder sb = new StringBuilder();
            foreach (Tuple<string, string> entry in SchemeList)
            {
                sb.AppendLine(entry.Item1 + ":" + entry.Item2);
            }

            return sb.ToString();
        }
    }

    /// <summary>
    /// Response class for parsing the mag config file mag_config.txt from the terminal's filesystem
    /// </summary>
    public class RpResponseWhiteList : RpResponseBase
    {
        public List<string> WhiteListList;
        public string FullConfig;

        public RpResponseWhiteList(byte[] response) : base(response)
        {
            FullConfig = Encoding.ASCII.GetString(Info, 0, (InfoLength - 1));

            WhiteListList = FullConfig.Split(new string[] { "\r\n", "\n", "#" }, StringSplitOptions.None).ToList();
        }

        public string PrintDetails()
        {
            return FullConfig;
        }
    }




    /// <summary>
    /// Class for parsing the list of CA files
    /// </summary>
    public class RpResponseCAKeyList : RpResponseBase
    {
        public List<string> CAKeyList;
        public string FullList;

        public RpResponseCAKeyList(byte[] response) : base(response)
        {
            CAKeyList = new List<string>();
            FullList = Encoding.ASCII.GetString(Info, 0, (InfoLength - 1));

            string[] lines = FullList.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Length < 1) break;
                CAKeyList.Add(lines[i]);
            }
        }

        public string PrintDetails()
        {
            return FullList;
        }
    }

    public class RpResponseTLVData : RpResponseBase
    {
        //Key value pair for TLV data
        public Dictionary<string,string> TLVList;
        public string FullASCII;

        public RpResponseTLVData(byte[] response) : base(response)
        {
            TLVList = new Dictionary<string, string>();
            FullASCII = Encoding.ASCII.GetString(Info, 0, (InfoLength - 1)).TrimEnd('\n');
            
            for (int gIndex = 0; gIndex < (FullASCII.Length -1); gIndex++)
            {
                string sTagLength;
                if (checkTLVFirstByte(FullASCII.Substring(gIndex, 2)))
                {
                    sTagLength = FullASCII.Substring(gIndex + 4, 2);
                    int tagLength = 0;
                    if (!(Int32.TryParse(sTagLength, System.Globalization.NumberStyles.HexNumber,
                        CultureInfo.CurrentCulture, out tagLength)))
                        throw new Exception("Unable to parse TLV Length to Int. Value:" + sTagLength);
                    TLVList.Add(FullASCII.Substring(gIndex, 4), FullASCII.Substring(gIndex + 6, (tagLength * 2)));
                    gIndex += 5 + (tagLength * 2);
                }
                else
                {
                    sTagLength = FullASCII.Substring(gIndex + 2, 2);
                    int tagLength = 0;
                    if (!(Int32.TryParse(sTagLength, System.Globalization.NumberStyles.HexNumber,
                        CultureInfo.CurrentCulture, out tagLength)))
                        throw new Exception("Unable to parse TLV Length to Int. Value:" + sTagLength);

                    TLVList.Add(FullASCII.Substring(gIndex, 2), FullASCII.Substring(gIndex + 4, (tagLength * 2)));
                    gIndex += 3 + (tagLength * 2);
                }

            }
        }

        public string PrintDetails()
        {
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, string> entry in TLVList)
            {
                sb.AppendLine(entry.Key + ":" + entry.Value);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Function to determine if the first TAG byte of BER-TLV Data indicates a 2nd tag byte follows
        /// </summary>
        /// <param name="stestByte"></param>
        /// <returns>bool </returns>
        public bool checkTLVFirstByte(string stestByte)
        {
            byte testByte = byte.Parse(stestByte, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            return (IsBitSet(testByte, 4) && IsBitSet(testByte, 3) && IsBitSet(testByte, 2) &&
                    IsBitSet(testByte, 1) && IsBitSet(testByte, 0));

        }
    }

    public class RpResponseDynamicTLVData : RpResponseBase
    {
        public Dictionary<string, string> GlobalTLVOverwrite;
        public Dictionary<string, string> AIDlist;
        public List<Tuple<string, string, string>> TLVOverwriteByAID;

        public string FullASCII;

        public RpResponseDynamicTLVData(byte[] response) : base(response)
        {
            GlobalTLVOverwrite = new Dictionary<string, string>();
            AIDlist = new Dictionary<string, string>();
            TLVOverwriteByAID = new List<Tuple<string, string, string>>();

            FullASCII = Encoding.ASCII.GetString(Info, 0, (InfoLength - 1));

            string[] lines = FullASCII.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("#"))
                {
                    string sTLVData = lines[i].Substring(4);
                    if (lines[i].StartsWith("#AIDLIST")) continue;
                    //Global tag
                    for (int gIndex = 0; gIndex < (sTLVData.Length - 1); gIndex++)
                    {

                        string sTagLength;
                        if (checkTLVFirstByte(sTLVData.Substring(gIndex, 2)))
                        {
                            sTagLength = sTLVData.Substring(gIndex + 4, 2);
                            int tagLength = 0;
                            if (!(Int32.TryParse(sTagLength, System.Globalization.NumberStyles.HexNumber,
                                CultureInfo.CurrentCulture, out tagLength)))
                                throw new Exception("Unable to parse TLV Length to Int. Value:" + sTagLength);
                            GlobalTLVOverwrite.Add(sTLVData.Substring(gIndex, 4), sTLVData.Substring(gIndex + 6, (tagLength * 2)));
                            gIndex += 5 + (tagLength * 2);
                        }
                        else
                        {
                            sTagLength = sTLVData.Substring(gIndex + 2, 2);
                            int tagLength = 0;
                            if (!(Int32.TryParse(sTagLength, System.Globalization.NumberStyles.HexNumber,
                                CultureInfo.CurrentCulture, out tagLength)))
                                throw new Exception("Unable to parse TLV Length to Int. Value:" + sTagLength);

                            GlobalTLVOverwrite.Add(sTLVData.Substring(gIndex, 2), sTLVData.Substring(gIndex + 4, (tagLength * 2)));
                            gIndex += 3 + (tagLength * 2);
                        }


                    }
                    continue;
                }
                if (lines[i].StartsWith("A"))
                {
                    //AID List
                    int delimpos = lines[i].IndexOf('#');
                    if (delimpos < 0)
                        throw new Exception("Unable to parse TLV Data " + lines[i]);

                    string sAIDIndex = lines[i].Substring(1, delimpos - 1);
                    string sTagLength = lines[i].Substring(delimpos + 1, 2);
                    int tagLength = 0;
                    if (!(Int32.TryParse(sTagLength, System.Globalization.NumberStyles.HexNumber,
                        CultureInfo.CurrentCulture, out tagLength)))
                        throw new Exception("Unable to parse TLV Length to Int. Value:" + sTagLength);
                    AIDlist.Add(sAIDIndex, lines[i].Substring(delimpos + 3, tagLength*2));
                    continue;
                }
                if (lines[i].StartsWith("T"))
                {
                    //T overwrite list
                    int delimpos = lines[i].IndexOf('#');
                    string sAIDIndex = lines[i].Substring(1, delimpos - 1);
                    string sTLVData = lines[i].Substring(delimpos + 1);

                    for (int gIndex = 0; gIndex < (sTLVData.Length - 1); gIndex++)
                    {
                        string sTagLength;
                        if (checkTLVFirstByte(sTLVData.Substring(gIndex, 2)))
                        {
                            sTagLength = sTLVData.Substring(gIndex + 4, 2);
                            int tagLength = 0;
                            if (!(Int32.TryParse(sTagLength, System.Globalization.NumberStyles.HexNumber,
                                CultureInfo.CurrentCulture, out tagLength)))
                                throw new Exception("Unable to parse TLV Length to Int. Value:" + sTagLength);

                            if (sTLVData.Length < (gIndex + 6 + (tagLength * 2)))
                                TLVOverwriteByAID.Add(new Tuple<string, string, string>(AIDlist[sAIDIndex], sTLVData.Substring(gIndex, 4), "Error..Insufficient data"));
                            else
                                TLVOverwriteByAID.Add(new Tuple<string, string, string>(AIDlist[sAIDIndex], sTLVData.Substring(gIndex, 4), sTLVData.Substring(gIndex + 6, (tagLength * 2))));
 
                            gIndex += 5 + (tagLength * 2);
                        }
                        else
                        {
                            sTagLength = sTLVData.Substring(gIndex + 2, 2);
                            int tagLength = 0;
                            if (!(Int32.TryParse(sTagLength, System.Globalization.NumberStyles.HexNumber,
                                CultureInfo.CurrentCulture, out tagLength)))
                                throw new Exception("Unable to parse TLV Length to Int. Value:" + sTagLength);

                            TLVOverwriteByAID.Add(new Tuple<string, string, string>(AIDlist[sAIDIndex], sTLVData.Substring(gIndex, 2), sTLVData.Substring(gIndex + 4, (tagLength * 2))));
                            gIndex += 3 + (tagLength * 2);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Function to determine if the first TAG byte of BER-TLV Data indicates a 2nd tag byte follows
        /// </summary>
        /// <param name="stestByte"></param>
        /// <returns>bool </returns>
        public bool checkTLVFirstByte(string stestByte)
        {
            byte testByte = byte.Parse(stestByte, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            return (IsBitSet(testByte, 4) && IsBitSet(testByte, 3) && IsBitSet(testByte, 2) &&
                    IsBitSet(testByte, 1) && IsBitSet(testByte, 0));

        }
    }
    

    /// <summary>
    /// Response class for GPIO Status
    /// </summary>
    public class RpResponseGPIO : RpResponseBase
    {
        public bool In1;
        public bool In2;

        public RpResponseGPIO(byte[] response)
            : base(response)
        {
            if (VerifyOutcome() == EOutcome.OK)
            {
                In1 = IsBitSet(response[6], 0);
                In2 = IsBitSet(response[6], 1);
            }
        }

    }

    /// <summary>
    /// Response class for Upgrade Result Messages
    /// </summary>
    public class RpResponseUpgradeResult : RpResponseBase
    {
        public EUpgradeStatus MasterStatus;
        public EUpgradeStatus DeviceStatus;
        public EUpgradeStatus Device2Status;

        public RpResponseUpgradeResult(byte[] response) : base(response)
        {
            if (VerifyOutcome() == EOutcome.OK)
            {
                MasterStatus = (EUpgradeStatus)response[6];
                DeviceStatus = (EUpgradeStatus)response[7];
                Device2Status = (EUpgradeStatus)response[8];
            }
        }
    }


    /// <summary>
    /// Class describing a circuit
    /// </summary>
    public class Circuit
    {
        public string CircuitId;
        public UInt32 Total;
    }

    /// <summary>
    /// Response Class for Total requests
    /// </summary>
    public class RpResponseTotals : RpResponseBase
    {
        public List<Circuit> Circuits;

        public RpResponseTotals(byte[] response)
            : base(response)
        {
            if (VerifyOutcome() == EOutcome.OK)
            {
                for (int i = 1; i < InfoLength; i += 20)
                {
                    byte[] circuitIdBytes = new byte[16];
                    Buffer.BlockCopy(Info, i, circuitIdBytes, 0, 16);

                    Circuits.Add(new Circuit()
                    {
                        CircuitId = Encoding.ASCII.GetString(circuitIdBytes),
                        Total = BitConverter.ToUInt32(Info, (i + 16))
                    });
                }
            }
        }
    }


    /// <summary>
    /// Response class for total amount requests
    /// </summary>
    public class RpResponseTotalAmount : RpResponseBase
    {
        public UInt32 TotalAmount;

        public RpResponseTotalAmount(byte[] response)
            : base(response)
        {
            if (VerifyOutcome() == EOutcome.OK)
            {
                TotalAmount = BitConverter.ToUInt32(response, 6);
            }
        }
    }

    /// <summary>
    /// Response class for GSM Status requests
    /// </summary>
    public class RpResponseGSMStatus : RpResponseBase
    {
        public bool SimPresent;
        public ushort SignalLevel;
        public ushort SignalQuality;
        public byte[] Carrier;

        public RpResponseGSMStatus(byte[] response)
            : base(response)
        {
            if (VerifyOutcome() == EOutcome.OK)
            {
                Carrier = new byte[16];
                SimPresent = IsBitSet(response[6], 0);
                SignalLevel =  response[7];
                SignalQuality = response[8];
                Buffer.BlockCopy(response, 9, Carrier, 0, 16);
            }
        }

        public string PrintDetails()
        {
            StringBuilder sb = new StringBuilder();
            if (VerifyOutcome() == EOutcome.OK)
            {
                sb.AppendLine((SimPresent) ? "SIM Present" : "Absent");
                sb.AppendLine("Carrier: " + Encoding.ASCII.GetString(Carrier));
                sb.AppendLine("Signal Level: " + SignalLevel.ToString());
                sb.AppendLine("Signal Qaul: " + SignalQuality.ToString());
            }

            return sb.ToString();
        }
    }

    /// <summary>
    /// Response Class for Pre-Auth Requests
    /// </summary>
    public class RpResponsePreAuthOutcome : RpResponseBase
    {
        public byte PreAuthorizationIndex;
        public byte[] TransactionData;
        public Dictionary<string, string> Results = new Dictionary<string, string>();
        
        public List<EPetroPromptTags> PromptTags = new List<EPetroPromptTags>();

        public RpResponsePreAuthOutcome(byte[] response) : base(response)
        {
            if (InfoLength == 1)
                throw new Exception("PreAuthOutcome Request Failed with " + VerifyOutcome().ToString());

            PreAuthorizationIndex = response[6];

            var transactionDataSize = 0;
            transactionDataSize = InfoLength - 2; // minus Outcome and RFU
            TransactionData = new byte[transactionDataSize];
            Buffer.BlockCopy(response, 7, TransactionData, 0, transactionDataSize);

            string[] resultPairs = Encoding.ASCII.GetString(TransactionData).Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
            foreach (var item in resultPairs)
            {
                int pos = item.IndexOf(':');
                if (pos > 1) Results.Add(item.Substring(0, pos), item.Substring(pos + 1, (item.Length - pos - 1)));
            }

            string tagstring = "";
            if (Results.ContainsKey("PetroTags"))
            {
                tagstring = Results["PetroTags"] ?? "";
            }

            for (int x = 0; x < (tagstring.Length); x += 2)
            {
                string hexstring = tagstring.Substring(x, 2).ToUpper();
                if (hexstring == "0D0A") continue;
                PromptTags.Add(EnumHelper.GetEnumValue<EPetroPromptTags>(Convert.ToInt32(hexstring, 16)));
            }

        }

        public string ToXmlString(bool limited)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(VerifyOutcome().ToString() + " ");
            sb.Append("<Slot>" + PreAuthorizationIndex.ToString());

            if (Results.Count > 0)
            {
                foreach (KeyValuePair<string, string> result in Results)
                {
                    if (!limited)
                        sb.Append("<" + result.Key + ">" + result.Value);
                    else
                    {
                        switch (result.Key)
                        {
                            case "Pan":
                            case "Result":
                            case "Auth Code":
                            case "Err code":
                            case "Card type":
                            case "Acquirer":
                                sb.Append("<" + result.Key + ">" + result.Value.Trim());
                                break;
                        }
                    }
                }
            }

            return sb.ToString();
        }
    }


    public class RpResponsePaymentOutcome : RpResponseBase
    {
        public byte RFU;
        public byte[] TransactionData;
        public Dictionary<string,string> Results = new Dictionary<string, string>();

        public RpResponsePaymentOutcome(byte[] response)
            : base(response)
        {
            if (InfoLength < 3) return;
            
            var transactionDataSize = 0;
            transactionDataSize = InfoLength - 2; // minus Outcome and RFU
            RFU = response[6];
            TransactionData = new byte[transactionDataSize];
            Buffer.BlockCopy(response, 7, TransactionData, 0, transactionDataSize);

            string[] resultPairs = Encoding.ASCII.GetString(TransactionData)
                .Split(new string[] {"\r\n", "\n"}, StringSplitOptions.None);
            foreach (var item in resultPairs)
            {
                int pos = item.IndexOf(':');
                if (pos > 1) Results.Add(item.Substring(0,pos),item.Substring(pos + 1,(item.Length - pos - 1)));
            }
            
        }
    }



    /// <summary>
    /// Response Class for DateTime requests
    /// </summary>
    public class RpResponseDateTime : RpResponseBase
    {
        public DateTime DtDateTime;

        public RpResponseDateTime(byte[] response)
            : base(response)
        {
            if (VerifyOutcome() == EOutcome.OK)
            {
                if (response.Length != 24)
                    throw new Exception("Could not pull time from Terminal");

                if ("00" == Encoding.ASCII.GetString(response, 12, 2))
                {
                    DtDateTime = new DateTime();
                    return;
                }

                String sTemp =
                "20" + Encoding.ASCII.GetString(response, 12, 2) +
                    Encoding.ASCII.GetString(response, 9, 2) +
                    Encoding.ASCII.GetString(response, 6, 2) +
                    Encoding.ASCII.GetString(response, 14, 2) +
                    Encoding.ASCII.GetString(response, 17, 2) +
                    Encoding.ASCII.GetString(response, 20, 2);


                DtDateTime = new DateTime(
                    int.Parse("20" + Encoding.ASCII.GetString(response, 12, 2)),
                    int.Parse(Encoding.ASCII.GetString(response, 9, 2)),
                    int.Parse(Encoding.ASCII.GetString(response, 6, 2)),
                    int.Parse(Encoding.ASCII.GetString(response, 14, 2)),
                    int.Parse(Encoding.ASCII.GetString(response, 17, 2)),
                    int.Parse(Encoding.ASCII.GetString(response, 20, 2))
                    );
            }
        }
    }

    /// <summary>
    /// Response Class for DateTime requests 2nd format
    /// </summary>
    public class RpResponseDateTime2 : RpResponseBase
    {
        public DateTime DtDateTime;

        public RpResponseDateTime2(byte[] response)
            : base(response)
        {
            if (VerifyOutcome() == EOutcome.OK)
            {
                if ("00" == Encoding.ASCII.GetString(response, 12, 2))
                {
                    DtDateTime = new DateTime();
                    return;
                }
                DtDateTime = new DateTime(
                    int.Parse(Encoding.ASCII.GetString(response, 12, 4)),
                    int.Parse(Encoding.ASCII.GetString(response, 9, 2)),
                    int.Parse(Encoding.ASCII.GetString(response, 6, 2)),
                    int.Parse(Encoding.ASCII.GetString(response, 17, 2)),
                    int.Parse(Encoding.ASCII.GetString(response, 20, 2)),
                    0
                );
            }
        }
    }

    

    /// <summary>
    /// Response Class for the Settlement Response for Mag Store and Forward Settlement Requests
    /// </summary>
    public class RpResponseMsfOutcome : RpResponseBase
    {
        public byte[] ReferenceNumber;
        public byte[] ErrorCode;

        public RpResponseMsfOutcome(byte[] response) : base(response)
        {
            ReferenceNumber = new byte[4];
            ErrorCode = new byte[4];
            Buffer.BlockCopy(response, 6, ReferenceNumber, 0, 4);
            Buffer.BlockCopy(response, 10, ErrorCode, 0, 4);
        }
    }

    /// <summary>
    /// Response class for Hashing of public magnetic track data
    /// </summary>
    public class RpResponseHash : RpResponseBase
    {
        public byte[] HashTrack1;
        public byte[] HashTrack2;
        public string MaskedPan;
        public string ExpiryDate;
        public string CardHolderName;

        public RpResponseHash(byte[] response) : base(response)
        {
            HashTrack1 = Enumerable.Repeat((byte)0x00, 32).ToArray();
            HashTrack2 = Enumerable.Repeat((byte)0x00, 32).ToArray();
            if (VerifyOutcome() == EOutcome.OK)
            {
                Buffer.BlockCopy(Info, 0, HashTrack1, 0, 32);
                Buffer.BlockCopy(Info, 32, HashTrack2, 0, 32);

                if (InfoLength > 64)
                {
                    MaskedPan = Encoding.UTF8.GetString(Info, 64 , 19);
                    MaskedPan = MaskedPan.Trim('\0');
                }

                if (InfoLength > 84)
                {
                    ExpiryDate = Encoding.UTF8.GetString(Info, 83, 4);
                    ExpiryDate = ExpiryDate.Trim('\0');
                }

                if (InfoLength > 87)
                {
                    CardHolderName = Encoding.UTF8.GetString(Info, 87, 26);
                    CardHolderName = CardHolderName.Trim('\0');
                }
            }
        }

        public string PrintDetails()
        {
            StringBuilder sb = new StringBuilder();
            if (!string.IsNullOrEmpty(MaskedPan)) 
                sb.AppendLine( "MaskedPAN = " + MaskedPan );
            if (!string.IsNullOrEmpty(ExpiryDate))
                sb.AppendLine("ExpiryDate = " + ExpiryDate);
            if (!string.IsNullOrEmpty(CardHolderName))
                sb.AppendLine("CardHolderName = " + CardHolderName);

            if (HashTrack1.Length > 0)
            {
                sb.AppendLine("HasTrack1 = ");
                foreach (var hexchar in HashTrack1)
                    sb.Append(hexchar.ToString("X2") + " ");

                sb.Append(Environment.NewLine);
            }

            if (HashTrack2.Length > 0)
            {
                sb.AppendLine("HasTrack2 = ");

                foreach (var hexchar in HashTrack2)
                    sb.Append(hexchar.ToString("X2") + " ");

                sb.Append(Environment.NewLine);
            }

            return sb.ToString();
        }
    }

    /// <summary>
    /// Response Class for IP Address Requests
    /// </summary>
    public class RpResponseIPAddress : RpResponseBase
    {
        public string IPAddress;
        public int PcPort;
        public EPPP PPP;


        public RpResponseIPAddress(byte[] response)
            : base(response)
        {
            IPAddress = Encoding.ASCII.GetString(response, 5, 15);
            int.TryParse(Encoding.ASCII.GetString(response, 20, 5), out PcPort);
            PPP = (Response[25] == 0x00) ? EPPP.Disabled : EPPP.Enabled;
        }

    }

    /// <summary>
    /// Response Class for MDB Parameter Requests
    /// </summary>
    public class RpResponseMdbParams : RpResponseBase
    {
        public ECashlessNumber CashlessNumber;
        public int Foundincents;
        public int MdbCommandTimeout;
        public int VendRequestAppriveTimeout;
        public ESaleType SaleType;


        public RpResponseMdbParams(byte[] response)
            : base(response)
        {
            CashlessNumber = (response[5] == 0x01) ? ECashlessNumber.Cashless1 : ECashlessNumber.Cashless2;
            Foundincents = EndianBitConverter.Big.ToInt32(response, 6); // from bytes 7,8,9,10
            MdbCommandTimeout = response[10];
            VendRequestAppriveTimeout = response[11];
            SaleType = (response[12] == 0x01) ? ESaleType.SingleSale : ESaleType.MultipleSale;
        }

    }

    
    /// <summary>
    /// Class for the Command object. This object is used to initiate requests to the terminal.
    /// </summary>
    public class RpCommand
    {
        //ASCII
        public const byte STX = 0x02;
        public const byte ETX = 0x03;
        //Members
        private readonly RetailProtocol _rp;
        private byte _checksum;
        public byte[] Command;
        public byte? CommandId;
        public byte[] Info;
        public ushort InfoLength;
        public byte EcrStopCommand;
        
        public byte UPTAddress;
        public string FriendlyName;
        public int ewt; //expected wait time for this command (in seconds)
        
        /// <summary>
        /// Create the command class
        /// </summary>
        /// <param name="rp">Retail Protocol object</param>
        public RpCommand(RetailProtocol rp)
        {
            UPTAddress = 0;
            CommandId = null;
            InfoLength = 0;
            Info = new byte[10*1024]; // arbitrary max
            _checksum = 0x7F;
            _rp = rp;
            EcrStopCommand = 0x00;
            FriendlyName = "";
            ewt = 10; //default to 10 seconds
        }

        /// <summary>
        /// Builds the byte array required for the terminal to execute the specific request
        /// </summary>
        public void CompileCommand()
        {
            if (null == CommandId)
            {
                throw new Exception("No Command ID");
            }


            int j = 0; //byte position counter for command
            _checksum = 0x7F;
            //int maxLen = (Info.Length < 1200) ? 1200 : 100 + Info.Length;
            int maxLen = (Info.Length < 10000) ? 10000 : 100 + Info.Length;
            var command = new byte[maxLen];
            command[j++] = STX;
            command[j++] = UPTAddress;
            command[j++] = (byte)CommandId;
            command[j++] = (byte) (InfoLength >> 8); //MSB
            command[j++] = (byte) (InfoLength & 0xFF); //LSB

            int x = j;

            for (var i = 0; i < InfoLength; i++)
            {
                command[x++] = Info[i];
            }

            command[x++] = ETX;

            for (var i = 0; i <= x; i++)
            {
                _checksum = (byte) (_checksum ^ command[i]);
            }

            command[x] = _checksum;

            Command = new byte[(x + 1)];
            Buffer.BlockCopy(command,0,Command,0,(x + 1));
        }

        /// <summary>
        /// Optionally verify outbound checksum
        /// </summary>
        /// <returns>Bool - TRUE=Valid</returns>
        public bool VerifyChecksum()
        {
            byte computeCheckSum = 0x7F;

            for (var i = 0; i <= (InfoLength + 5); i++)
            {
                computeCheckSum = (byte)(computeCheckSum ^ Command[i]);
            }
            return (computeCheckSum == Command[(Command.Length -1)]); //0 ordinal
        }

        /// <summary>
        /// Executes the request to the terminal
        /// </summary>
        /// <returns>Byte array of the response from the terminal</returns>
        public byte[] Execute()
        {
            if (Command.Length == 0)
            {
                throw new Exception("Command not built.");
            }

            _rp.ExpectedWaitTime = ewt;
            if (_rp.UseEthernet) _rp.EthernetListener.ExpectedWaitTime = ewt;
            if (EcrStopCommand != 0x00) _rp.EcrStopMessage = EcrStopCommand;
            return _rp.BytesOut(Command, FriendlyName);
            
        }

        /// <summary>
        /// Executes the request to the terminal. 
        /// This method is specific for messages that continue across multiple requests until the terminal has exchanged all the data.
        /// </summary>
        /// <param name="requestName">The name of the request for logging and exception management</param>
        /// <returns>Byte array of the response from the terminal</returns>
        public byte[] ExecuteUntilEmpty(string requestName)
        {

            bool moreData = true;
            byte[] dataBytes = new byte[0];
            
            

            if (Command.Length == 0)
            {
                throw new Exception("Command not built.");
            }

            int oldSize = 0;
            while (moreData)
            {
                RpResponseBase rpInnerResponse = new RpResponseBase(_rp.BytesOut(Command, requestName));

                if (1 == rpInnerResponse.Info[1])
                {
                    moreData = false;
                }
                if (0 == dataBytes.Length)
                {
                    dataBytes = new byte[rpInnerResponse.InfoLength];
                    Buffer.BlockCopy(rpInnerResponse.Info,0,dataBytes,0,rpInnerResponse.InfoLength);
                }
                else
                {
                    oldSize = dataBytes.Length;
                    Array.Resize(ref dataBytes,(oldSize+rpInnerResponse.InfoLength));
                    Buffer.BlockCopy(rpInnerResponse.Info,0,dataBytes,oldSize,rpInnerResponse.InfoLength);
                }
            }
            return dataBytes;
        }
    }
}