namespace AA.PangoApp.Payments.GlobalCom
{


    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class GlobalcomReceipt
    {

        private GlobalcomReceiptTransactionData transactionDataField;

        private GlobalcomReceiptResponse[] gatewayField;

        /// <remarks/>
        public GlobalcomReceiptTransactionData TransactionData
        {
            get
            {
                return this.transactionDataField;
            }
            set
            {
                this.transactionDataField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("Response", IsNullable = false)]
        public GlobalcomReceiptResponse[] Gateway
        {
            get
            {
                return this.gatewayField;
            }
            set
            {
                this.gatewayField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class GlobalcomReceiptTransactionData
    {

        private string dateField;

        private string timeField;

        private uint tIDField;

        private uint merchantField;

        private string acquirerField;

        private string pANField;

        private byte gBCCardTypeField;

        private byte errorField;

        private byte cVMField;

        private string currencyField;

        private decimal totalField;

        private string authCodeField;

        private byte authModeField;

        private byte gBCResultField;

        private GlobalcomReceiptTransactionDataTag[] eMVTagsField;

        /// <remarks/>
        public string Date
        {
            get
            {
                return this.dateField;
            }
            set
            {
                this.dateField = value;
            }
        }

        /// <remarks/>
        public string Time
        {
            get
            {
                return this.timeField;
            }
            set
            {
                this.timeField = value;
            }
        }

        /// <remarks/>
        public uint TID
        {
            get
            {
                return this.tIDField;
            }
            set
            {
                this.tIDField = value;
            }
        }

        /// <remarks/>
        public uint Merchant
        {
            get
            {
                return this.merchantField;
            }
            set
            {
                this.merchantField = value;
            }
        }

        /// <remarks/>
        public string Acquirer
        {
            get
            {
                return this.acquirerField;
            }
            set
            {
                this.acquirerField = value;
            }
        }

        /// <remarks/>
        public string PAN
        {
            get
            {
                return this.pANField;
            }
            set
            {
                this.pANField = value;
            }
        }

        /// <remarks/>
        public byte GBCCardType
        {
            get
            {
                return this.gBCCardTypeField;
            }
            set
            {
                this.gBCCardTypeField = value;
            }
        }

        /// <remarks/>
        public byte Error
        {
            get
            {
                return this.errorField;
            }
            set
            {
                this.errorField = value;
            }
        }

        /// <remarks/>
        public byte CVM
        {
            get
            {
                return this.cVMField;
            }
            set
            {
                this.cVMField = value;
            }
        }

        /// <remarks/>
        public string Currency
        {
            get
            {
                return this.currencyField;
            }
            set
            {
                this.currencyField = value;
            }
        }

        /// <remarks/>
        public decimal Total
        {
            get
            {
                return this.totalField;
            }
            set
            {
                this.totalField = value;
            }
        }

        /// <remarks/>
        public string AuthCode
        {
            get
            {
                return this.authCodeField;
            }
            set
            {
                this.authCodeField = value;
            }
        }

        /// <remarks/>
        public byte AuthMode
        {
            get
            {
                return this.authModeField;
            }
            set
            {
                this.authModeField = value;
            }
        }

        /// <remarks/>
        public byte GBCResult
        {
            get
            {
                return this.gBCResultField;
            }
            set
            {
                this.gBCResultField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("Tag", IsNullable = false)]
        public GlobalcomReceiptTransactionDataTag[] EMVTags
        {
            get
            {
                return this.eMVTagsField;
            }
            set
            {
                this.eMVTagsField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class GlobalcomReceiptTransactionDataTag
    {

        private string tagIDField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string TagID
        {
            get
            {
                return this.tagIDField;
            }
            set
            {
                this.tagIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class GlobalcomReceiptResponse
    {

        private GlobalcomReceiptResponseTransactionDetails transactionDetailsField;

        private GlobalcomReceiptResponseResult resultField;

        private GlobalcomReceiptResponseCardDetails cardDetailsField;

        private string typeField;

        private string versionField;

        /// <remarks/>
        public GlobalcomReceiptResponseTransactionDetails TransactionDetails
        {
            get
            {
                return this.transactionDetailsField;
            }
            set
            {
                this.transactionDetailsField = value;
            }
        }

        /// <remarks/>
        public GlobalcomReceiptResponseResult Result
        {
            get
            {
                return this.resultField;
            }
            set
            {
                this.resultField = value;
            }
        }

        /// <remarks/>
        public GlobalcomReceiptResponseCardDetails CardDetails
        {
            get
            {
                return this.cardDetailsField;
            }
            set
            {
                this.cardDetailsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string type
        {
            get
            {
                return this.typeField;
            }
            set
            {
                this.typeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string version
        {
            get
            {
                return this.versionField;
            }
            set
            {
                this.versionField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class GlobalcomReceiptResponseTransactionDetails
    {

        private string cardEaseReferenceField;

        private GlobalcomReceiptResponseTransactionDetailsLocalDateTime localDateTimeField;

        private GlobalcomReceiptResponseTransactionDetailsUTC uTCField;

        /// <remarks/>
        public string CardEaseReference
        {
            get
            {
                return this.cardEaseReferenceField;
            }
            set
            {
                this.cardEaseReferenceField = value;
            }
        }

        /// <remarks/>
        public GlobalcomReceiptResponseTransactionDetailsLocalDateTime LocalDateTime
        {
            get
            {
                return this.localDateTimeField;
            }
            set
            {
                this.localDateTimeField = value;
            }
        }

        /// <remarks/>
        public GlobalcomReceiptResponseTransactionDetailsUTC UTC
        {
            get
            {
                return this.uTCField;
            }
            set
            {
                this.uTCField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class GlobalcomReceiptResponseTransactionDetailsLocalDateTime
    {

        private string formatField;

        private ulong valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string format
        {
            get
            {
                return this.formatField;
            }
            set
            {
                this.formatField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public ulong Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class GlobalcomReceiptResponseTransactionDetailsUTC
    {

        private string formatField;

        private ulong valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string format
        {
            get
            {
                return this.formatField;
            }
            set
            {
                this.formatField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public ulong Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class GlobalcomReceiptResponseResult
    {

        private byte localResultField;

        private byte acquirerResponseCodeField;

        private bool acquirerResponseCodeFieldSpecified;

        private string authCodeField;

        private string authorisationEntityField;

        private GlobalcomReceiptResponseResultAmountOnlineApproved amountOnlineApprovedField;

        /// <remarks/>
        public byte LocalResult
        {
            get
            {
                return this.localResultField;
            }
            set
            {
                this.localResultField = value;
            }
        }

        /// <remarks/>
        public byte AcquirerResponseCode
        {
            get
            {
                return this.acquirerResponseCodeField;
            }
            set
            {
                this.acquirerResponseCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool AcquirerResponseCodeSpecified
        {
            get
            {
                return this.acquirerResponseCodeFieldSpecified;
            }
            set
            {
                this.acquirerResponseCodeFieldSpecified = value;
            }
        }

        /// <remarks/>
        public string AuthCode
        {
            get
            {
                return this.authCodeField;
            }
            set
            {
                this.authCodeField = value;
            }
        }

        /// <remarks/>
        public string AuthorisationEntity
        {
            get
            {
                return this.authorisationEntityField;
            }
            set
            {
                this.authorisationEntityField = value;
            }
        }

        /// <remarks/>
        public GlobalcomReceiptResponseResultAmountOnlineApproved AmountOnlineApproved
        {
            get
            {
                return this.amountOnlineApprovedField;
            }
            set
            {
                this.amountOnlineApprovedField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class GlobalcomReceiptResponseResultAmountOnlineApproved
    {

        private string unitField;

        private decimal valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string unit
        {
            get
            {
                return this.unitField;
            }
            set
            {
                this.unitField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public decimal Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class GlobalcomReceiptResponseCardDetails
    {

        private string cardReferenceField;

        private string cardHashField;

        private string pANField;

        private GlobalcomReceiptResponseCardDetailsExpiryDate expiryDateField;

        private GlobalcomReceiptResponseCardDetailsStartDate startDateField;

        private byte issueNumberField;

        private GlobalcomReceiptResponseCardDetailsCardScheme cardSchemeField;

        private GlobalcomReceiptResponseCardDetailsContact contactField;

        private GlobalcomReceiptResponseCardDetailsICC iCCField;

        /// <remarks/>
        public string CardReference
        {
            get
            {
                return this.cardReferenceField;
            }
            set
            {
                this.cardReferenceField = value;
            }
        }

        /// <remarks/>
        public string CardHash
        {
            get
            {
                return this.cardHashField;
            }
            set
            {
                this.cardHashField = value;
            }
        }

        /// <remarks/>
        public string PAN
        {
            get
            {
                return this.pANField;
            }
            set
            {
                this.pANField = value;
            }
        }

        /// <remarks/>
        public GlobalcomReceiptResponseCardDetailsExpiryDate ExpiryDate
        {
            get
            {
                return this.expiryDateField;
            }
            set
            {
                this.expiryDateField = value;
            }
        }

        /// <remarks/>
        public GlobalcomReceiptResponseCardDetailsStartDate StartDate
        {
            get
            {
                return this.startDateField;
            }
            set
            {
                this.startDateField = value;
            }
        }

        /// <remarks/>
        public byte IssueNumber
        {
            get
            {
                return this.issueNumberField;
            }
            set
            {
                this.issueNumberField = value;
            }
        }

        /// <remarks/>
        public GlobalcomReceiptResponseCardDetailsCardScheme CardScheme
        {
            get
            {
                return this.cardSchemeField;
            }
            set
            {
                this.cardSchemeField = value;
            }
        }

        /// <remarks/>
        public GlobalcomReceiptResponseCardDetailsContact Contact
        {
            get
            {
                return this.contactField;
            }
            set
            {
                this.contactField = value;
            }
        }

        /// <remarks/>
        public GlobalcomReceiptResponseCardDetailsICC ICC
        {
            get
            {
                return this.iCCField;
            }
            set
            {
                this.iCCField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class GlobalcomReceiptResponseCardDetailsExpiryDate
    {

        private string formatField;

        private ushort valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string format
        {
            get
            {
                return this.formatField;
            }
            set
            {
                this.formatField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public ushort Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class GlobalcomReceiptResponseCardDetailsStartDate
    {

        private string formatField;

        private ushort valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string format
        {
            get
            {
                return this.formatField;
            }
            set
            {
                this.formatField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public ushort Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class GlobalcomReceiptResponseCardDetailsCardScheme
    {

        private string descriptionField;

        private byte idField;

        private string accountTypeField;

        /// <remarks/>
        public string Description
        {
            get
            {
                return this.descriptionField;
            }
            set
            {
                this.descriptionField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public byte id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string accountType
        {
            get
            {
                return this.accountTypeField;
            }
            set
            {
                this.accountTypeField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class GlobalcomReceiptResponseCardDetailsContact
    {

        private GlobalcomReceiptResponseCardDetailsContactName nameField;

        /// <remarks/>
        public GlobalcomReceiptResponseCardDetailsContactName Name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class GlobalcomReceiptResponseCardDetailsContactName
    {

        private string firstNameField;

        private string lastNameField;

        /// <remarks/>
        public string FirstName
        {
            get
            {
                return this.firstNameField;
            }
            set
            {
                this.firstNameField = value;
            }
        }

        /// <remarks/>
        public string LastName
        {
            get
            {
                return this.lastNameField;
            }
            set
            {
                this.lastNameField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class GlobalcomReceiptResponseCardDetailsICC
    {

        private GlobalcomReceiptResponseCardDetailsICCICCTag[] iCCTagField;

        private string typeField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("ICCTag")]
        public GlobalcomReceiptResponseCardDetailsICCICCTag[] ICCTag
        {
            get
            {
                return this.iCCTagField;
            }
            set
            {
                this.iCCTagField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string type
        {
            get
            {
                return this.typeField;
            }
            set
            {
                this.typeField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class GlobalcomReceiptResponseCardDetailsICCICCTag
    {

        private string tagidField;

        private ulong valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string tagid
        {
            get
            {
                return this.tagidField;
            }
            set
            {
                this.tagidField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public ulong Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }



}
