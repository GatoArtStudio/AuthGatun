using System;
using AuthGatun.Core.Platform;
using AuthGatun.Helpers;
using Avalonia.Controls;

namespace AuthGatun.Services;

public class PlatformNativeWindowsService : IPlatformService
{
    public bool HiddenCurrentWindowByExcludeFromCapture(Window window)
    {
        var hwnd = window.TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;
        NativeWindowsHelper.SetWindowDisplayAffinity(hwnd, NativeWindowsHelper.WDA_EXCLUDEFROMCAPTURE);
        return true;
    }
}