using CCI.Globalcom.GlobalcomRetailProtocol;
using System;
using System.Globalization;
using System.Text;

namespace CCI.Globalcom.GlobalcomRetailProtocol
{
    /// <summary>
    /// Response Class for Get Slot requests
    ///   0x51
    /// </summary>
    public class RpResponseGetSlotDetails : RpResponseBase
    {
        public enum ESlotStatus { Empty = 0x00, Preauthorized = 0x01, Notified = 0x02, DataCorupted = 0x03 };

        public ESlotStatus SlotStatus;
        public DateTime TransactionDateTime;
        public String Amount;
        public String Authcode;
        public String LastFour;
        public String BinRange;

        /// <summary>
        /// 0x51 Slot Detail Response
        /// </summary>
        /// <param name="response"></param>
        public RpResponseGetSlotDetails(byte[] response)
              : base(response)
        {
            if (VerifyOutcome() != EOutcome.OK)
                return;
            if (CommandId != (byte)ERetailProtocolCommands.GetSlotDetails)
                return;

            SlotStatus = (ESlotStatus)response[6];
            
            if (SlotStatus == ESlotStatus.Notified || SlotStatus == ESlotStatus.Empty || SlotStatus == ESlotStatus.DataCorupted) return;

            string timestamp = Encoding.ASCII.GetString(response, 7, 16);
            TransactionDateTime = new DateTime();
            if (!(DateTime.TryParseExact(timestamp, "dd/MM/yyHH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out TransactionDateTime))) 
            { 
                // all zeros?
            }

            string temp;
            temp = Encoding.ASCII.GetString(response, 23, 10);

            Amount = String.Format("{0:C}", Convert.ToDecimal(temp) / 100);

            Authcode = Encoding.ASCII.GetString(response, 33, 12).TrimStart('0');
            LastFour = Encoding.ASCII.GetString(response, 45, 4);
            if (response.Length > 51) // 49 plus ETX & LRC
                BinRange = Encoding.ASCII.GetString(response, 49, 6);
        }

        /// <summary>
        /// Produces human readable card info
        /// </summary>
        /// <returns>string</returns>
        public String MaskedPan()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(BinRange + "..." + LastFour);
            return sb.ToString();
        }

    /// <summary>
    /// Produces human readable contents of the Slot detail
    /// </summary>
    /// <returns>string</returns>
    public String GetDetails(int nSlot)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Slot " + nSlot.ToString() + " Status: " + SlotStatus.ToString() + ".  ");
            if (SlotStatus == ESlotStatus.Preauthorized)
            {
                sb.AppendLine("   Timestamp: " + TransactionDateTime.ToString() + ".  ");
                sb.Append("   Amount: " + Amount + ".  ");
                sb.Append("BinRange: " + BinRange + ".  ");
                sb.Append("LastFour: " + LastFour + ".  ");
                sb.Append("Authcode: " + Authcode + ".  ");
                sb.AppendLine("");
            }
            return sb.ToString();
        }
    }
}
