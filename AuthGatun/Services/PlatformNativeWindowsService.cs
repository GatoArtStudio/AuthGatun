using System;
using AuthGatun.Core.Platform;
using AuthGatun.Helpers;
using Avalonia.Controls;

namespace AuthGatun.Services;

public class PlatformNativeWindowsService : IPlatformService
{
    public bool SupportsScreenCaptureExclusion => true;

    public bool SetExcludedFromScreenCapture(Window window, bool excluded)
    {
        var hwnd = window.TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;
        if (hwnd == IntPtr.Zero)
            return false;

        return NativeWindowsHelper.SetWindowDisplayAffinity(
            hwnd,
            excluded ? NativeWindowsHelper.WDA_EXCLUDEFROMCAPTURE : NativeWindowsHelper.WDA_NONE);
    }
}
