using System;
using AuthGatun.Domains.Settings.Application.Models;
using AuthGatun.Domains.Settings.Application.Ports.Inbound;

namespace AuthGatun.Services;

public class AppSettingsState(IGetSettingsUseCase getSettings, IUpdateSettingsUseCase updateSettings)
{
    public AppSettingsModel Current { get; private set; } = getSettings.Execute();

    public event Action? Changed;

    public void Update(AppSettingsModel settings)
    {
        Current = updateSettings.Execute(settings);
        Changed?.Invoke();
    }
}
