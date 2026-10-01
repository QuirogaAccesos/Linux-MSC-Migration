using System;
using System.Collections.Generic;
using System.Text;
//using System.Windows.Forms;

namespace CCI.Globalcom.GlobalcomRetailProtocol
{
    /// <summary> Object to hold the contents of the Parameter File </summary>
    public class Level2PrmFile
    {
        /// <summary> List of Parameters in the file. </summary>
        public Dictionary<int, ParameterList> _listOfParameters;

        /// <summary> Constructor</summary>
        public Level2PrmFile()  { ; }

        /// <summary>
        ///   Remove a second level block section from the prm file.
        /// </summary>
        /// <param name="topKey"></param>
        /// <param name="parmKey"></param>
        //public void Remove(TreeNode node)
        //{
        //    TreeNode parent = node.Parent;

        //    // Removing an AID
        //    if (node.Text.Equals("node tag='0x1000'"))
        //    {
        //        int topKey = (int)parent.Tag;
        //        int parmKey = (int)node.Tag;
        //        _listOfParameters[topKey].RemoveFromBlock(parmKey);
        //    }
        //    // Removing a Tag under and AID
        //    else
        //    {
        //        int topKey = (int)parent.Parent.Tag;
        //        int parmKey = (int) parent.Tag;
        //        int tagKey = (int) node.Tag;
                
        //        _listOfParameters[topKey].ParameterBlock[parmKey].RemoveTag(tagKey);
        //    }
        //}

        public void Add(int topKey, int parmKey, Node node)
        {
            NodeCollection coll = _listOfParameters[topKey].ParameterBlock[parmKey];
            coll.Add(node);
        }

        /// <summary> Given a byte array parse the TLV </summary>
        /// <param name="info"></param>
        /// <param name="infoLength"></param>
        public void Decode(byte[] info, int infoLength)
        {
            _listOfParameters = new Dictionary<int, ParameterList>();

            ParameterList parmList = null;

            int topLevelKey = 0;
            int parmListKey = 0;
            int gIndex = 0;
            while (gIndex < infoLength - 1)
            {
                string outerTagName = "";
                int outerTagLength = 0;
                bool extenderByte = false;

                // This is an outside Tag. 
                gIndex = getTag(gIndex, info, ref outerTagName, ref outerTagLength, ref extenderByte);
                outerTagLength += gIndex;

                // If this is our first one create it
                if (parmList == null)
                    parmList = new ParameterList(outerTagName);
                // If this is different from the last collection.  Save the collection and start a new one.
                else if (!parmList.IsType(outerTagName))
                {
                    _listOfParameters.Add(topLevelKey++, parmList);
                    parmList = new ParameterList(outerTagName);
                }

                NodeCollection aidNode = new NodeCollection(outerTagName, extenderByte);

                // Loop Over the inner data.
                while (gIndex < outerTagLength)
                {
                    string tagName = "";
                    int tagLength = 0;
                    bool tagExtender = false;

                    gIndex = getTag(gIndex, info, ref tagName, ref tagLength, ref tagExtender);

                    StringBuilder sbHexValue = new StringBuilder();
                    int x = 0;
                    while (x++ < tagLength)
                    {
                        if (x > 1)
                            sbHexValue.Append(" ");
                        sbHexValue.Append(info[gIndex++].ToString("X2"));
                    }

                    aidNode.Add(tagName, sbHexValue.ToString(), tagExtender);
                }

                parmList.Add(parmListKey++, aidNode);
            }

            // Add the last one to the list.
            _listOfParameters.Add(topLevelKey++, parmList);
        }

        /// <summary> Get the Binary Length of this Object </summary>
        /// <returns> Length of binary data</returns>
        public int GetLength()
        {
            int nLen = 0;
            foreach (var node in _listOfParameters)
                nLen += node.Value.GetLength();

            return nLen;
        }


        /// <summary> Get the binary encoded data </summary>
        /// <param name="info"></param>
        /// <param name="infoLength"></param>
        /// <returns> the next position in the info that can be written to.</returns>
        public int Encode(ref byte[] info, int pos)
        {
            foreach (var node in _listOfParameters)
                pos = node.Value.Encode(ref info, pos);

            return pos;
        }

        /// <summary> Xml tag without the brackets </summary>
        /// <returns> string representing this objects name </returns>
        public string LongTag()
        {
            return "tlvtree ver='1.0'";
        }

        /// <summary>  Get EMVCo formatted XML </summary>
        /// <returns> xml string</returns>
        public string GetXml()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<tlvtree ver='1.0'><node tag='0x0'>");

            foreach (var node in _listOfParameters)
            {
                sb.Append(node.Value.GetXML());
            }

            sb.AppendLine("</node></tlvtree>");
            return sb.ToString();
        }

        /// <summary> Parse the Tag and Length portion of the TLV </summary>
        /// <param name="gIndex">index of where to start</param>
        /// <param name="infoBytes">bytes array with the data</param>
        /// <param name="tagName"> out parameter the resulting tag</param>
        /// <param name="tagLength">out parameter the length of the value for this tag</param>
        /// <returns> next index position after reading the tag info</returns>
        private int getTag(int gIndex, byte[] infoBytes, ref string tagName, ref int tagLength, ref bool tagExtender)
        {
            tagName = infoBytes[gIndex].ToString("X2");

            // If the first byte is "long form" we have at minimum 2 byte tag
            if (checkTLVFirstByte(infoBytes[gIndex++]))
            {
                do
                {
                    tagName += infoBytes[gIndex].ToString("X2");
                } while (IsBitSet(infoBytes[gIndex++], 7));  // Extended byte notes another byte.
            }

            // This is an indicator of some sort.  Not part of the Tag but indicates a length extension.
            if (infoBytes[gIndex] == 0x82)
            {
                tagExtender = true;
                gIndex++;  // skip the tag
                tagLength = EndianBitConverter.Big.ToInt16(infoBytes, gIndex);
                gIndex += 2;
            }
            else if (infoBytes[gIndex] == 0x81)
            {
                tagExtender = true;
                gIndex++;  // skip the tag
                tagLength = Convert.ToInt16(infoBytes[gIndex]);
                gIndex += 1;
            }
            else
                tagLength = Convert.ToInt16(infoBytes[gIndex++]);

            return gIndex;
        }

        /// <summary>
        /// Function to determine if the first TAG byte of BER-TLV Data indicates a 2nd tag byte follows
        /// </summary>
        /// <param name="stestByte"></param>
        /// <returns>bool </returns>
        private bool checkTLVFirstByte(byte testByte)
        {
            return (IsBitSet(testByte, 4) && IsBitSet(testByte, 3) && IsBitSet(testByte, 2) &&
                    IsBitSet(testByte, 1) && IsBitSet(testByte, 0));

        }

        /// <summary>
        /// Method to check if a bit is set in a byte
        /// </summary>
        /// <param name="b">Byte</param>
        /// <param name="pos">Position</param>
        /// <returns></returns>
        private bool IsBitSet(byte b, int pos)
        {
            return (b & (1 << pos)) != 0;
        }
    }
}
