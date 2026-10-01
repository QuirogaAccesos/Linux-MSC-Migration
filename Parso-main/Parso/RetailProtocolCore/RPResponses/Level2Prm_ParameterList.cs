using System.Collections.Generic;
using System.Text;

namespace CCI.Globalcom.GlobalcomRetailProtocol
{
    /// <summary> Collection of Parameter Lists </summary>
    public class ParameterList
    {
        /// <summary> Tag of this object </summary>
        public string Tag;

        /// <summary> If this Tag supports the Extended Length Byte </summary>
        public bool ExtenderByte;

        /// <summary> Collection of Parameter Blocks </summary>
        public Dictionary<int, NodeCollection> ParameterBlock;

        /// <summary> Consructor </summary>
        /// <param name="sTag"> Tag of this object</param>
        public ParameterList(string sTag)
        {
            Tag = sTag;
            ParameterBlock = new Dictionary<int, NodeCollection>();
        }

        /// <summary>
        ///  Remove a block like an AID from the PRM file.
        /// </summary>
        /// <param name="key"></param>
        public void RemoveFromBlock(int key)
        {
            ParameterBlock.Remove(key);
        }

        /// <summary> Compare this object to the tag passed in for equivelency </summary>
        /// <param name="sTag"></param>
        /// <returns> if the types match</returns>
        public bool IsType(string sTag)
        {
            if (Tag.CompareTo(sTag) == 0)
                return true;
            return false;
        }

        /// <summary> Add a Parameter Block </summary>
        /// <param name="col"></param>
        public void Add(int key, NodeCollection col)
        {
            ParameterBlock.Add(key, col);
        }

        /// <summary> Get an xml friendly description of the tag without the brackets </summary>
        /// <returns> string description of this object</returns>
        public string LongTag()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("node tag='0x" + Tag + "' ");

            switch (Tag)
            {
                case "BF918800":
                    sb.Append(" <!-- TAG AID PARAMETERS -->");
                    break;
                case "BF91880F":
                    sb.Append("<!-- Dynamic Reader Limits -->");
                    break;
                case "BF918801":
                    sb.Append("<!-- CA Keys -->");
                    break;
                case "BF918803":
                    sb.Append("<!-- TAG ICS Parameters -->");
                    break;
                case "BF918804":
                    sb.Append("<!-- parameters : kernel subset -->");
                    break;
                default:
                    sb.Append(" <!-- " + Tag + " PARAMETERS -->");
                    break;
            }

            return sb.ToString();
        }

        /// <summary> Get the XML representation of this object </summary>
        /// <returns> XML string of this object</returns>
        public string GetXML()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("<node tag='0x" + Tag + "'>");
            switch (Tag)
            {
                case "BF918800":
                    sb.AppendLine(" <!-- TAG AID PARAMETERS -->");
                    break;
                case "BF91880F":
                    sb.AppendLine( "<!-- Dynamic Reader Limits -->");
                    break;
                case "BF918801":
                    sb.AppendLine("<!-- CA Keys -->");
                    break;
                case "BF918803":
                    sb.AppendLine("<!-- TAG ICS Parameters -->");
                    break;
                case "BF918804":
                    sb.AppendLine("<!-- parameters : kernel subset -->");
                    break;
                default:
                    sb.AppendLine(" <!-- " + Tag + " PARAMETERS -->");
                    break;
            }


            foreach (KeyValuePair<int, NodeCollection> block in ParameterBlock)
            {
                sb.Append(block.Value.GetXML());
            }

            sb.AppendLine("</node>");

            return sb.ToString();
        }

        /// <summary> Length of this objects data in binary </summary>
        /// <returns> length </returns>
        public int GetLength()
        {
            int nLen = 0;
            foreach (KeyValuePair<int, NodeCollection> block in ParameterBlock)
            {
                nLen += block.Value.GetLength();
            }

            return nLen;
        }

        /// <summary> Get the binary encoded data </summary>
        /// <param name="info"></param>
        /// <param name="pos"></param>
        /// <returns>position in info plus 1 that was last written to</returns>
        public int Encode(ref byte[] info, int pos)
        {
            foreach (KeyValuePair<int, NodeCollection> block in ParameterBlock)
            {
                pos = block.Value.Encode(ref info, pos);
            }

            return pos;
        }
    }
}
