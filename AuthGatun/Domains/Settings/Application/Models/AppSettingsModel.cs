namespace AuthGatun.Domains.Settings.Application.Models;

public record AppSettingsModel(
    bool RunInBackground,
    int AutoLockMinutes,
    bool HideFromScreenCapture,
    string Language,
    bool PrivacyMode = false,
    string Theme = "dark");
