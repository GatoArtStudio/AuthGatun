using System;
using System.Reactive;
using AuthGatun.Core.Localization;
using AuthGatun.Domains.IdentityAccess.Application;
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
    private readonly ILocalizer _localizer;

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
        UserStatusService userStatusService,
        ILocalizer localizer
    )
    {
        _userStatusService = userStatusService;
        _localizer = localizer;
        _notifyManagerService = notifyManagerService;
        _newKey = newKey;

        SaveKeyCommand = ReactiveCommand.Create(OnSaveKeyCommand);
        
        var user = _userStatusService.User;
        discordService.UpdatePresence(user?.UserName ?? "AuthGatun", _localizer.Get("Presence_Keys"));
    }

    private void OnSaveKeyCommand()
    {
        if (string.IsNullOrWhiteSpace(ServiceName))
        {
            _notifyManagerService.SendNotifyInWindow(_localizer.Get("Keys_FillAll"), title: _localizer.Get("Keys_NotifyTitle"), NotificationType.Error);
            return;
        }

        if (string.IsNullOrWhiteSpace(TotpKey) || TotpKey.Contains(' '))
        {
            _notifyManagerService.SendNotifyInWindow(message: _localizer.Get("Keys_NoSpaces"), title: _localizer.Get("Keys_NotifyTitle"), NotificationType.Error);
            return;
        }

        var user = _userStatusService.User;
        if (user is null)
        {
            _notifyManagerService.SendNotifyInWindow(_localizer.Get("Keys_NoUser"), title: _localizer.Get("Keys_NotifyTitle"), NotificationType.Error);
            return;
        }

        try
        {
            _newKey.Execute(new NewKeyRequest(user.Id, ServiceName, TotpKey));
            _notifyManagerService.SendNotifyInWindow(_localizer.Get("Keys_Saved"), title: _localizer.Get("Keys_NotifyTitle"), NotificationType.Success);
        }
        catch (IdentityAccessException e)
        {
            _notifyManagerService.SendNotifyInWindow(_localizer.Get("Error_" + e.Error), title: _localizer.Get("Keys_NotifyTitle"), NotificationType.Error);
        }
        catch (Exception)
        {
            _notifyManagerService.SendNotifyInWindow(_localizer.Get("Error_Unexpected"), title: _localizer.Get("Keys_NotifyTitle"), NotificationType.Error);
        }
    }
}