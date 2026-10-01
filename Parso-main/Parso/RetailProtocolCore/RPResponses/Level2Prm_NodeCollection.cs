using System;
using System.Collections.Generic;
using System.Text;

namespace CCI.Globalcom.GlobalcomRetailProtocol
{
    /// <summary> Collection of Parameters </summary>
    public class NodeCollection
    {
        /// <summary> Tag of this Collection of Nodes </summary>
        private string Tag;
        /// <summary> Indicator if this Tag uses the length extender </summary>
        public bool ExtenderByte;
        /// <summary> Collection of TLV tag/value pairs </summary>
        public Dictionary<int, Node> TLVList;

        /// <summary> Consructor </summary>
        /// <param name="sTag"></param>
        /// <param name="bExtenderByte"></param>
        public NodeCollection(string sTag, bool bExtenderByte)
        {
            Tag = sTag;
            ExtenderByte = bExtenderByte;
            TLVList = new Dictionary<int, Node>(); 
        }

        /// <summary> Add a Node to the TLV list </summary>
        /// <param name="node"></param>
        public void Add(Node node)
        {
            TLVList.Add(TLVList.Count, node);
        }

        /// <summary> Simple description of this object </summary>
        /// <returns> string representation of this object</returns>
        public string LongTag()
        {
            return "node tag='0x1000'";
        }

        /// <summary>
        ///  Add a Tag to the collection for this outer node
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="value"></param>
        /// <param name="extender"></param>
        public void Add(string tag, string value, bool extender)
        {
            Node node = new Node(extender);
            node.Tag = tag;
            node.Value = value;
            TLVList.Add(TLVList.Count, node);
        }

        public void RemoveTag(int key)
        {
            TLVList.Remove(key);
        }

        /// <summary>
        ///  Get XML format of the outer node and all its tags.
        /// </summary>
        /// <returns>xml formatted string</returns>
        public string GetXML()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("<node tag='0x1000'>");

            foreach (var item in TLVList)
            {
                sb.Append(item.Value.GetXML());
            }

            sb.AppendLine("</node>");

            return sb.ToString();
        }

        /// <summary> Get the Length of this data encoded as binary </summary>
        /// <returns> Length of the object as binary data</returns>
        public int GetLength()
        {
            // Get the data length
            int nLen = 0;
            foreach (var item in TLVList)
            {
                nLen += item.Value.GetLength();
            }

            // Length and if extended applies
            if (ExtenderByte && (nLen > 255))
                nLen += 3;
            else if (ExtenderByte)
                nLen += 2;
            else
                nLen += 1;

            // Tag size
            nLen += Tag.Length / 2;

            return nLen;
        }

        /// <summary> Get the binary encoded data </summary>
        /// <param name="info"></param>
        /// <param name="infoLength"></param>
        /// <returns> Position in the info that can next be written to</returns>
        public int Encode(ref byte[] info, int pos)
        {
            // Copy the Tag
            for (int i = 0; i < Tag.Length; i += 2)
                info[pos++] = Convert.ToByte(Tag.Substring(i, 2), 16);

            // Get the Len
            int nLen = 0;
            foreach (var item in TLVList)
                nLen += item.Value.GetLength();

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

            // Serialize all the Tags.
            foreach (var item in TLVList)
            {
                pos = item.Value.Encode(ref info, pos);
            }

            return pos;
        }
    }
}
 
