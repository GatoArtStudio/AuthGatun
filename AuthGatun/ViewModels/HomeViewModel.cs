using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using AuthGatun.Core.AvaloniaUI;
using AuthGatun.Domains.IdentityAccess.Application.Ports.Inbound;
using AuthGatun.Domains.IdentityAccess.Application.Models;
using AuthGatun.Helpers;
using AuthGatun.Models;
using AuthGatun.Services;
using Avalonia.Controls.Notifications;
using Avalonia.Input.Platform;
using Avalonia.Threading;
using ReactiveUI;

namespace AuthGatun.ViewModels;

public class HomeViewModel : ViewModelBase
{
    public ObservableCollection<UserKey> UserKeys { get; set; }
    
    private readonly IReadKeysUseCase _readKeys;
    private readonly IDeleteKeyUseCase _deleteKey;
    private readonly IGenerateTotpCodeUseCase _generateTotpCode;
    
    private readonly UserStatusService _userStatusService;
    private readonly NotifyManagerService _notifyManagerService;
    private readonly IWindowService _windowService;

    private static readonly TimeSpan ClipboardClearDelay = TimeSpan.FromSeconds(30);

    public ReactiveCommand<Guid, Unit> CopyToClipboardCodeCommand { get; }
    public ReactiveCommand<Guid, Unit> DeleteServiceKeyCommand { get; }

    public HomeViewModel(
        DiscordService discordService,
        IReadKeysUseCase readKeys,
        IDeleteKeyUseCase deleteKey,
        IGenerateTotpCodeUseCase generateTotpCode,
        NotifyManagerService notifyManagerService,
        UserStatusService userStatusService,
        IWindowService windowService
    )
    {
        _notifyManagerService = notifyManagerService;
        _userStatusService = userStatusService;
        _windowService = windowService;
        _readKeys = readKeys;
        _deleteKey = deleteKey;
        _generateTotpCode = generateTotpCode;

        CopyToClipboardCodeCommand = ReactiveCommand.CreateFromTask<Guid>(OnCopyToClipboardCodeCommand);
        DeleteServiceKeyCommand = ReactiveCommand.Create<Guid>(OnDeleteServiceKeyCommand, Observable.Return(true));

        UserKeys = GetUserKeys();
        var user = _userStatusService.User;
        discordService.UpdatePresence(user?.UserName ?? "AuthGatun", "Observando las claves TOTP.");
    }

    private ObservableCollection<UserKey> GetUserKeys()
    {
        ObservableCollection<UserKey> userKeys = new ObservableCollection<UserKey>();
        
        var user = _userStatusService.User;
        if (user is null)
            return userKeys;

        foreach (KeySummary key in _readKeys.Execute(user.Id))
        {
            userKeys.Add(
                new UserKey(
                    key.Id,
                    key.ServiceName,
                    DeleteServiceKeyCommand
                )
            );
        }
        
        return userKeys;
    }

    private async Task OnCopyToClipboardCodeCommand(Guid id)
    {
        var clipboard = _windowService.GetClipboard();
        
        if (clipboard != null)
        {
            string? code = _generateTotpCode.Execute(id);
            if (code == null)
            {
                _notifyManagerService.SendNotifyInWindow("Clave no encontrada", title: "Gestor de claves TOTP", NotificationType.Error);
                return;
            }
        
            _notifyManagerService.SendNotifyInWindow($"Código copiado con éxito, código: {code}", title: "Gestor de claves TOTP");
            _ = Task.Run(() => clipboard.SetTextAsync(code));
            _ = ClearClipboardLaterAsync(clipboard, code);
        }
    }

    private static async Task ClearClipboardLaterAsync(IClipboard clipboard, string code)
    {
        await Task.Delay(ClipboardClearDelay);

        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            if (await clipboard.GetTextAsync() == code)
                await clipboard.ClearAsync();
        });
    }

    private void OnDeleteServiceKeyCommand(Guid id)
    {
        if (!_deleteKey.Execute(id))
        {
            _notifyManagerService.SendNotifyInWindow("Clave no encontrada", title: "Gestor de claves TOTP", NotificationType.Error);
            return;
        }
        
        _notifyManagerService.SendNotifyInWindow("Clave eliminada con éxito", title: "Gestor de claves TOTP");

        UserKey? userKey = UserKeys
            .FirstOrDefault(u => u.Id == id);

        if (userKey != null)
            UserKeys.Remove(userKey);
    }
}