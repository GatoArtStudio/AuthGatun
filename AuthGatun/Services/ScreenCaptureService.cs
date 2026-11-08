using AuthGatun.Core.AvaloniaUI;
using AuthGatun.Core.Platform;
using Avalonia.Controls;

namespace AuthGatun.Services;

public class ScreenCaptureService
{
    private readonly IPlatformService _platform;
    private readonly IWindowService _windows;
    private readonly AppSettingsState _settings;

    public ScreenCaptureService(IPlatformService platform, IWindowService windows, AppSettingsState settings)
    {
        _platform = platform;
        _windows = windows;
        _settings = settings;
        _settings.Changed += ApplyToOpenWindows;
    }

    public bool IsSupported => _platform.SupportsScreenCaptureExclusion;

    public void Apply(Window window)
    {
        if (_platform.SupportsScreenCaptureExclusion)
            _platform.SetExcludedFromScreenCapture(window, _settings.Current.HideFromScreenCapture);
    }

    private void ApplyToOpenWindows()
    {
        foreach (var window in _windows.OpenWindows())
            Apply(window);
    }
}
