using System;
using System.Reactive;
using AuthGatun.Domains.IdentityAccess.Application.Ports.Inbound;
using AuthGatun.Domains.IdentityAccess.Application.Models;
using AuthGatun.Services;
using Avalonia.Controls.Notifications;
using ReactiveUI;

namespace AuthGatun.ViewModels;

public class KeyViewModel : ViewModelBase
{
    private readonly INewKeyUseCase _newKey;
    
    private readonly NotifyManagerService _notifyManagerService;
    private readonly UserStatusService _userStatusService;

    private string _serviceName = "";
    private string _totpKey = "";

    public string ServiceName
    {
        get => _serviceName;
        set => this.RaiseAndSetIfChanged(ref _serviceName, value);
    }

    public string TotpKey
    {
        get => _totpKey;
        set => this.RaiseAndSetIfChanged(ref _totpKey, value);
    }
    
    public ReactiveCommand<Unit, Unit> SaveKeyCommand { get; }

    public KeyViewModel(
        DiscordService discordService,
        INewKeyUseCase newKey,
        NotifyManagerService notifyManagerService,
        UserStatusService userStatusService
    )
    {
        _userStatusService = userStatusService;
        _notifyManagerService = notifyManagerService;
        _newKey = newKey;

        SaveKeyCommand = ReactiveCommand.Create(OnSaveKeyCommand);
        
        var user = _userStatusService.User;
        discordService.UpdatePresence(user?.UserName ?? "AuthGatun", "Agregemos una nueva clave TOTP!.");
    }

    private void OnSaveKeyCommand()
    {
        if (string.IsNullOrWhiteSpace(ServiceName))
        {
            _notifyManagerService.SendNotifyInWindow("Por favor, rellene todos los campos", title: "Gestor TOTP", NotificationType.Error);
            return;
        }

        if (string.IsNullOrWhiteSpace(TotpKey) || TotpKey.Contains(' '))
        {
            _notifyManagerService.SendNotifyInWindow(message: "La clave TOTP no debe contener espacios", title: "Gestor TOTP", NotificationType.Error);
            return;
        }

        var user = _userStatusService.User;
        if (user is null)
        {
            _notifyManagerService.SendNotifyInWindow("Aun no hay ningun usuario logeado", title: "Gestor TOTP", NotificationType.Error);
            return;
        }

        try
        {
            _newKey.Execute(new NewKeyRequest(user.Id, ServiceName, TotpKey));
            _notifyManagerService.SendNotifyInWindow("Clave guardada con éxito", title: "Gestor TOTP", NotificationType.Success);
        }
        catch (Exception e)
        {
            _notifyManagerService.SendNotifyInWindow($"Hubo un error, {e.Message}", title: "Gestor TOTP", NotificationType.Error);
        }
    }
}