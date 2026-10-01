using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCI.Globalcom.GlobalcomRetailProtocol
{
    /// <summary>
    /// Response Class for HRT1000 Device Info Requests
    /// </summary>
    public class RpResponseHRT1000DeviceInfo : RpResponseBase
    {
        public byte SubCommand;
        public bool Mesh1Violation;
        public bool Mesh2Violation;
        public bool SW1Violation;
        public bool SW2Violation;
        public bool AR1Violation;
        public bool AR2Violation;

        public bool SHLDMViolation;
        public bool DBLFMViolation;
        public bool TestViolation;
        public bool JTAGViolation;
        public bool MCKMViolation;
        public bool TPMLViolation;
        public bool TPMHViolation;
        public bool VDDBULViolation;
        public bool VDDBUL2Violation;
        public bool VDDCORELViolation;
        public bool VDDCOREHViolation;

        public RpResponseHRT1000DeviceInfo(byte[] response)
            : base(response)
        {
            if (VerifyOutcome() == EOutcome.OK)
            {
                SubCommand = response[6];

                Mesh1Violation = IsBitSet(response[7], 0); //00
                Mesh2Violation = IsBitSet(response[7], 1);
                SW1Violation = IsBitSet(response[7], 2);
                SW2Violation = IsBitSet(response[7], 3);
                AR1Violation = IsBitSet(response[7], 4);
                AR2Violation = IsBitSet(response[7], 5);

                SHLDMViolation = IsBitSet(response[8], 0); // 00
                DBLFMViolation = IsBitSet(response[8], 1);
                TestViolation = IsBitSet(response[8], 2);
                JTAGViolation = IsBitSet(response[8], 3);
                MCKMViolation = IsBitSet(response[8], 4);
                TPMLViolation = IsBitSet(response[8], 5);
                TPMHViolation = IsBitSet(response[8], 6);
                VDDBULViolation = IsBitSet(response[8], 7);

                VDDBUL2Violation = IsBitSet(response[9], 0);
                VDDCORELViolation = IsBitSet(response[9], 1);
                VDDCOREHViolation = IsBitSet(response[9], 2);
            }
        }

        /// <summary>
        /// Return a print friendly string related to a Master Device
        /// </summary>
        public string PrintMasterDetails()
        {
            StringBuilder sb = new StringBuilder(1000);

            sb.AppendLine("HRT1000 Sensors:");
            sb.AppendLine("--> HRT1000:");
            sb.AppendLine("-----> Mesh 1:" + ((Mesh1Violation) ? "Violated" : "OK"));
            sb.AppendLine("-----> Mesh 2:" + ((Mesh2Violation) ? "Violated" : "OK"));
            sb.AppendLine("-----> SW 1:" + ((SW1Violation) ? "Violated" : "OK"));
            sb.AppendLine("-----> SW 2:" + ((SW2Violation) ? "Violated" : "OK"));
            sb.AppendLine("-----> AR 1:" + ((AR1Violation) ? "Violated" : "OK"));
            sb.AppendLine("-----> AR 2:" + ((AR2Violation) ? "Violated" : "OK"));

            sb.AppendLine("-----> SHLDM:" + ((SHLDMViolation) ? "Violated" : "OK"));
            sb.AppendLine("-----> DBLFM:" + ((DBLFMViolation) ? "Violated" : "OK"));
            sb.AppendLine("-----> TEST:" + ((TestViolation) ? "Violated" : "OK"));
            sb.AppendLine("-----> JTAG:" + ((JTAGViolation) ? "Violated" : "OK"));
            sb.AppendLine("-----> MCKM:" + ((MCKMViolation) ? "Violated" : "OK"));
            sb.AppendLine("-----> TPML:" + ((TPMLViolation) ? "Violated" : "OK"));
            sb.AppendLine("-----> TPMH:" + ((TPMHViolation) ? "Violated" : "OK"));
            sb.AppendLine("-----> VDDBUL:" + ((VDDBULViolation) ? "Violated" : "OK"));

            sb.AppendLine("-----> VDDBUL2:" + ((VDDBUL2Violation) ? "Violated" : "OK"));
            sb.AppendLine("-----> VDDCOREL:" + ((VDDCORELViolation) ? "Violated" : "OK"));
            sb.AppendLine("-----> VDDCOREH:" + ((VDDCOREHViolation) ? "Violated" : "OK"));

            sb.AppendLine(Environment.NewLine);

            return sb.ToString();
        }

    }

}
