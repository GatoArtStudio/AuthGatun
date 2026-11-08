using Avalonia;
using Avalonia.Styling;

namespace AuthGatun.Services;

public static class AppThemes
{
    public const string CatppuccinMochaCode = "catppuccin-mocha";

    // Resources keyed "CatppuccinMocha" in AppStyles.axaml; any resource it does not define falls back to Dark.
    public static readonly ThemeVariant CatppuccinMocha = new("CatppuccinMocha", ThemeVariant.Dark);
}

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
        AppThemes.CatppuccinMochaCode => AppThemes.CatppuccinMocha,
        _ => ThemeVariant.Dark
    };

    public void Apply()
    {
        if (Application.Current is { } application)
            application.RequestedThemeVariant = ResolveVariant(_settings.Current.Theme);
    }
}
