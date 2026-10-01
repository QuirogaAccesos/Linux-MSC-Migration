using System;
using System.Text;

namespace CCI.Globalcom.GlobalcomRetailProtocol
{
    /// <summary> Tag and Value </summary>
    public class Node
    {
        /// <summary> Tag of this object </summary>
        public string Tag;
        /// <summary> Value of this object </summary>
        public string Value;
        /// <summary> Denotes if an extender length byte is used. </summary>
        public bool ExtenderByte;

        /// <summary> Consturctor </summary>
        /// <param name="bExtenderByte"></param>
        public Node(bool bExtenderByte) { ExtenderByte = bExtenderByte; }

        /// <summary> string representation of this object </summary>
        /// <returns> string of this object </returns>
        public string LongTag()
        {
            ETLVTags eTag =
                EnumHelper.GetEnumValue<ETLVTags>(long.Parse(Tag, System.Globalization.NumberStyles.HexNumber));
            if (eTag.ToString().CompareTo(long.Parse(Tag, System.Globalization.NumberStyles.HexNumber).ToString()) == 0)
                eTag = ETLVTags.Unknown_Tag;

            StringBuilder sb = new StringBuilder();
            sb.Append("node tag='0x" + Tag + "'");

            // Add the comment
            if (eTag != ETLVTags.Unknown_Tag)
            {
                if (eTag == ETLVTags.terminal_AID)
                {
                    if (Value.Equals("A0 00 00 00 03 10 10"))
                        sb.Append(" <!-- Visa AID -->");
                    else if (Value.Equals("A0 00 00 00 04 10 10"))
                        sb.Append(" <!-- Mastercard AID -->");
                    else if (Value.Equals("A0 00 00 00 03 20 10"))
                        sb.Append(" <!-- Visa Electron AID -->");
                    else if (Value.Equals("A0 00 00 02 77 10 10"))
                        sb.Append(" <!-- Interac AID -->");
                    else
                        sb.Append(" <!--" + eTag.ToString() + "-->");
                }
                else
                    sb.Append(" <!--" + eTag.ToString() + "-->");
            }

            return sb.ToString();
        }

        /// <summary>
        ///  Get XML format of the node
        /// </summary>
        /// <returns>xml formatted string</returns>
        public string GetXML()
        {
            ETLVTags eTag =
                EnumHelper.GetEnumValue<ETLVTags>(long.Parse(Tag, System.Globalization.NumberStyles.HexNumber));
            if (eTag.ToString().CompareTo(long.Parse(Tag, System.Globalization.NumberStyles.HexNumber).ToString()) == 0)
                eTag = ETLVTags.Unknown_Tag;

            StringBuilder sb = new StringBuilder();
            // Add the comment
            if (eTag != ETLVTags.Unknown_Tag)
            {
                if (eTag == ETLVTags.terminal_AID)
                {
                    if (Value.Equals("A0 00 00 00 03 10 10"))
                        sb.Append("    <!-- Visa AID -->");
                    else if (Value.Equals("A0 00 00 00 04 10 10"))
                        sb.Append("    <!-- Mastercard AID -->");
                    else if (Value.Equals("A0 00 00 00 03 20 10"))
                        sb.Append("    <!-- Visa Electron AID -->");
                    else if (Value.Equals("A0 00 00 02 77 10 10"))
                        sb.Append("    <!-- Interac AID -->");
                    else
                        sb.Append("    <!--" + eTag.ToString() + "-->");
                }
                else
                    sb.Append("    <!--" + eTag.ToString() + "-->");
            }

            sb.Append("  <node tag='0x" + Tag + "'>");
            sb.Append(Value);
            sb.Append("</node>");
            sb.AppendLine("");

            return sb.ToString();
        }

        /// <summary> Get the Length of this object if binary encoded </summary>
        /// <returns> length or binary encoded data</returns>
        public int GetLength()
        {
            // Tag size
            int nLen = Tag.Length / 2;

            // Data Length
            string sBytes = Value.Replace(" ", string.Empty);
            nLen += sBytes.Length / 2;

            // Length and if extended applies
            if (ExtenderByte && ((sBytes.Length / 2) > 255))
                nLen+=3;
            else if (ExtenderByte)
                nLen += 2;
            else
                nLen += 1;

            return nLen;
        }

        /// <summary> Get the binary encoded data </summary>
        /// <param name="info"></param>
        /// <param name="infoLength"></param>
        /// <returns> postion in the info of the next spot to write to</returns>
        public int Encode(ref byte[] info, int pos)
        {
            // Copy the TAG
            for (int i = 0; i < Tag.Length; i+=2)
                info[pos++] = Convert.ToByte(Tag.Substring(i, 2), 16);

            string sValue = Value.Replace(" ", string.Empty);
            int nLen = sValue.Length / 2;

            byte[] intBytes = BitConverter.GetBytes(nLen);

            // Copy Length and Opt Extender Byte
            if (ExtenderByte && nLen > 255)
            {
                info[pos++] = 0x82;
                info[pos++] = intBytes[1];
                info[pos++] = intBytes[0];
            }
            else if (ExtenderByte)
            {
                info[pos++] = 0x81;
                info[pos++] = intBytes[0];
            }
            else
                info[pos++] = intBytes[0];

            // Copy the Value
            for (int i = 0; i < sValue.Length; i += 2)
                info[pos++] = Convert.ToByte(sValue.Substring(i, 2), 16);

            return pos;
        }
    }
}
