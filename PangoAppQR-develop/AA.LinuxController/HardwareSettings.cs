using System;
using System.Configuration;

namespace AA.LinuxController
{
    public static class HardwareSettings
    {
        public static readonly string HardwareMode = ConfigurationManager.AppSettings["HardwareMode"] ?? "Direct";

        public static readonly bool UseLinuxController = HardwareMode.Equals("LinuxController", StringComparison.OrdinalIgnoreCase);
    }
}