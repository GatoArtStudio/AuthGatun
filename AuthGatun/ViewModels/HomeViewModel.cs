using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
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

public class HomeViewModel : ReactiveObject
{
    public ObservableCollection<UserKey> UserKeys { get; set; }
    
    private readonly IReadKeyUseCase _readKey;
    private readonly IReadKeysUseCase _readKeys;
    private readonly IDeleteKeyUseCase _deleteKey;
    private readonly TotpManager _totpManager;

    private readonly NotifyManager _notifyManager = NotifyManager.GetInstance();

    public ReactiveCommand<Guid, Unit> CopyToClipboardCodeCommand { get; }
    public ReactiveCommand<Guid, Unit> DeleteServiceKeyCommand { get; }

    public HomeViewModel()
    {
        var repository = RepositoryFactory.GetInstance().CreateRepository();
        var bus = BusFactory.GetInstance().CreateBus();
        _readKey = new ReadKeyUseCase(bus, repository);
        _readKeys = new ReadKeysUseCase(bus, repository);
        _deleteKey = new DeleteKeyUseCase(bus, repository);
        
        _totpManager = new TotpManager();

        CopyToClipboardCodeCommand = ReactiveCommand.CreateFromTask<Guid>(OnCopyToClipboardCodeCommand);
        DeleteServiceKeyCommand = ReactiveCommand.Create<Guid>(OnDeleteServiceKeyCommand, Observable.Return(true));

        UserKeys = GetUserKeys();
        UserStatus.GetInstance().RunRpcDiscord();
    }

    private ObservableCollection<UserKey> GetUserKeys()
    {
        ObservableCollection<UserKey> userKeys = new ObservableCollection<UserKey>();
        
        var user = UserStatus.GetInstance().User;
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
        var clipboard = ClipboardClass.Get();
        
        if (clipboard != null)
        {
            var key = _readKey.Execute(id);
            if (key == null)
            {
                _notifyManager.SendNotifyInWindow("Clave no encontrada", title: "Gestor de claves TOTP", NotificationType.Error);
                return;
            }
            
            string code = _totpManager.GetTotpCode(key.SecretKey);
        
            _notifyManager.SendNotifyInWindow($"Código copiado con éxito, código: {code}", title: "Gestor de claves TOTP");
            _ = Task.Run(() => clipboard.SetTextAsync(code));
        }
    }

    private void OnDeleteServiceKeyCommand(Guid id)
    {
        var key = _readKey.Execute(id);
        if (key == null)
        {
            _notifyManager.SendNotifyInWindow("Clave no encontrada", title: "Gestor de claves TOTP", NotificationType.Error);
            return;
        }
        
        _deleteKey.Execute(key);
        _notifyManager.SendNotifyInWindow("Clave eliminada con éxito", title: "Gestor de claves TOTP");

        UserKey? userKey = UserKeys
            .FirstOrDefault(u => u.Id == id);

        if (userKey != null)
            UserKeys.Remove(userKey);
    }
}