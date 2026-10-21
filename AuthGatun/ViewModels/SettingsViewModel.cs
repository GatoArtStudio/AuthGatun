using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using AuthGatun.Core.AvaloniaUI;
using AuthGatun.Domains.IdentityAccess.Application.Ports.Inbound;
using AuthGatun.Domains.Settings.Domain;
using AuthGatun.Services;
using Avalonia.Controls.Notifications;
using Avalonia.Platform.Storage;
using ReactiveUI;

namespace AuthGatun.ViewModels;

public record AutoLockOption(int Minutes)
{
    public override string ToString() => Minutes == 0 ? "Nunca" : $"{Minutes} min";
}

public class SettingsViewModel : ViewModelBase
{
    private const string NotifyTitle = "Ajustes";

    private readonly IChangePasswordUseCase _changePassword;
    private readonly IRegenerateRecoveryCodeUseCase _regenerateRecoveryCode;
    private readonly IExportKeysUseCase _exportKeys;
    private readonly IImportKeysUseCase _importKeys;
    private readonly IWindowService _windowService;
    private readonly AppSettingsState _settingsState;
    private readonly UserStatusService _userStatusService;
    private readonly NotifyManagerService _notifyManagerService;

    private string _currentPassword = "";
    private string _newPassword = "";
    private string _repeatPassword = "";
    private string _recoveryCode = "";
    private AutoLockOption _selectedAutoLock;
    private bool _runInBackground;
    private string _exportPassword = "";
    private string _importPassword = "";

    private static readonly FilePickerFileType BackupFileType = new("AuthGatun backup") { Patterns = ["*.authgatun"] };

    public string CurrentPassword
    {
        get => _currentPassword;
        set => this.RaiseAndSetIfChanged(ref _currentPassword, value);
    }

    public string NewPassword
    {
        get => _newPassword;
        set => this.RaiseAndSetIfChanged(ref _newPassword, value);
    }

    public string RepeatPassword
    {
        get => _repeatPassword;
        set => this.RaiseAndSetIfChanged(ref _repeatPassword, value);
    }

    public string RecoveryCode
    {
        get => _recoveryCode;
        private set => this.RaiseAndSetIfChanged(ref _recoveryCode, value);
    }

    public IReadOnlyList<AutoLockOption> AutoLockOptions { get; } =
        AppSettings.AllowedAutoLockMinutes.Select(minutes => new AutoLockOption(minutes)).ToList();

    public bool RunInBackground
    {
        get => _runInBackground;
        set
        {
            this.RaiseAndSetIfChanged(ref _runInBackground, value);
            OnRunInBackgroundChanged(value);
        }
    }

    public bool ShowLinuxTrayNote { get; } = OperatingSystem.IsLinux();

    public AutoLockOption SelectedAutoLock
    {
        get => _selectedAutoLock;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedAutoLock, value);
            OnAutoLockChanged(value);
        }
    }

    public string ExportPassword
    {
        get => _exportPassword;
        set => this.RaiseAndSetIfChanged(ref _exportPassword, value);
    }

    public string ImportPassword
    {
        get => _importPassword;
        set => this.RaiseAndSetIfChanged(ref _importPassword, value);
    }

    public ReactiveCommand<Unit, Unit> ExportKeysCommand { get; }
    public ReactiveCommand<Unit, Unit> ImportKeysCommand { get; }
    public ReactiveCommand<Unit, Unit> ChangePasswordCommand { get; }
    public ReactiveCommand<Unit, Unit> RegenerateRecoveryCodeCommand { get; }

    public SettingsViewModel(
        IChangePasswordUseCase changePassword,
        IRegenerateRecoveryCodeUseCase regenerateRecoveryCode,
        IExportKeysUseCase exportKeys,
        IImportKeysUseCase importKeys,
        IWindowService windowService,
        AppSettingsState settingsState,
        UserStatusService userStatusService,
        NotifyManagerService notifyManagerService
    )
    {
        _settingsState = settingsState;
        _runInBackground = settingsState.Current.RunInBackground;
        _selectedAutoLock = AutoLockOptions.First(option => option.Minutes == settingsState.Current.AutoLockMinutes);
        _changePassword = changePassword;
        _regenerateRecoveryCode = regenerateRecoveryCode;
        _exportKeys = exportKeys;
        _importKeys = importKeys;
        _windowService = windowService;
        _userStatusService = userStatusService;
        _notifyManagerService = notifyManagerService;

        ChangePasswordCommand = ReactiveCommand.Create(OnChangePassword);
        RegenerateRecoveryCodeCommand = ReactiveCommand.Create(OnRegenerateRecoveryCode);
        ExportKeysCommand = ReactiveCommand.CreateFromTask(OnExportKeys);
        ImportKeysCommand = ReactiveCommand.CreateFromTask(OnImportKeys);
    }

    private void OnChangePassword()
    {
        var user = _userStatusService.User;
        if (user is null)
        {
            Notify("Aun no hay ningun usuario logeado", NotificationType.Error);
            return;
        }

        if (string.IsNullOrWhiteSpace(CurrentPassword) || string.IsNullOrWhiteSpace(NewPassword))
        {
            Notify("Por favor, rellene todos los campos", NotificationType.Error);
            return;
        }

        if (NewPassword.Contains(' ') || NewPassword.Length < 8)
        {
            Notify("La nueva contraseña debe tener al menos 8 caracteres y no contener espacios", NotificationType.Error);
            return;
        }

        if (NewPassword != RepeatPassword)
        {
            Notify("La contraseña no coincide", NotificationType.Error);
            return;
        }

        try
        {
            _changePassword.Execute(user.Id, CurrentPassword, NewPassword);
            CurrentPassword = "";
            NewPassword = "";
            RepeatPassword = "";
            Notify("Contraseña actualizada con éxito", NotificationType.Success);
        }
        catch (ArgumentException e)
        {
            Notify(e.Message, NotificationType.Error);
        }
    }

    private void OnRegenerateRecoveryCode()
    {
        RecoveryCode = _regenerateRecoveryCode.Execute();
        Notify("Nuevo código de recuperación generado, el anterior ya no sirve", NotificationType.Success);
    }

    private async Task OnExportKeys()
    {
        var user = _userStatusService.User;
        var storage = _windowService.GetStorageProvider();
        if (user is null || storage is null)
        {
            Notify("No se pudo abrir el selector de archivos", NotificationType.Error);
            return;
        }

        if (string.IsNullOrWhiteSpace(ExportPassword) || ExportPassword.Length < 8)
        {
            Notify("La contraseña del respaldo debe tener al menos 8 caracteres", NotificationType.Error);
            return;
        }

        try
        {
            var backup = _exportKeys.Execute(user.Id, ExportPassword);

            var file = await storage.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Exportar claves",
                SuggestedFileName = "authgatun-backup",
                DefaultExtension = "authgatun",
                FileTypeChoices = [BackupFileType]
            });
            if (file is null) return;

            await using var stream = await file.OpenWriteAsync();
            stream.SetLength(0);
            await stream.WriteAsync(backup);

            ExportPassword = "";
            Notify("Respaldo exportado con éxito", NotificationType.Success);
        }
        catch (Exception e) when (e is ArgumentException or IOException or UnauthorizedAccessException)
        {
            Notify(e.Message, NotificationType.Error);
        }
    }

    private async Task OnImportKeys()
    {
        var user = _userStatusService.User;
        var storage = _windowService.GetStorageProvider();
        if (user is null || storage is null)
        {
            Notify("No se pudo abrir el selector de archivos", NotificationType.Error);
            return;
        }

        if (string.IsNullOrWhiteSpace(ImportPassword))
        {
            Notify("Ingresa la contraseña del respaldo", NotificationType.Error);
            return;
        }

        try
        {
            var files = await storage.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Importar claves",
                AllowMultiple = false,
                FileTypeFilter = [BackupFileType]
            });
            if (files.Count == 0) return;

            await using var stream = await files[0].OpenReadAsync();
            using var memory = new MemoryStream();
            await stream.CopyToAsync(memory);

            var result = _importKeys.Execute(user.Id, memory.ToArray(), ImportPassword);

            ImportPassword = "";
            Notify($"Claves importadas: {result.Imported}, omitidas por duplicadas o inválidas: {result.Skipped}", NotificationType.Success);
        }
        catch (Exception e) when (e is ArgumentException or IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            Notify(e.Message, NotificationType.Error);
        }
    }

    private void OnRunInBackgroundChanged(bool runInBackground)
    {
        if (runInBackground == _settingsState.Current.RunInBackground)
            return;

        _settingsState.Update(_settingsState.Current with { RunInBackground = runInBackground });
    }

    private void OnAutoLockChanged(AutoLockOption option)
    {
        if (option.Minutes == _settingsState.Current.AutoLockMinutes)
            return;

        _settingsState.Update(_settingsState.Current with { AutoLockMinutes = option.Minutes });
        Notify("Bloqueo automático actualizado", NotificationType.Success);
    }

    private void Notify(string message, NotificationType type) =>
        _notifyManagerService.SendNotifyInWindow(message, title: NotifyTitle, type);
}
