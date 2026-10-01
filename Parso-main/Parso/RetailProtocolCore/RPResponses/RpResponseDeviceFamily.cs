using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCI.Globalcom.GlobalcomRetailProtocol
{
    /// <summary>
    /// Response Class for Extended Device Family Requests (Not supported by BV1000)
    /// </summary>
    public class RpResponseDeviceFamily : RpResponseBase
    {
        public EDeviceFamily DeviceFamily;
        public byte ExtCommandID;
        public byte[] ExtendedInfo;

        public RpResponseDeviceFamily(byte[] response) : base(response)
        {
            if (VerifyOutcome() == EOutcome.UnrecognizedCommand)
            {
                DeviceFamily = EDeviceFamily.BV1000;
            }
            else
            {
                ExtCommandID = Info[0];
                ExtendedInfo = new byte[Info.Length - 1];
                Buffer.BlockCopy(Info, 1, ExtendedInfo, 0, Info.Length - 1);
                string deviceFamily = Encoding.ASCII.GetString(ExtendedInfo);
                //Expect an outcome of 0xE1 - command not recognized for BV1000

                DeviceFamily = (deviceFamily == "HRT1000") ? EDeviceFamily.HRT1000 : EDeviceFamily.BV1000;
            }
        }

        /// <summary>
        /// Friendly String of the Device Family Information
        /// </summary>
        public string PrintDetails()
        {
            return DeviceFamily.ToString();
        }
    }
}
