using CCI.Globalcom.GlobalcomRetailProtocol;
using System.Collections.Generic;

namespace AA.PangoApp.Payments.GlobalCom
{
    /// <summary>
    ///    Transaction object to hold all transaction variables.
    /// </summary>
    public class Transaction
    {
        /// <summary>  Amount of the Transaction in cents </summary>
        public int AmountInCents;
        /// <summary>  Currency of the transaction </summary>
        public ECurrencies Currency;
        /// <summary>  Online, Online with Backup, or Offline modes </summary>
        public EMode Mode;
        /// <summary>  Unique ID controlled by the client </summary>
        public string ClientTransactionId;
        /// <summary>  Language the terminal should prompt in </summary>
        public ELanguage Lang;

        /// <summary>  Response Name/Value pairs </summary>
        public Dictionary<string, string> ResponsePairs;
        /// <summary>  XML detail that can be used for an email or printed receipt  </summary>
        public string ResponseReceiptXml;
        /// <summary>  Slot in which the PreAuth is stored </summary>
        public int PreAuthSlot;

        /// <summary> Class to hold all the variables associated to a transaction. </summary>
        public Transaction()
        {
            Mode = EMode.Online;  // default
            PreAuthSlot = -1;
        }
    }

    /// <summary>
    ///   Functions supported by this Example Application
    /// </summary>
    public enum ETranType
    {
        Sale,
        OfflineSale,
        ReverseLast,
        AuthOnly,
        PreAuthWithCompletion,
        PreAuthWithReverse
    };

    /// <summary>
    ///   Modes supported by this Example Application
    /// </summary>
    public enum EModeOfCom
    {
        MUX,
        Ethernet
    };
}
