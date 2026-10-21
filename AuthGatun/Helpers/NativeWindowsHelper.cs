using System;
using System.Runtime.InteropServices;

namespace AuthGatun.Helpers;

public class NativeWindowsHelper
{
    [DllImport("user32.dll")]
    public static extern bool SetWindowDisplayAffinity(IntPtr hWnd, uint dwAffinity);

    public const uint WDA_EXCLUDEFROMCAPTURE = 0x11;
}