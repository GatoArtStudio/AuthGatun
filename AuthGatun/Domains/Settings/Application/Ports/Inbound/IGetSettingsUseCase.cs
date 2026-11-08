using AuthGatun.Domains.Settings.Application.Models;

namespace AuthGatun.Domains.Settings.Application.Ports.Inbound;

public interface IGetSettingsUseCase
{
    AppSettingsModel Execute();
}
