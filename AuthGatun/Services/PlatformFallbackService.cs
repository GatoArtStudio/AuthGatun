using AuthGatun.Core.Platform;
using Avalonia.Controls;

namespace AuthGatun.Services;

public class PlatformFallbackService : IPlatformService
{
    public bool SupportsScreenCaptureExclusion => false;

    public bool SetExcludedFromScreenCapture(Window window, bool excluded) => false;
}
