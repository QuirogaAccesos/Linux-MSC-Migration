using CCI.Globalcom.GlobalcomRetailProtocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CCI.Globalcom.GlobalcomRetailProtocol.RpResponseTrackType;

namespace CCI.Globalcom.GlobalcomRetailProtocol
{
    /// <summary>
    /// Response Class for Track Type requests
    /// </summary>
    public class RpResponseTrackType : RpResponseBase
    {
        /// <summary>
        ///  Very specific to Interac and the required language.
        /// </summary>
        public enum ECardHolderPreference { NoneSelected, English, French };

        public byte TrackType;
        public bool Iso2Banking;
        public bool Iso2NotBanking;
        public bool Iso3Bancomat;
        public bool Iso3NotBanking;
        public bool Iso1NotBanking;
        public bool EmvCard;
        public bool ContactlessCard;
        public bool Iso1Banking;
        public bool ChipPresent;
        public bool HostManagesTrack2;
        public bool NoSlotsAvailableForPreAuth;
        public bool VisaFleet2;
        public enum EVisaFleet2Restrictions { None, FuelMaintenance, FuelOnly, FullChipVisa2 };

        public EVisaFleet2Restrictions VisaFleet2Restrictions;

        public ECardHolderPreference CardHolderPreference;

        /// <summary>
        /// List of Tags the submitted card requires for authorization.
        /// </summary>
        public List<EPetroPromptTags> PromptTags = new List<EPetroPromptTags>();

        public RpResponseTrackType(byte[] response)
            : base(response)
        {
            if (VerifyOutcome() == EOutcome.OK)
            {
                int promptPos = 3;

                TrackType = response[6];
                Iso2Banking = IsBitSet(TrackType, 0);
                Iso2NotBanking = IsBitSet(TrackType, 1);
                Iso3Bancomat = IsBitSet(TrackType, 2);
                Iso3NotBanking = IsBitSet(TrackType, 3);
                Iso1NotBanking = IsBitSet(TrackType, 4);
                EmvCard = IsBitSet(TrackType, 5);
                ContactlessCard = IsBitSet(TrackType, 6);
                Iso1Banking = IsBitSet(TrackType, 7);


                // Default
                CardHolderPreference = ECardHolderPreference.NoneSelected;

                if (InfoLength > 2) //infolength of 3 is outcome + track type + chip presence
                {
                    byte InfoData = response[7];
                    ChipPresent = IsBitSet(InfoData, 0);
                    HostManagesTrack2 = IsBitSet(InfoData, 1);
                    NoSlotsAvailableForPreAuth = IsBitSet(InfoData, 2);

                    if ( IsBitSet(InfoData, 4) && IsBitSet(InfoData, 3) )
                        CardHolderPreference = ECardHolderPreference.French;
                    else if ( IsBitSet(InfoData, 4) && !IsBitSet(InfoData, 3) )
                        CardHolderPreference = ECardHolderPreference.English;

                    VisaFleet2 = (!IsBitSet(InfoData, 6) && IsBitSet(InfoData, 5)) ;
                }


                int pos=12; // prompts header
                if (InfoLength > promptPos) //greater than 3 means we have petro tags.
                {
                    // skip the header 'PetroTags:'
                    for (pos = 12; pos < (InfoLength - 2); pos += 2)
                    {
                        string hexstring = Encoding.ASCII.GetString(Info, pos, 2).ToUpper();
                        if (hexstring == "\r\n")
                        {
                            pos += 2;
                            break;
                        }

                        PromptTags.Add(EnumHelper.GetEnumValue<EPetroPromptTags>(Convert.ToInt32(hexstring, 16)));
                    }
                }

                if (pos < Info.Length)
                {
                    // read Visa2 restriction byte
                    byte VisaRest = Info[pos];
                    switch (VisaRest)
                    {
                        case byte n when (!IsBitSet(n, 1) && !IsBitSet(n, 0)):
                            VisaFleet2Restrictions = EVisaFleet2Restrictions.None;
                            break;
                        case byte n when (!IsBitSet(n, 1) && IsBitSet(n, 0)):
                            VisaFleet2Restrictions = EVisaFleet2Restrictions.FuelMaintenance;
                            break;
                        case byte n when (IsBitSet(n, 1) && !IsBitSet(n, 0)):
                            VisaFleet2Restrictions = EVisaFleet2Restrictions.FuelOnly;
                            break;
                        case byte n when (IsBitSet(n, 1) && IsBitSet(n, 0)):
                            VisaFleet2Restrictions = EVisaFleet2Restrictions.FullChipVisa2;
                            break;
                    }
                }
            }
        }

        public string ToXmlString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(VerifyOutcome().ToString() + " ");

            sb.Append(Iso2Banking ? "<Iso2Banking/>" : "");
            sb.Append(Iso2NotBanking ? "<Iso2NotBanking/>" : "");
            sb.Append(Iso3Bancomat ? "<Iso3Bancomat/>" : "");
            sb.Append(Iso3NotBanking ? "<Iso3NotBanking/>" : "");
            sb.Append(Iso1NotBanking ? "<Iso1NotBanking/>" : "");
            sb.Append(EmvCard ? "<EmvCard/>" : "");
            sb.Append(ContactlessCard ? "<ContactlessCard/>" : "");
            sb.Append(Iso1Banking ? "<Iso1Banking/>" : "");
            sb.Append(ChipPresent ? "<ChipPresent/>" : "");
            sb.Append(HostManagesTrack2 ? "<HostManagesTrack2/>" : "");
            sb.Append(NoSlotsAvailableForPreAuth ? "<NoSlotsAvailableForPreAuth/>" : "");
            sb.Append(VisaFleet2 ? "<VisaFleet2Enabled/>" : "");

            switch (CardHolderPreference)
            {
                case ECardHolderPreference.NoneSelected:
                    sb.Append("<LangPref>None");
                    break;

                case ECardHolderPreference.French:
                    sb.Append("<LangPref>French");
                    break;

                case ECardHolderPreference.English:
                    sb.Append("<LangPref>English");
                    break;
            }

            if (VisaFleet2)
                switch (VisaFleet2Restrictions)
                {
                    case EVisaFleet2Restrictions.None:
                        sb.Append("<Visa2Fleet>No Restriction");
                        break;

                    case EVisaFleet2Restrictions.FuelMaintenance:
                        sb.Append("<Visa2Fleet>Fuel and Maintanence Only");
                        break;

                    case EVisaFleet2Restrictions.FuelOnly:
                        sb.Append("<Visa2Fleet>Fuel Only");
                        break;

                    case EVisaFleet2Restrictions.FullChipVisa2:
                        sb.Append("<Visa2Fleet>See Chip Tags");
                        break;

                }

            return sb.ToString();
        }

        public string GetFleetInfoAsString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(VerifyOutcome().ToString() + " ");
            sb.Append(VisaFleet2 ? "VisaFleet2Enabled " : "");

            if (VisaFleet2)
                switch (VisaFleet2Restrictions)
                {
                    case EVisaFleet2Restrictions.None:
                        sb.Append("with No Restriction");
                        break;

                    case EVisaFleet2Restrictions.FuelMaintenance:
                        sb.Append("with Fuel and Maintanence Only");
                        break;

                    case EVisaFleet2Restrictions.FuelOnly:
                        sb.Append("with Fuel Only");
                        break;

                    case EVisaFleet2Restrictions.FullChipVisa2:
                        sb.Append("with See Chip Tags");
                        break;

                }

            return sb.ToString();
        }

    }
    }
