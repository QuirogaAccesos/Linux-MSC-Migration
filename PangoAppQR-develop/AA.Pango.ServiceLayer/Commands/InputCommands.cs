namespace AA.Pango.ServiceLayer.Commands
{
    internal class InputCommands
    {
        public const char CommandSeparator = '$'; //0x24
        public const char CommandInnerSeparator = '-'; //0x96?

        public static readonly byte[] OpenBarrier = { 0x41 }; //A
        public static readonly byte[] KeepBarrierOpened = { 0x49 }; //I
        public static readonly byte[] DeactivateKeepBarrierOpened = { 0x51 }; //I

        public static readonly byte[] TurnOnButtonLight = { 0x42 }; //B
        public static readonly byte[] KeepButtonLightOn = { 0x4A }; //J
        public static readonly byte[] KeepButtonLightOff = { 0x52 }; //R

        public static readonly byte[] TurnOnCompassAntenna = { 0x43 }; //C
        public static readonly byte[] KeepCompassAntennaOn = { 0x4B }; //K
        public static readonly byte[] KeepCompassAntennaOff = { 0x53 }; //S

        public static readonly byte[] CloseBarrier = { 0x44 }; //D
        public static readonly byte[] KeepBarrierClosed = { 0x4C }; //L
        public static readonly byte[] DeactivateKeepBarrierClosed = { 0x54 }; //T

        public static readonly byte[] TurnLedMatrixOn = { 0x45 }; //E //RESET
        public static readonly byte[] KeepLedMatrixOn = { 0x4D }; //M
        public static readonly byte[] DeactivateKeepLedMatrixOn = { 0x55 }; //U

        public static readonly byte[] TurnTrafficLightOn = { 0x46 }; //F
        public static readonly byte[] SendTicketDeniedOnlineSignal = { 0x4E }; //N
        public static readonly byte[] DeactivateKeepTrafficLightOn = { 0x56 }; //V

        //additional output #1 (doesn't do anything yet)
        public static readonly byte[] TurnGPOOn = { 0x47 }; //G

        public static readonly byte[] KeepGPOOn = { 0x4F }; //O
        public static readonly byte[] DeactivateKeepGPOOn = { 0x57 }; //W

        //additional output #2 (doesn't do anything yet)
        public static readonly byte[] TurnGPO2On = { 0x48 }; //H //NEW RESET

        public static readonly byte[] KeepGPO2On = { 0x50 }; //P
        public static readonly byte[] DeactivateKeepGPO2On = { 0x58 }; //X
    }
}