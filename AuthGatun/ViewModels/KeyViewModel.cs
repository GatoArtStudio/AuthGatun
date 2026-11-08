using System;
using System.IO;
using System.Reactive;
using System.Threading.Tasks;
using AuthGatun.Core.AvaloniaUI;
using AuthGatun.Core.Localization;
using AuthGatun.Domains.IdentityAccess.Application;
using AuthGatun.Domains.IdentityAccess.Application.Ports.Inbound;
using AuthGatun.Domains.IdentityAccess.Application.Models;
using AuthGatun.Services;
using Avalonia.Controls.Notifications;
using Avalonia.Platform.Storage;
using ReactiveUI;

namespace AuthGatun.ViewModels;

public class KeyViewModel : ViewModelBase
{
    private readonly INewKeyUseCase _newKey;
    
    private readonly NotifyManagerService _notifyManagerService;
    private readonly UserStatusService _userStatusService;
    private readonly ILocalizer _localizer;
    private readonly IScanKeyFromQrUseCase _scanKeyFromQr;
    private readonly IWindowService _windowService;

    private const long MaxImageBytes = 10 * 1024 * 1024;
    private static readonly FilePickerFileType ImageFileType = new("Images") { Patterns = ["*.png", "*.jpg", "*.jpeg", "*.bmp", "*.webp", "*.gif"] };

    private string _serviceName = "";
    private string _totpKey = "";
    private TotpParametersModel? _scannedParameters;
    private string _scannedSecret = "";

    public string ServiceName
    {
        get => _serviceName;
        set => this.RaiseAndSetIfChanged(ref _serviceName, value);
    }

    public string TotpKey
    {
        get => _totpKey;
        set
        {
            this.RaiseAndSetIfChanged(ref _totpKey, value);
            if (_scannedParameters is not null && value != _scannedSecret)
                ClearScannedParameters();
        }
    }

    public string ParametersHint =>
        _scannedParameters is null || _scannedParameters.IsDefault
            ? ""
            : _localizer.Get("Parameters_Hint", _scannedParameters.Algorithm, _scannedParameters.Digits, _scannedParameters.PeriodSeconds);
    
    public ReactiveCommand<Unit, Unit> SaveKeyCommand { get; }
    public ReactiveCommand<Unit, Unit> ScanFromClipboardCommand { get; }
    public ReactiveCommand<Unit, Unit> ScanFromFileCommand { get; }

    public KeyViewModel(
        DiscordService discordService,
        INewKeyUseCase newKey,
        NotifyManagerService notifyManagerService,
        UserStatusService userStatusService,
        ILocalizer localizer,
        IScanKeyFromQrUseCase scanKeyFromQr,
        IWindowService windowService
    )
    {
        _scanKeyFromQr = scanKeyFromQr;
        _windowService = windowService;
        _userStatusService = userStatusService;
        _localizer = localizer;
        _notifyManagerService = notifyManagerService;
        _newKey = newKey;

        SaveKeyCommand = ReactiveCommand.Create(OnSaveKeyCommand);
        ScanFromClipboardCommand = ReactiveCommand.CreateFromTask(OnScanFromClipboard);
        ScanFromFileCommand = ReactiveCommand.CreateFromTask(OnScanFromFile);
        
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
            _newKey.Execute(new NewKeyRequest(user.Id, ServiceName, TotpKey, _scannedParameters));
            _notifyManagerService.SendNotifyInWindow(_localizer.Get("Keys_Saved"), title: _localizer.Get("Keys_NotifyTitle"), NotificationType.Success);
            ServiceName = "";
            TotpKey = "";
            ClearScannedParameters();
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

    public async Task ScanDroppedFileAsync(IStorageFile file)
    {
        await RunScanAsync(async () => _scanKeyFromQr.ScanImageForKeys(await ReadFileAsync(file)));
    }

    private async Task OnScanFromClipboard()
    {
        var clipboard = _windowService.GetClipboard();
        if (clipboard is null)
        {
            Notify("Keys_NoClipboardContent", NotificationType.Error);
            return;
        }

        ClipboardContent content;
        try
        {
            content = await ClipboardContentReader.ReadAsync(clipboard);
        }
        catch (Exception)
        {
            Notify("Keys_NoClipboardContent", NotificationType.Error);
            return;
        }

        if (content.Image is not null)
        {
            await RunScanAsync(() => Task.FromResult(_scanKeyFromQr.ScanImageForKeys(content.Image)));
            return;
        }

        if (!string.IsNullOrWhiteSpace(content.Text))
        {
            await RunScanAsync(() => Task.FromResult(_scanKeyFromQr.ParseLinkForKeys(content.Text)));
            return;
        }

        Notify("Keys_NoClipboardContent", NotificationType.Error);
    }

    private async Task OnScanFromFile()
    {
        var storage = _windowService.GetStorageProvider();
        if (storage is null)
        {
            Notify("Settings_NoFilePicker", NotificationType.Error);
            return;
        }

        var files = await storage.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = _localizer.Get("Keys_PickerTitle"),
            AllowMultiple = false,
            FileTypeFilter = [ImageFileType]
        });
        if (files.Count == 0)
            return;

        await ScanDroppedFileAsync(files[0]);
    }

    private async Task RunScanAsync(Func<Task<ScanResultModel>> scan)
    {
        try
        {
            var result = await scan();
            if (result.Keys.Count == 1)
            {
                var key = result.Keys[0];
                ServiceName = key.ServiceName;
                TotpKey = key.SecretKey;
                _scannedSecret = key.SecretKey;
                _scannedParameters = key.Parameters;
                this.RaisePropertyChanged(nameof(ParametersHint));
                Notify("Keys_ScanSuccess", NotificationType.Success);
                return;
            }

            ImportScannedKeys(result);
        }
        catch (IdentityAccessException e)
        {
            Notify("Error_" + e.Error, NotificationType.Error);
        }
        catch (FileTooLargeException)
        {
            Notify("Keys_FileTooLarge", NotificationType.Error);
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            Notify("Error_ImageUnreadable", NotificationType.Error);
        }
        catch (Exception)
        {
            Notify("Error_Unexpected", NotificationType.Error);
        }
    }

    private void ClearScannedParameters()
    {
        _scannedParameters = null;
        _scannedSecret = "";
        this.RaisePropertyChanged(nameof(ParametersHint));
    }

    private void ImportScannedKeys(ScanResultModel result)
    {
        var user = _userStatusService.User;
        if (user is null)
        {
            Notify("Keys_NoUser", NotificationType.Error);
            return;
        }

        var imported = 0;
        var skipped = result.Unsupported;
        foreach (var key in result.Keys)
        {
            try
            {
                _newKey.Execute(new NewKeyRequest(user.Id, key.ServiceName, key.SecretKey, key.Parameters, RejectDuplicate: true));
                imported++;
            }
            catch (IdentityAccessException e) when (e.Error == IdentityAccessError.DuplicateKey)
            {
                skipped++;
            }
        }

        var messageKey = imported > 0 ? "Parameters_MigrationResult" : "Parameters_MigrationNothing";
        var message = imported > 0 ? _localizer.Get(messageKey, imported, skipped) : _localizer.Get(messageKey, skipped);
        _notifyManagerService.SendNotifyInWindow(message, title: _localizer.Get("Keys_NotifyTitle"), imported > 0 ? NotificationType.Success : NotificationType.Warning);
    }

    private static async Task<byte[]> ReadFileAsync(IStorageFile file)
    {
        await using var stream = await file.OpenReadAsync();
        if (stream.CanSeek && stream.Length > MaxImageBytes)
            throw new FileTooLargeException();

        using var memory = new MemoryStream();
        await stream.CopyToAsync(memory);
        if (memory.Length > MaxImageBytes)
            throw new FileTooLargeException();

        return memory.ToArray();
    }

    private void Notify(string messageKey, NotificationType type) =>
        _notifyManagerService.SendNotifyInWindow(_localizer.Get(messageKey), title: _localizer.Get("Keys_NotifyTitle"), type);

    private sealed class FileTooLargeException : Exception;
}
