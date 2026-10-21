using AuthGatun.Domains.Settings.Application.Models;
using AuthGatun.Domains.Settings.Application.Ports.Inbound;
using AuthGatun.Domains.Settings.Application.Ports.Outbound;
using AuthGatun.Domains.Settings.Domain;

namespace AuthGatun.Domains.Settings.Application.UseCases;

public class UpdateSettingsUseCase(ISettingsRepository repository) : IUpdateSettingsUseCase
{
    public AppSettingsModel Execute(AppSettingsModel settings)
    {
        var validated = new AppSettings(settings.RunInBackground, settings.AutoLockMinutes);
        repository.Save(validated);
        return new AppSettingsModel(validated.RunInBackground, validated.AutoLockMinutes);
    }
}
