using Avalonia;
using Avalonia.Styling;

namespace AuthGatun.Services;

public class ThemeService
{
    private readonly AppSettingsState _settings;

    public ThemeService(AppSettingsState settings)
    {
        _settings = settings;
        _settings.Changed += Apply;
    }

    public static ThemeVariant ResolveVariant(string? themeCode) => themeCode switch
    {
        "light" => ThemeVariant.Light,
        "system" => ThemeVariant.Default,
        _ => ThemeVariant.Dark
    };

    public void Apply()
    {
        if (Application.Current is { } application)
            application.RequestedThemeVariant = ResolveVariant(_settings.Current.Theme);
    }
}
