using Avalonia.Controls;

namespace AuthGatun.Core.Platform;

public interface IPlatformService
{
    bool SupportsScreenCaptureExclusion { get; }

    bool SetExcludedFromScreenCapture(Window window, bool excluded);
}
