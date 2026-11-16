using System;
using System.Diagnostics;

namespace AA.PangoApp.Payments.Interface.Helpers
{
    public static class ProcessLauncher
    {
        public static Process Start(string command)
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                return Process.Start(command);

            return Process.Start(new ProcessStartInfo
            {
                FileName = "/bin/sh",
                Arguments = "-c \"" + command.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"",
                UseShellExecute = false
            });
        }
    }
}