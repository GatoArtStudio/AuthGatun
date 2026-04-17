using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using AuthGatun.Core.AvaloniaUI;
using AuthGatun.Domains.IdentityAccess.Application.Ports.In.UseCases;
using AuthGatun.Domains.IdentityAccess.Application.UseCases;
using AuthGatun.Domains.IdentityAccess.Domain.Model;
using AuthGatun.Domains.IdentityAccess.Infrastructure.Eventbus;
using AuthGatun.Domains.IdentityAccess.Infrastructure.Persistence;
using AuthGatun.Helpers;
using AuthGatun.Models;
using AuthGatun.Services;
using Avalonia.Controls.Notifications;
using ReactiveUI;

namespace AuthGatun.ViewModels;

public class HomeViewModel : ViewModelBase
{
    public ObservableCollection<UserKey> UserKeys { get; set; }
    
    private readonly IReadKeyUseCase _readKey;
    private readonly IReadKeysUseCase _readKeys;
    private readonly IDeleteKeyUseCase _deleteKey;
    
    private readonly TotpManagerService _totpManagerService;
    private readonly UserStatusService _userStatusService;
    private readonly NotifyManagerService _notifyManagerService;
    private readonly IWindowService _windowService;

    public ReactiveCommand<Guid, Unit> CopyToClipboardCodeCommand { get; }
    public ReactiveCommand<Guid, Unit> DeleteServiceKeyCommand { get; }

    public HomeViewModel(
        DiscordService discordService,
        RepositoryFactory repositoryFactory,
        BusFactory busFactory,
        NotifyManagerService notifyManagerService,
        UserStatusService userStatusService,
        IWindowService windowService
    )
    {
        _notifyManagerService = notifyManagerService;
        _userStatusService = userStatusService;
        _windowService = windowService;
        var repository = repositoryFactory.CreateRepository();
        var bus = busFactory.CreateBus();
        _readKey = new ReadKeyUseCase(bus, repository);
        _readKeys = new ReadKeysUseCase(bus, repository);
        _deleteKey = new DeleteKeyUseCase(bus, repository);
        
        _totpManagerService = new TotpManagerService();

        CopyToClipboardCodeCommand = ReactiveCommand.CreateFromTask<Guid>(OnCopyToClipboardCodeCommand);
        DeleteServiceKeyCommand = ReactiveCommand.Create<Guid>(OnDeleteServiceKeyCommand, Observable.Return(true));

        UserKeys = GetUserKeys();
        var user = _userStatusService.User;
        discordService.UpdatePresence(user?.Username.Value ?? "AuthGatun", "Observando las claves TOTP.");
    }

    private ObservableCollection<UserKey> GetUserKeys()
    {
        ObservableCollection<UserKey> userKeys = new ObservableCollection<UserKey>();
        
        var user = _userStatusService.User;
        if (user is null)
            return userKeys;

        foreach (Key key in _readKeys.Execute(user.Id))
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
            var key = _readKey.Execute(id);
            if (key == null)
            {
                _notifyManagerService.SendNotifyInWindow("Clave no encontrada", title: "Gestor de claves TOTP", NotificationType.Error);
                return;
            }
            
            string code = _totpManagerService.GetTotpCode(key.SecretKey);
        
            _notifyManagerService.SendNotifyInWindow($"Código copiado con éxito, código: {code}", title: "Gestor de claves TOTP");
            _ = Task.Run(() => clipboard.SetTextAsync(code));
        }
    }

    private void OnDeleteServiceKeyCommand(Guid id)
    {
        var key = _readKey.Execute(id);
        if (key == null)
        {
            _notifyManagerService.SendNotifyInWindow("Clave no encontrada", title: "Gestor de claves TOTP", NotificationType.Error);
            return;
        }
        
        _deleteKey.Execute(key);
        _notifyManagerService.SendNotifyInWindow("Clave eliminada con éxito", title: "Gestor de claves TOTP");

        UserKey? userKey = UserKeys
            .FirstOrDefault(u => u.Id == id);

        if (userKey != null)
            UserKeys.Remove(userKey);
    }
}