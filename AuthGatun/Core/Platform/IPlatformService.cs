using Avalonia.Controls;

namespace AuthGatun.Core.Platform;

public interface IPlatformService
{
    bool HiddenCurrentWindowByExcludeFromCapture(Window window);
}