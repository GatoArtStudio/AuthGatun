using AuthGatun.Domains.Settings.Application.Models;
using AuthGatun.Domains.Settings.Application.Ports.Inbound;
using AuthGatun.Domains.Settings.Application.Ports.Outbound;
using AuthGatun.Domains.Settings.Domain;

namespace AuthGatun.Domains.Settings.Application.UseCases;

public class GetSettingsUseCase(ISettingsRepository repository, ISystemLanguageProvider systemLanguage)
    : IGetSettingsUseCase
{
    public AppSettingsModel Execute()
    {
        var settings = repository.Load() ?? AppSettings.Default;

        if (settings.Language is null)
        {
            var detected = systemLanguage.GetSystemLanguage();
            var language = AppSettings.IsSupportedLanguage(detected) ? detected! : AppSettings.DefaultLanguage;

            settings = new AppSettings(
                settings.RunInBackground,
                settings.AutoLockMinutes,
                settings.HideFromScreenCapture,
                language,
                settings.PrivacyMode,
                settings.Theme);
            repository.Save(settings);
        }

        return new AppSettingsModel(
            settings.RunInBackground,
            settings.AutoLockMinutes,
            settings.HideFromScreenCapture,
            settings.Language!,
            settings.PrivacyMode,
            settings.Theme);
    }
}
