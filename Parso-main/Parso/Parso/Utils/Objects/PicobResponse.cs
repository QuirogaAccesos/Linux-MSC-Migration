namespace Parso.Utils.Objects
{
    internal class PicobResponse
    {
        private static PicobResponse _instance;
        public static PicobResponse Instance => _instance ??= new PicobResponse();
        private PicobResponse() { }

        public int R { get; set; } = 0;          // Red LEDs
        public int G { get; set; } = 0;          // Green LEDs
        public int B { get; set; } = 0;          // Blue LEDs
        public int C { get; set; } = 0;          // SMA input (1: vehicle present)
        public int F { get; set; } = 0;          // Fan status
        public int Z { get; set; } = 0;          // Heater status
        public int H { get; set; } = 1;          // Hard reset
        public double T { get; set; } = 250.5;   // Current temperature
        public int U { get; set; } = 0;          // Upper door status
        public int J { get; set; } = 0;          // Lower door status
        public double L { get; set; } = 25.5;    // Max temperature (fan trigger)
        public double S { get; set; } = 35.2;    // Min temperature (heater trigger)
    }
}
