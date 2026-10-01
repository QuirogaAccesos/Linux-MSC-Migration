using System;
using System.Runtime.InteropServices;

namespace AA.Pango.TestWinApp
{
    public class OnScreenKeyboard
    {
        [DllImport("user32.dll", SetLastError = false)]
        public static extern IntPtr GetDesktopWindow();
    }

    [ComImport, Guid("4ce576fa-83dc-4F88-951c-9d0782b4e376")]
    class UIHostNoLaunch
    {
    }

    [ComImport, Guid("37c994e7-432b-4834-a2f7-dce1f13b834b")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface ITipInvocation
    {
        void Toggle(IntPtr hwnd);
    }


}
