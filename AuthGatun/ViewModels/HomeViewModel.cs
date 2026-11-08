using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using AuthGatun.Core.AvaloniaUI;
using AuthGatun.Core.Localization;
using System.Collections.Generic;
using AuthGatun.Domains.IdentityAccess.Application;
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
    private readonly IUpdateKeyUseCase _updateKey;
    private readonly IGenerateTotpCodeUseCase _generateTotpCode;
    
    private readonly UserStatusService _userStatusService;
    private readonly NotifyManagerService _notifyManagerService;
    private readonly IWindowService _windowService;
    private readonly ILocalizer _localizer;
    private readonly AppSettingsState _settingsState;

    private static readonly TimeSpan ClipboardClearDelay = TimeSpan.FromSeconds(30);

    private readonly List<TotpCardViewModel> _allCards = [];
    private readonly ISetKeyFavoriteUseCase _setKeyFavorite;
    private readonly IMoveKeyUseCase _moveKey;
    private string _searchText = "";
    private string _normalizedQuery = "";

    public bool HasCards => Cards.Count > 0;
    public bool HasKeys => _allCards.Count > 0;
    public bool HasNoResults => HasKeys && Cards.Count == 0;

    public string SearchText
    {
        get => _searchText;
        set
        {
            this.RaiseAndSetIfChanged(ref _searchText, value);
            _normalizedQuery = SearchTextNormalizer.Normalize(value);
            ApplyFilter();
        }
    }

    public HomeViewModel(
        DiscordService discordService,
        IReadTotpCodesUseCase readTotpCodes,
        IDeleteKeyUseCase deleteKey,
        IUpdateKeyUseCase updateKey,
        IGenerateTotpCodeUseCase generateTotpCode,
        ISetKeyFavoriteUseCase setKeyFavorite,
        IMoveKeyUseCase moveKey,
        NotifyManagerService notifyManagerService,
        UserStatusService userStatusService,
        IWindowService windowService,
        ILocalizer localizer,
        AppSettingsState settingsState
    )
    {
        _notifyManagerService = notifyManagerService;
        _userStatusService = userStatusService;
        _windowService = windowService;
        _localizer = localizer;
        _settingsState = settingsState;
        _readTotpCodes = readTotpCodes;
        _deleteKey = deleteKey;
        _updateKey = updateKey;
        _generateTotpCode = generateTotpCode;
        _setKeyFavorite = setKeyFavorite;
        _moveKey = moveKey;

        _refreshTimer.Tick += (_, _) => Refresh();
        Cards.CollectionChanged += (_, _) =>
        {
            this.RaisePropertyChanged(nameof(HasCards));
            this.RaisePropertyChanged(nameof(HasNoResults));
        };
        Refresh();

        var user = _userStatusService.User;
        discordService.UpdatePresence(user?.UserName ?? "AuthGatun", _localizer.Get("Presence_Home"));
    }

    public void StartRefreshing()
    {
        _settingsState.Changed -= ApplyPrivacyMode;
        _settingsState.Changed += ApplyPrivacyMode;
        ApplyPrivacyMode();
        Refresh();
        _refreshTimer.Start();
    }

    public void StopRefreshing()
    {
        _refreshTimer.Stop();
        _settingsState.Changed -= ApplyPrivacyMode;
    }

    private void ApplyPrivacyMode()
    {
        foreach (var card in _allCards)
            if (card.PrivacyEnabled != _settingsState.Current.PrivacyMode)
                card.SetPrivacyEnabled(_settingsState.Current.PrivacyMode);
    }

    private void Refresh()
    {
        var user = _userStatusService.User;
        if (user is null)
            return;

        var codes = _readTotpCodes.Execute(user.Id);

        var hadKeys = HasKeys;
        var ordered = new List<TotpCardViewModel>(codes.Count);
        foreach (var model in codes)
        {
            var card = _allCards.FirstOrDefault(c => c.Id == model.Id);
            if (card is null)
                card = new TotpCardViewModel(model, id => _ = OnCopyToClipboardCodeCommand(id), OnDeleteServiceKeyCommand, OnSetFavorite, OnMoveKey, SystemClock.Instance, _settingsState.Current.PrivacyMode, OnUpdateServiceKey);
            else
                card.Update(model);
            ordered.Add(card);
        }

        _allCards.Clear();
        _allCards.AddRange(ordered);
        if (hadKeys != HasKeys)
            this.RaisePropertyChanged(nameof(HasKeys));

        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var searching = _normalizedQuery.Length > 0;
        var visible = _allCards.Where(c => SearchTextNormalizer.Matches(c.ServiceName, _normalizedQuery)).ToList();

        for (var i = 0; i < _allCards.Count; i++)
        {
            var card = _allCards[i];
            var canMoveUp = !searching && i > 0 && _allCards[i - 1].IsFavorite == card.IsFavorite;
            var canMoveDown = !searching && i < _allCards.Count - 1 && _allCards[i + 1].IsFavorite == card.IsFavorite;
            card.SetMovability(canMoveUp, canMoveDown);
        }

        for (var i = Cards.Count - 1; i >= 0; i--)
        {
            if (!visible.Contains(Cards[i]))
                Cards.RemoveAt(i);
        }

        for (var i = 0; i < visible.Count; i++)
        {
            if (i < Cards.Count && ReferenceEquals(Cards[i], visible[i]))
                continue;

            var current = Cards.IndexOf(visible[i]);
            if (current < 0)
                Cards.Insert(i, visible[i]);
            else
                Cards.Move(current, i);
        }

        this.RaisePropertyChanged(nameof(HasNoResults));
    }

    private void OnSetFavorite(Guid id, bool isFavorite)
    {
        RunKeyAction(() =>
        {
            _setKeyFavorite.Execute(id, isFavorite);
            return true;
        });
    }

    private void OnMoveKey(Guid id, KeyMoveDirection direction)
    {
        RunKeyAction(() => _moveKey.Execute(id, direction));
    }

    private void RunKeyAction(Func<bool> action)
    {
        try
        {
            if (action())
                Refresh();
        }
        catch (IdentityAccessException e)
        {
            _notifyManagerService.SendNotifyInWindow(_localizer.Get("Error_" + e.Error), title: _localizer.Get("Home_NotifyTitle"), NotificationType.Error);
            Refresh();
        }
        catch (Exception)
        {
            _notifyManagerService.SendNotifyInWindow(_localizer.Get("Error_Unexpected"), title: _localizer.Get("Home_NotifyTitle"), NotificationType.Error);
        }
    }

    private async Task OnCopyToClipboardCodeCommand(Guid id)
    {
        var clipboard = _windowService.GetClipboard();
        
        if (clipboard != null)
        {
            string? code = _generateTotpCode.Execute(id);
            if (code == null)
            {
                _notifyManagerService.SendNotifyInWindow(_localizer.Get("Home_KeyNotFound"), title: _localizer.Get("Home_NotifyTitle"), NotificationType.Error);
                return;
            }
        
            _notifyManagerService.SendNotifyInWindow(_localizer.Get("Home_Copied"), title: _localizer.Get("Home_NotifyTitle"), NotificationType.Success);
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
            _notifyManagerService.SendNotifyInWindow(_localizer.Get("Home_KeyNotFound"), title: _localizer.Get("Home_NotifyTitle"), NotificationType.Error);
            return;
        }
        
        _notifyManagerService.SendNotifyInWindow(_localizer.Get("Home_Deleted"), title: _localizer.Get("Home_NotifyTitle"));

        Refresh();
    }

    private bool OnUpdateServiceKey(Guid id, string serviceName, string? secretKey)
    {
        var title = _localizer.Get("Edit_NotifyTitle");
        var user = _userStatusService.User;
        if (user is null)
            return false;

        if (string.IsNullOrWhiteSpace(serviceName))
        {
            _notifyManagerService.SendNotifyInWindow(_localizer.Get("Edit_NameRequired"), title: title, NotificationType.Error);
            return false;
        }

        try
        {
            _updateKey.Execute(new UpdateKeyRequest(user.Id, id, serviceName.Trim(), secretKey));
            _notifyManagerService.SendNotifyInWindow(_localizer.Get("Edit_Saved"), title: title, NotificationType.Success);
            Refresh();
            return true;
        }
        catch (IdentityAccessException e)
        {
            _notifyManagerService.SendNotifyInWindow(_localizer.Get("Error_" + e.Error), title: title, NotificationType.Error);
        }
        catch (Exception)
        {
            _notifyManagerService.SendNotifyInWindow(_localizer.Get("Error_Unexpected"), title: title, NotificationType.Error);
        }

        return false;
    }
}
