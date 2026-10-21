using AuthGatun.Core.Platform;
using Avalonia.Controls;

namespace AuthGatun.Services;

public class PlatformFallbackService : IPlatformService
{
    public bool HiddenCurrentWindowByExcludeFromCapture(Window window) => false;
}
