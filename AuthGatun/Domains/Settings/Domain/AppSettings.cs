using System;
using System.Collections.Generic;
using System.Linq;

namespace AuthGatun.Domains.Settings.Domain;

public sealed record AppSettings
{
    public static readonly IReadOnlyList<int> AllowedAutoLockMinutes = [0, 1, 5, 15, 30];

    public const string DefaultLanguage = "en";

    public static readonly IReadOnlyList<string> SupportedLanguages = ["en", "es"];

    public const string DefaultTheme = "dark";

    public static readonly IReadOnlyList<string> SupportedThemes = ["dark", "light", "system", "catppuccin-mocha"];

    public static AppSettings Default => new(false, 5, true, null);

    public static bool IsSupportedLanguage(string? language) =>
        language is not null && SupportedLanguages.Contains(language);

    public bool RunInBackground { get; }
    public int AutoLockMinutes { get; }
    public bool HideFromScreenCapture { get; }

    public bool PrivacyMode { get; }
    public string Theme { get; }

    // Null means the user never chose a language and it still has to be detected.
    public string? Language { get; }

    public AppSettings(bool runInBackground, int autoLockMinutes, bool hideFromScreenCapture,
        string? language,
        bool privacyMode = false,
        string theme = DefaultTheme)
    {
        if (!AllowedAutoLockMinutes.Contains(autoLockMinutes))
            throw new ArgumentException("The auto lock time is not one of the allowed values.", nameof(autoLockMinutes));

        if (language is not null && !IsSupportedLanguage(language))
            throw new ArgumentException("The language is not supported.", nameof(language));

        if (!SupportedThemes.Contains(theme))
            throw new ArgumentException("The theme is not supported.", nameof(theme));

        RunInBackground = runInBackground;
        PrivacyMode = privacyMode;
        Theme = theme;
        Language = language;
        AutoLockMinutes = autoLockMinutes;
        HideFromScreenCapture = hideFromScreenCapture;
    }
}
