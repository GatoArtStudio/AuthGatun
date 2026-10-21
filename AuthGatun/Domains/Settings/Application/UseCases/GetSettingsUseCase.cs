using AuthGatun.Domains.Settings.Application.Models;
using AuthGatun.Domains.Settings.Application.Ports.Inbound;
using AuthGatun.Domains.Settings.Application.Ports.Outbound;
using AuthGatun.Domains.Settings.Domain;

namespace AuthGatun.Domains.Settings.Application.UseCases;

public class GetSettingsUseCase(ISettingsRepository repository) : IGetSettingsUseCase
{
    public AppSettingsModel Execute()
    {
        var settings = repository.Load() ?? AppSettings.Default;
        return new AppSettingsModel(settings.RunInBackground, settings.AutoLockMinutes);
    }
}
