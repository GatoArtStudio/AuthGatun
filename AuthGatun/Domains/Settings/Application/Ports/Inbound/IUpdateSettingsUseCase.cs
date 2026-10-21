using AuthGatun.Domains.Settings.Application.Models;

namespace AuthGatun.Domains.Settings.Application.Ports.Inbound;

public interface IUpdateSettingsUseCase
{
    AppSettingsModel Execute(AppSettingsModel settings);
}
