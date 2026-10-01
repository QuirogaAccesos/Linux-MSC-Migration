using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCI.Globalcom.GlobalcomRetailProtocol
{
    /// <summary>
    /// Response Class for Device Info Requests
    /// </summary>
    public class RpResponseDeviceInfo : RpResponseBase
    {
        public bool Tamp0Violation;
        public bool Tamp1Violation;
        public bool Tamp2Violation;
        public bool Tamp3Violation;
        public bool TampRViolation;
        public bool MeshRViolation;
        public bool VoltageViolation;
        public bool FrequencyViolation;
        public bool TempViolation;
        public bool MeshAbCdViolation;
        public bool Tamp0123Violation;
        public bool CRCViolation;
        public bool GeneralViolation;
        public bool TamperViolation;
        public ESensorStatus SensorStatus;
        public bool MagneticError;
        public bool CryptoToolsError;
        public bool CardPresent;
        public bool TrackPresent;
        public bool BuzzerEnabled;
        public bool CardInserted;
        public bool CardPresent2;
        public bool TrackPresent2;
        public bool BuzzerEnabled2;
        public bool CardInserted2;

        public ESensorStatus AntiRemovalStatus;
        public bool Sw6Violated;
        public bool Sw7Violated;
        public bool Sw6MemViolated;
        public bool Sw7MemViolated;
        public EParingStatus ParingStatus;
        public bool PinPadConnected;

        /// <summary> Constructor for getting Device Info</summary>
        /// <param name="response"> info response from the reader</param>
        public RpResponseDeviceInfo(byte[] response)
            : base(response)
        {
            if (VerifyOutcome() == EOutcome.OK)
            {
                Tamp0Violation = IsBitSet(response[6], 0); //00
                Tamp1Violation = IsBitSet(response[6], 1);
                Tamp2Violation = IsBitSet(response[6], 2);
                Tamp3Violation = IsBitSet(response[6], 3);
                TampRViolation = IsBitSet(response[6], 4);
                MeshRViolation = IsBitSet(response[6], 5);

                VoltageViolation = IsBitSet(response[7], 0); // 00
                FrequencyViolation = IsBitSet(response[7], 1);
                TempViolation = IsBitSet(response[7], 2);
                MeshAbCdViolation = IsBitSet(response[7], 3);
                Tamp0123Violation = IsBitSet(response[7], 4);
                CRCViolation = IsBitSet(response[7], 5);
                GeneralViolation = IsBitSet(response[7], 6);
                TamperViolation = IsBitSet(response[7], 7);

                SensorStatus = (IsBitSet(response[8], 1)) //01
                    ? ESensorStatus.Violated
                    : ((IsBitSet(response[8], 0)) ? ESensorStatus.Active : ESensorStatus.NotActive);

                MagneticError = IsBitSet(response[8], 2);
                CryptoToolsError = IsBitSet(response[8], 3);

                CardPresent = IsBitSet(response[9], 0); //06
                TrackPresent = IsBitSet(response[9], 1);
                BuzzerEnabled = IsBitSet(response[9], 2);
                CardInserted = IsBitSet(response[9], 3);
                /*
                            CardPresent2 = IsBitSet(response[10], 0); //01
                            TrackPresent2 = IsBitSet(response[10], 1);
                            BuzzerEnabled2 = IsBitSet(response[10], 2);
                            CardInserted2 = IsBitSet(response[10], 3);
                */
                AntiRemovalStatus = (IsBitSet(response[10], 1)) //00
                    ? ESensorStatus.Violated
                    : ((IsBitSet(response[10], 0)) ? ESensorStatus.Active : ESensorStatus.NotActive);

                Sw6Violated = IsBitSet(response[11], 0); //00
                Sw7Violated = IsBitSet(response[11], 1);

                Sw6MemViolated = IsBitSet(response[12], 0); //01
                Sw7MemViolated = IsBitSet(response[12], 1);

                ParingStatus = (IsBitSet(response[13], 1)) //01
                    ? EParingStatus.Violated
                    : ((IsBitSet(response[13], 0)) ? EParingStatus.Paired : EParingStatus.ToBePaired);

                PinPadConnected = (IsBitSet(response[14], 0));
            }
        }

        /// <summary>
        /// Return a print friendly string related to a Master Device
        /// </summary>
        [Obsolete("PrintMasterDetails is deprecated, please use PrintDetails(TypeOfDevice) instead.")]
        public string PrintMasterDetails()
        {
            StringBuilder sb = new StringBuilder(1000);

            sb.AppendLine("Sensors:");
            sb.AppendLine("--> Master:");
            sb.AppendLine("----->: Sensor Status:" + (SensorStatus.ToString()));
            // good = (rpiInfo.SensorStatus == ESensorStatus.Active)
            sb.AppendLine("-----> Voltage:" + ((VoltageViolation) ? "Violated" : "OK"));
            sb.AppendLine("-----> Temperature:" + ((TempViolation) ? "Violated" : "OK"));
            sb.AppendLine("-----> CRC:" + ((CRCViolation) ? "Violated" : "OK"));
            sb.AppendLine("-----> Crypto Tools:" + ((CryptoToolsError) ? "Violated" : "OK"));
            sb.AppendLine("-----> Frequency:" + ((FrequencyViolation) ? "Violated" : "OK"));
            sb.AppendLine("-----> Buzzer Enabled:" + ((BuzzerEnabled) ? "Enabled" : "Disabled"));
            sb.AppendLine("-----> Card Present:" + ((CardPresent) ? "Present" : "Not Present"));
            sb.AppendLine("-----> Card Inserted:" + ((CardInserted) ? "Inserted" : "Not Inserted"));
            sb.AppendLine("-----> General Violation:" + ((GeneralViolation) ? "Violated" : "OK"));
            sb.AppendLine("-----> Mag Head:" + ((MagneticError) ? "Violated" : "OK"));
            sb.AppendLine("-----> MESH ABCD:" + ((MeshAbCdViolation) ? "Violated" : "OK"));
            sb.AppendLine("-----> AntiRemoval:" + AntiRemovalStatus.ToString());
            sb.AppendLine("--------> SW6 (Current State):" + ((Sw6Violated) ? "Violated" : "OK"));
            sb.AppendLine("--------> SW7 (Current State):" + ((Sw7Violated) ? "Violated" : "OK"));
            sb.AppendLine("--------> SW6MEM:" + ((Sw6MemViolated) ? "Violated" : "OK"));
            sb.AppendLine("--------> SW7MEM:" + ((Sw7MemViolated) ? "Violated" : "OK"));
            sb.AppendLine("-----> Tamper:" + ((TamperViolation) ? "Violated" : "OK"));
            sb.AppendLine("-----> Tamper0123:" + ((Tamp0123Violation) ? "Violated" : "OK"));
            sb.AppendLine("--------> Tamp 0:" + ((Tamp0Violation) ? "Violated" : "OK"));
            sb.AppendLine("--------> Tamp 1:" + ((Tamp1Violation) ? "Violated" : "OK"));
            sb.AppendLine("--------> Tamp 2:" + ((Tamp2Violation) ? "Violated" : "OK"));
            sb.AppendLine("--------> Tamp 3:" + ((Tamp3Violation) ? "Violated" : "OK"));
            sb.AppendLine("-----> PinPad:" + ((PinPadConnected) ? "Connected" : "No"));
            sb.AppendLine("-----> Pairing:" + (ParingStatus.ToString()));
            sb.AppendLine("-----> Mag Tracks:" + ((TrackPresent) ? "Present" : "Not Present"));
            sb.AppendLine(Environment.NewLine);

            return sb.ToString();
        }

        /// <summary>
        /// Return a print friendly string related to a Pin Pad Device
        /// </summary>
        [Obsolete("PrintPinPadDetails is deprecated, please use PrintDetails(TypeOfDevice) instead.", false)]
        public string PrintPinPadDetails()
        {
            StringBuilder sb = new StringBuilder(1000);

            sb.AppendLine("--> PinPad:");
            sb.AppendLine("-----> Sensor Status:" + (SensorStatus.ToString()));
            // good = (rpiInfo.SensorStatus == ESensorStatus.Active)
            sb.AppendLine("-----> Voltage:" + ((VoltageViolation) ? "Violated" : "OK"));
            sb.AppendLine("-----> Temperature:" + ((TempViolation) ? "Violated" : "OK"));
            sb.AppendLine("-----> CRC:" + ((CRCViolation) ? "Violated" : "OK"));
            sb.AppendLine("-----> Crypto Tools:" + ((CryptoToolsError) ? "Violated" : "OK"));
            sb.AppendLine("-----> Frequency:" + ((FrequencyViolation) ? "Violated" : "OK"));
            sb.AppendLine("-----> Buzzer Enabled:" + ((BuzzerEnabled) ? "Enabled" : "Disabled"));
            sb.AppendLine("-----> Card Present:" + ((CardPresent) ? "Present" : "Not Present"));
            sb.AppendLine("-----> Card Inserted:" + ((CardInserted) ? "Inserted" : "Not Inserted"));
            sb.AppendLine("-----> General Violation:" + ((GeneralViolation) ? "Violated" : "OK"));
            sb.AppendLine("-----> Mag Head:" + ((MagneticError) ? "Violated" : "OK"));
            sb.AppendLine("-----> MESH ABCD:" + ((MeshAbCdViolation) ? "Violated" : "OK"));
            sb.AppendLine("-----> AntiRemoval:" + AntiRemovalStatus.ToString());
            sb.AppendLine("--------> SW6 (Current State):" + ((Sw6Violated) ? "Violated" : "OK"));
            sb.AppendLine("--------> SW7 (Current State):" + ((Sw7Violated) ? "Violated" : "OK"));
            sb.AppendLine("--------> SW6MEM:" + ((Sw6MemViolated) ? "Violated" : "OK"));
            sb.AppendLine("--------> SW7MEM:" + ((Sw7MemViolated) ? "Violated" : "OK"));
            sb.AppendLine("-----> Tamper:" + ((TamperViolation) ? "Violated" : "OK"));
            sb.AppendLine("-----> Tamper0123:" + ((Tamp0123Violation) ? "Violated" : "OK"));
            sb.AppendLine("--------> Tamp 0:" + ((Tamp0Violation) ? "Violated" : "OK"));
            sb.AppendLine("--------> Tamp 1:" + ((Tamp1Violation) ? "Violated" : "OK"));
            sb.AppendLine("--------> Tamp 2:" + ((Tamp2Violation) ? "Violated" : "OK"));
            sb.AppendLine("--------> Tamp 3:" + ((Tamp3Violation) ? "Violated" : "OK"));
            sb.AppendLine("-----> Pairing:" + (ParingStatus.ToString()));
            sb.AppendLine("-----> Mag Tracks:" + ((TrackPresent) ? "Present" : "Not Present"));
            sb.AppendLine(Environment.NewLine);

            return sb.ToString();
        }

        /// <summary>
        /// Return a print friendly string related to a NFC Device
        /// </summary>
        [Obsolete("PrintNFCDetails is deprecated, please use PrintDetails(TypeOfDevice) instead.", false)]
        public string PrintNFCDetails()
        {
            StringBuilder sb = new StringBuilder(1000);

            sb.AppendLine("--> Contactless:");
            sb.AppendLine("-----> Sensor Status:" + (SensorStatus.ToString()));
            // good = (rpiInfo.SensorStatus == ESensorStatus.Active)
            sb.AppendLine("-----> Voltage:" + ((VoltageViolation) ? "Violated" : "OK"));
            sb.AppendLine("-----> Temperature:" + ((TempViolation) ? "Violated" : "OK"));
            sb.AppendLine("-----> CRC:" + ((CRCViolation) ? "Violated" : "OK"));
            sb.AppendLine("-----> Crypto Tools:" + ((CryptoToolsError) ? "Violated" : "OK"));
            sb.AppendLine("-----> Frequency:" + ((FrequencyViolation) ? "Violated" : "OK"));
            sb.AppendLine("-----> Buzzer Enabled:" + ((BuzzerEnabled) ? "Enabled" : "Disabled"));
            sb.AppendLine("-----> Card Present:" + ((CardPresent) ? "Present" : "Not Present"));
            sb.AppendLine("-----> Card Inserted:" + ((CardInserted) ? "Inserted" : "Not Inserted"));
            sb.AppendLine("-----> General Violation:" + ((GeneralViolation) ? "Violated" : "OK"));
            sb.AppendLine("-----> Mag Head:" + ((MagneticError) ? "Violated" : "OK"));
            sb.AppendLine("-----> MESH ABCD:" + ((MeshAbCdViolation) ? "Violated" : "OK"));
            sb.AppendLine("-----> Tamper:" + ((TamperViolation) ? "Violated" : "OK"));
            sb.AppendLine("-----> Tamper0123:" + ((Tamp0123Violation) ? "Violated" : "OK"));
            sb.AppendLine("--------> Tamp 0:" + ((Tamp0Violation) ? "Violated" : "OK"));
            sb.AppendLine("--------> Tamp 1:" + ((Tamp1Violation) ? "Violated" : "OK"));
            sb.AppendLine("--------> Tamp 2:" + ((Tamp2Violation) ? "Violated" : "OK"));
            sb.AppendLine("--------> Tamp 3:" + ((Tamp3Violation) ? "Violated" : "OK"));
            sb.AppendLine("-----> Pairing:" + (ParingStatus.ToString()));
            sb.AppendLine("-----> Mag Tracks:" + ((TrackPresent) ? "Present" : "Not Present"));
            sb.AppendLine(Environment.NewLine);

            return sb.ToString();
        }

        /// <summary> This Response class can be used by the following BV1000 types </summary>
        public enum TypeOfDevice
        {
            Master,
            PinPad,
            NFC
        };

        /// <summary>
        /// Return a print friendly string related to a Master Device
        /// </summary>
        /// <param name="eType"></param>
        /// <returns></returns>
        public string PrintDetails(TypeOfDevice eType)
        {
            if (VerifyOutcome() != EOutcome.OK)
                return "Sensors : No External " + eType.ToString() + Environment.NewLine;

            StringBuilder sb = new StringBuilder(1000);

            sb.AppendLine("Sensors:");
            sb.AppendLine("--> " + eType.ToString() + ":");
            sb.AppendLine("----->: Sensor Status:" + (SensorStatus.ToString()));
            // good = (rpiInfo.SensorStatus == ESensorStatus.Active)
            sb.AppendLine("-----> Voltage:" + ((VoltageViolation) ? "Violated" : "OK"));
            sb.AppendLine("-----> Temperature:" + ((TempViolation) ? "Violated" : "OK"));
            sb.AppendLine("-----> CRC:" + ((CRCViolation) ? "Violated" : "OK"));
            sb.AppendLine("-----> Crypto Tools:" + ((CryptoToolsError) ? "Violated" : "OK"));
            sb.AppendLine("-----> Frequency:" + ((FrequencyViolation) ? "Violated" : "OK"));
            sb.AppendLine("-----> Buzzer Enabled:" + ((BuzzerEnabled) ? "Enabled" : "Disabled"));
            sb.AppendLine("-----> Card Present:" + ((CardPresent) ? "Present" : "Not Present"));
            sb.AppendLine("-----> Card Inserted:" + ((CardInserted) ? "Inserted" : "Not Inserted"));
            sb.AppendLine("-----> General Violation:" + ((GeneralViolation) ? "Violated" : "OK"));
            sb.AppendLine("-----> TamperR Violation:" + ((TampRViolation) ? "Violated" : "OK"));
            sb.AppendLine("-----> MeshR Violation:" + ((MeshRViolation) ? "Violated" : "OK"));

            sb.AppendLine("-----> Mag Head:" + ((MagneticError) ? "Violated" : "OK"));
            sb.AppendLine("-----> MESH ABCD:" + ((MeshAbCdViolation) ? "Violated" : "OK"));

            if (eType != TypeOfDevice.NFC)
            {
                sb.AppendLine("-----> AntiRemoval:" + AntiRemovalStatus.ToString());
                sb.AppendLine("--------> SW6 (Current State):" + ((Sw6Violated) ? "Violated" : "OK"));
                sb.AppendLine("--------> SW7 (Current State):" + ((Sw7Violated) ? "Violated" : "OK"));
                sb.AppendLine("--------> SW6MEM:" + ((Sw6MemViolated) ? "Violated" : "OK"));
                sb.AppendLine("--------> SW7MEM:" + ((Sw7MemViolated) ? "Violated" : "OK"));
            }

            sb.AppendLine("-----> Tamper:" + ((TamperViolation) ? "Violated" : "OK"));
            sb.AppendLine("-----> Tamper0123:" + ((Tamp0123Violation) ? "Violated" : "OK"));
            sb.AppendLine("--------> Tamp 0:" + ((Tamp0Violation) ? "Violated" : "OK"));
            sb.AppendLine("--------> Tamp 1:" + ((Tamp1Violation) ? "Violated" : "OK"));
            sb.AppendLine("--------> Tamp 2:" + ((Tamp2Violation) ? "Violated" : "OK"));
            sb.AppendLine("--------> Tamp 3:" + ((Tamp3Violation) ? "Violated" : "OK"));

            if (eType == TypeOfDevice.Master)
                sb.AppendLine("-----> PinPad:" + ((PinPadConnected) ? "Connected" : "No"));

            sb.AppendLine("-----> Pairing:" + (ParingStatus.ToString()));
            sb.AppendLine("-----> Mag Tracks:" + ((TrackPresent) ? "Present" : "Not Present"));
            sb.AppendLine(Environment.NewLine);

            return sb.ToString();
        }

        /// <summary>
        /// Return a print friendly string related to a Master Errors
        /// </summary>
        /// <param name="eType"></param>
        /// <returns></returns>
        public string PrintDetailsV2(TypeOfDevice eType)
        {
            if (VerifyOutcome() != EOutcome.OK)
                return "Sensors: No External " + eType.ToString() + Environment.NewLine;

            StringBuilder sb = new StringBuilder(1000);

            sb.AppendLine("Sensors: " + eType.ToString());
            sb.AppendLine("-----> Status:" + (SensorStatus.ToString()));
            // good = (rpiInfo.SensorStatus == ESensorStatus.Active)
            if (VoltageViolation)  sb.AppendLine("-----> Voltage:" + ((VoltageViolation) ? "############### Violated" : "OK"));
            if (TempViolation) sb.AppendLine("-----> Temperature:" + ((TempViolation) ? "############### Violated" : "OK"));
            if (CRCViolation) sb.AppendLine("-----> CRC:" + ((CRCViolation) ? "############### Violated" : "OK"));
            if (CryptoToolsError) sb.AppendLine("-----> Crypto Tools:" + ((CryptoToolsError) ? "############### Violated" : "OK"));
            if (FrequencyViolation) sb.AppendLine("-----> Frequency:" + ((FrequencyViolation) ? "############### Violated" : "OK"));
            if (!BuzzerEnabled) sb.AppendLine("-----> Buzzer:" + ((BuzzerEnabled) ? "Enabled" : "Disabled"));
            if (CardPresent) sb.AppendLine("-----> Card Present:" + ((CardPresent) ? "Present" : "Not Present"));
            if (CardInserted) sb.AppendLine("-----> Card Inserted:" + ((CardInserted) ? "Inserted" : "Not Inserted"));
            if (GeneralViolation) sb.AppendLine("-----> General Violation:" + ((GeneralViolation) ? "############### Violated" : "OK"));
            if (TampRViolation) sb.AppendLine("-----> TamperR Violation:" + ((TampRViolation) ? "############### Violated" : "OK"));
            if (MeshRViolation) sb.AppendLine("-----> MeshR Violation:" + ((MeshRViolation) ? "############### Violated" : "OK"));

            if (MagneticError) sb.AppendLine("-----> Mag Head:" + ((MagneticError) ? "############### Violated" : "OK"));
            if (MeshAbCdViolation) sb.AppendLine("-----> MESH ABCD:" + ((MeshAbCdViolation) ? "############### Violated" : "OK"));

            if (eType != TypeOfDevice.NFC)
            {
                if (AntiRemovalStatus != ESensorStatus.Active) sb.AppendLine("-----> AntiRemoval: ############### " + AntiRemovalStatus.ToString());
                if (Sw6Violated) sb.AppendLine("--------> SW6 (Current State):" + ((Sw6Violated) ? "############### Left Pin Not Latched, Needs Tension" : "OK"));
                if (Sw7Violated) sb.AppendLine("--------> SW7 (Current State):" + ((Sw7Violated) ? "############### Right Pin Not Latched, Needs Tension" : "OK"));
                if (Sw6MemViolated) sb.AppendLine("--------> SW6 MEM:" + ((Sw6MemViolated) ? "############### Violated, Code Required" : "OK"));
                if (Sw7MemViolated) sb.AppendLine("--------> SW7 MEM:" + ((Sw7MemViolated) ? "############### Violated, Code Required" : "OK"));
            }

            if (TamperViolation) sb.AppendLine("-----> Tamper:" + ((TamperViolation) ? "############### Violated" : "OK"));
            if (Tamp0123Violation) sb.AppendLine("-----> Tamper0123:" + ((Tamp0123Violation) ? "############### Violated" : "OK"));
            if (Tamp0Violation) sb.AppendLine("--------> Tamp 0:" + ((Tamp0Violation) ? "############### Violated" : "OK"));
            if (Tamp1Violation) sb.AppendLine("--------> Tamp 1:" + ((Tamp1Violation) ? "############### Violated" : "OK"));
            if (Tamp2Violation) sb.AppendLine("--------> Tamp 2:" + ((Tamp2Violation) ? "############### Violated" : "OK"));
            if (Tamp3Violation) sb.AppendLine("--------> Tamp 3:" + ((Tamp3Violation) ? "############### Violated" : "OK"));

            if (eType == TypeOfDevice.Master)
                sb.AppendLine("-----> PinPad:" + ((PinPadConnected) ? "Connected" : "No"));

            sb.AppendLine("-----> Pairing:" + (ParingStatus.ToString()));
            sb.AppendLine("-----> Mag Tracks:" + ((TrackPresent) ? "Present" : "Not Present"));
            sb.AppendLine();

            return sb.ToString();
        }

        /// <summary>
        /// Return a print friendly string related to a Master Errors
        /// </summary>
        /// <param name="eType"></param>
        /// <returns></returns>
        public string PrintErrorDetails(TypeOfDevice eType)
        {
            if (VerifyOutcome() != EOutcome.OK)
                return "Sensors: No External " + eType.ToString() + Environment.NewLine;

            StringBuilder sb = new StringBuilder(1000);

            sb.AppendLine("Sensors: " + eType.ToString());
            sb.AppendLine("-----> Status:" + (SensorStatus.ToString()));
            // good = (rpiInfo.SensorStatus == ESensorStatus.Active)
            if (VoltageViolation) sb.AppendLine("-----> Voltage:" + ((VoltageViolation) ? "Violated" : "OK"));
            if (TempViolation) sb.AppendLine("-----> Temperature:" + ((TempViolation) ? "Violated" : "OK"));
            if (CRCViolation) sb.AppendLine("-----> CRC:" + ((CRCViolation) ? "Violated" : "OK"));
            if (CryptoToolsError) sb.AppendLine("-----> Crypto Tools:" + ((CryptoToolsError) ? "Violated" : "OK"));
            if (FrequencyViolation) sb.AppendLine("-----> Frequency:" + ((FrequencyViolation) ? "Violated" : "OK"));
            if (!BuzzerEnabled) sb.AppendLine("-----> Buzzer Enabled:" + ((BuzzerEnabled) ? "Enabled" : "Disabled"));
            if (CardPresent) sb.AppendLine("-----> Card Present:" + ((CardPresent) ? "Present" : "Not Present"));
            if (CardInserted) sb.AppendLine("-----> Card Inserted:" + ((CardInserted) ? "Inserted" : "Not Inserted"));
            if (GeneralViolation) sb.AppendLine("-----> General Violation:" + ((GeneralViolation) ? "Violated" : "OK"));
            if (TampRViolation) sb.AppendLine("-----> TamperR Violation:" + ((TampRViolation) ? "Violated" : "OK"));
            if (MeshRViolation) sb.AppendLine("-----> MeshR Violation:" + ((MeshRViolation) ? "Violated" : "OK"));

            if (MagneticError) sb.AppendLine("-----> Mag Head:" + ((MagneticError) ? "Violated" : "OK"));
            if (MeshAbCdViolation) sb.AppendLine("-----> MESH ABCD:" + ((MeshAbCdViolation) ? "Violated" : "OK"));

            if (eType != TypeOfDevice.NFC)
            {
                if (AntiRemovalStatus != ESensorStatus.Active) sb.AppendLine("-----> AntiRemoval:" + AntiRemovalStatus.ToString());
                if (Sw6Violated) sb.AppendLine("--------> SW6 (Current State):" + ((Sw6Violated) ? "Left Pin Not Latched, Needs Tension\" " : "OK"));
                if (Sw7Violated) sb.AppendLine("--------> SW7 (Current State):" + ((Sw7Violated) ? "Right Pin Not Latched, Needs Tension\" " : "OK"));
                if (Sw6MemViolated) sb.AppendLine("--------> SW6MEM:" + ((Sw6MemViolated) ? "Violated, Code Required" : "OK"));
                if (Sw7MemViolated) sb.AppendLine("--------> SW7MEM:" + ((Sw7MemViolated) ? "Violated, Code Required" : "OK"));
            }

            if (TamperViolation) sb.AppendLine("-----> Tamper:" + ((TamperViolation) ? "Violated" : "OK"));
            if (Tamp0123Violation) sb.AppendLine("-----> Tamper0123:" + ((Tamp0123Violation) ? "Violated" : "OK"));
            if (Tamp0Violation) sb.AppendLine("--------> Tamp 0:" + ((Tamp0Violation) ? "Violated" : "OK"));
            if (Tamp1Violation) sb.AppendLine("--------> Tamp 1:" + ((Tamp1Violation) ? "Violated" : "OK"));
            if (Tamp2Violation) sb.AppendLine("--------> Tamp 2:" + ((Tamp2Violation) ? "Violated" : "OK"));
            if (Tamp3Violation) sb.AppendLine("--------> Tamp 3:" + ((Tamp3Violation) ? "Violated" : "OK"));

            sb.AppendLine("-----> Pairing:" + (ParingStatus.ToString()));
            if (TrackPresent) sb.AppendLine("-----> Mag Tracks:" + ((TrackPresent) ? "Present" : "Not Present"));
            sb.AppendLine();

            return sb.ToString();
        }

        /// <summary>
        /// Get a string with the details in a well formatted xml format
        /// </summary>
        /// <param name="eType"> what type of device your expecting to see the details for</param>
        /// <returns></returns>
        public string PrintDetailsAsXml(TypeOfDevice eType)
        {
            StringBuilder sb = new StringBuilder(1000);

            sb.AppendLine("<" + eType.ToString() + "Sensors>");

            sb.AppendLine("  <Status>" + (SensorStatus.ToString()) + "</Status>");
            sb.AppendLine("  <Voltage>" + ((VoltageViolation) ? "Violated" : "OK") + "</Voltage>");
            sb.AppendLine("  <Temperature>" + ((TempViolation) ? "Violated" : "OK") + "</Temperature>");
            sb.AppendLine("  <CRC>" + ((CRCViolation) ? "Violated" : "OK") + "</CRC>");
            sb.AppendLine("  <CryptoTools>" + ((CryptoToolsError) ? "Violated" : "OK") + "</CryptoTools>");
            sb.AppendLine("  <Frequency>" + ((FrequencyViolation) ? "Violated" : "OK") + "</Frequency>");
            sb.AppendLine("  <Buzzer>" + ((BuzzerEnabled) ? "Enabled" : "Disabled") + "</Buzzer>");
            sb.AppendLine("  <CardPresent>" + ((CardPresent) ? "Present" : "Not Present") + "</CardPresent>");
            sb.AppendLine("  <CardInserted>" + ((CardInserted) ? "Inserted" : "Not Inserted") + "</CardInserted>");
            sb.AppendLine("  <GeneralViolation>" + ((GeneralViolation) ? "Violated" : "OK") + "</GeneralViolation>");
            sb.AppendLine("  <TamperRViolation>" + ((TampRViolation) ? "Violated" : "OK") + "</TamperRViolation>");
            sb.AppendLine("  <MeshRViolation>" + ((MeshRViolation) ? "Violated" : "OK") + "</MeshRViolation>");
            sb.AppendLine("  <MagHead>" + ((MagneticError) ? "Violated" : "OK") + "</MagHead>");
            sb.AppendLine("  <MESHABCD>" + ((MeshAbCdViolation) ? "Violated" : "OK") + "</MESHABCD>");

            if (eType != TypeOfDevice.NFC)
            {
                sb.AppendLine("  <AntiRemoval>");
                sb.AppendLine("    <Status>" + AntiRemovalStatus.ToString() + "</Status>");
                sb.AppendLine("    <SW6_CurrentState>" + ((Sw6Violated) ? "Violated" : "OK") + "</SW6_CurrentState>");
                sb.AppendLine("    <SW7_CurrentState>" + ((Sw7Violated) ? "Violated" : "OK") + "</SW7_CurrentState>");
                sb.AppendLine("    <SW6MEM>" + ((Sw6MemViolated) ? "Violated" : "OK") + "</SW6MEM>");
                sb.AppendLine("    <SW7MEM>" + ((Sw7MemViolated) ? "Violated" : "OK") + "</SW7MEM>");
                sb.AppendLine("  </AntiRemoval>");
            }

            sb.AppendLine("  <Tamper>" + ((TamperViolation) ? "Violated" : "OK") + "</Tamper>");
            sb.AppendLine("  <Tamper0123>");
            sb.AppendLine("    <Status>" + ((Tamp0123Violation) ? "Violated" : "OK") + "</Status>");
            sb.AppendLine("    <Tamp0>" + ((Tamp0Violation) ? "Violated" : "OK") + "</Tamp0>");
            sb.AppendLine("    <Tamp1>" + ((Tamp1Violation) ? "Violated" : "OK") + "</Tamp1>");
            sb.AppendLine("    <Tamp2>" + ((Tamp2Violation) ? "Violated" : "OK") + "</Tamp2>");
            sb.AppendLine("    <Tamp3>" + ((Tamp3Violation) ? "Violated" : "OK") + "</Tamp3>");
            sb.AppendLine("  </Tamper0123>");

            if (eType == TypeOfDevice.Master)
                sb.AppendLine("  <PinPad>" + ((PinPadConnected) ? "Connected" : "No") + "</PinPad>");

            sb.AppendLine("  <Pairing>" + (ParingStatus.ToString()) + "</Pairing>");
            sb.AppendLine("  <MagTracks>" + ((TrackPresent) ? "Present" : "Not Present") + "</MagTracks>");

            sb.AppendLine("</" + eType.ToString() + "Sensors>");

            return sb.ToString();
        }
    }

}
