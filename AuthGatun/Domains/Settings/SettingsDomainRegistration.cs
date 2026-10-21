using System.IO;
using AuthGatun.Domains.Settings.Adapters.Outbound.JsonFile;
using AuthGatun.Domains.Settings.Application.Ports.Inbound;
using AuthGatun.Domains.Settings.Application.Ports.Outbound;
using AuthGatun.Domains.Settings.Application.UseCases;
using Microsoft.Extensions.DependencyInjection;

namespace AuthGatun.Domains.Settings;

public static class SettingsDomainRegistration
{
    public static IServiceCollection AddSettingsDomain(this IServiceCollection services, string appFolderPathData)
    {
        var settingsPath = Path.Combine(appFolderPathData, "AuthGatun", "settings.json");

        services.AddSingleton<ISettingsRepository>(new JsonFileSettingsRepository(settingsPath));
        services.AddTransient<IGetSettingsUseCase, GetSettingsUseCase>();
        services.AddTransient<IUpdateSettingsUseCase, UpdateSettingsUseCase>();

        return services;
    }
}
