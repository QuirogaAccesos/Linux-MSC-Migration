using System.Globalization;

namespace Parso.Utils.Objects
{
    internal class PicobResponseParso
    {
        private static PicobResponseParso _instance;
        public static PicobResponseParso Instance => _instance ??= new PicobResponseParso();
        private PicobResponseParso() { }

        //Custom datetime manager
        public DateTime customDateTime { get; set; } = DateTime.Now;

        public bool TrySetCustomTime(string timeString)
        {
            if (TimeSpan.TryParseExact(timeString, "hh\\:mm\\:ss",
                CultureInfo.InvariantCulture, out TimeSpan timeSpan))
            {
                // Combine with today's date at midnight
                customDateTime = DateTime.Today.Add(timeSpan);
                return true;
            }
            return false;
        }

        public string CustomTimeString => customDateTime.ToString("HH:mm:ss");

        // Deep Sleep mode
        public int A { get; set; } = 0;

        // LED Strip Controls
        public int R { get; set; } = 0;  // Red
        public int G { get; set; } = 0;  // Green
        public int B { get; set; } = 0;  // Blue

        // Peripheral Controls
        public int D { get; set; } = 0;  // Datafono
        public int M { get; set; } = 0;  // Modem
        public int I { get; set; } = 0;  // Impresora
        public int K { get; set; } = 0;  // PC
        public int O { get; set; } = 0;  // Otro

        // Sensors and Measurements
        public double T { get; set; } = 39.1;  // Temperature
        public int[] C { get; set; } = {1, 2799, 755 };  // External sensors [detection, analog1, analog2]
        public int W { get; set; } = 0;  // Position sensor

        // Door Status
        public int U { get; set; } = 1;  // Upper door (1: open, 0: closed)
        public int J { get; set; } = 0;  // Lower door (1: open, 0: closed)

        // Position Commands
        public int N { get; set; } = 2;  // Cerrado position status (1-4)

        // Power Management
        public double V { get; set; } = 12.4;  // Battery voltage
        public int E { get; set; } = 452;  // Touch input status
        public int H { get; set; } = 1;  // Deep Sleep timer reset

        // Time and Scheduling
        public string Q { get; set; } = DateTime.Now.ToString();  //Current time
        public string[] P { get; set; } = { "06:30:00", "19:00:00" };  // Schedule times
    }
}
