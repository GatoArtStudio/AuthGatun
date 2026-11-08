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
using AuthGatun.Services;
using Avalonia.Controls.Notifications;
using Avalonia.Input.Platform;
using Avalonia.Threading;
using ReactiveUI;

namespace AuthGatun.ViewModels;

public class HomeViewModel : ViewModelBase
{
    public ObservableCollection<TotpCardViewModel> Cards { get; } = new();

    private readonly IReadTotpCodesUseCase _readTotpCodes;
    private readonly DispatcherTimer _refreshTimer = new() { Interval = TimeSpan.FromMilliseconds(500) };
    private readonly IDeleteKeyUseCase _deleteKey;
    private readonly IGenerateTotpCodeUseCase _generateTotpCode;
    
    private readonly UserStatusService _userStatusService;
    private readonly NotifyManagerService _notifyManagerService;
    private readonly IWindowService _windowService;

    private static readonly TimeSpan ClipboardClearDelay = TimeSpan.FromSeconds(30);

    public bool HasCards => Cards.Count > 0;

    public HomeViewModel(
        DiscordService discordService,
        IReadTotpCodesUseCase readTotpCodes,
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
        _readTotpCodes = readTotpCodes;
        _deleteKey = deleteKey;
        _generateTotpCode = generateTotpCode;

        _refreshTimer.Tick += (_, _) => Refresh();
        Cards.CollectionChanged += (_, _) => this.RaisePropertyChanged(nameof(HasCards));
        Refresh();

        var user = _userStatusService.User;
        discordService.UpdatePresence(user?.UserName ?? "AuthGatun", "Observando las claves TOTP.");
    }

    public void StartRefreshing()
    {
        Refresh();
        _refreshTimer.Start();
    }

    public void StopRefreshing() => _refreshTimer.Stop();

    private void Refresh()
    {
        var user = _userStatusService.User;
        if (user is null)
            return;

        var codes = _readTotpCodes.Execute(user.Id);

        foreach (var model in codes)
        {
            var card = Cards.FirstOrDefault(c => c.Id == model.Id);
            if (card is null)
                Cards.Add(new TotpCardViewModel(model, id => _ = OnCopyToClipboardCodeCommand(id), OnDeleteServiceKeyCommand));
            else
                card.Update(model);
        }

        foreach (var stale in Cards.Where(c => codes.All(m => m.Id != c.Id)).ToList())
            Cards.Remove(stale);
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
        
            _notifyManagerService.SendNotifyInWindow("Código copiado. Se borrará del portapapeles en 30 segundos.", title: "Gestor de claves TOTP", NotificationType.Success);
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

        Refresh();
    }
}