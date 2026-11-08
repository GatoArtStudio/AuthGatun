using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using AuthGatun.Core.AvaloniaUI;
using AuthGatun.Core.Localization;
using AuthGatun.Domains.IdentityAccess.Application;
using AuthGatun.Domains.IdentityAccess.Application.Ports.Inbound;
using AuthGatun.Domains.Settings.Domain;
using AuthGatun.Services;
using Avalonia.Controls.Notifications;
using Avalonia.Platform.Storage;
using ReactiveUI;

namespace AuthGatun.ViewModels;

public class AutoLockOption(int minutes, ILocalizer localizer) : ViewModelBase
{
    public int Minutes { get; } = minutes;

    public string Label => Minutes == 0 ? localizer.Get("Settings_AutoLockNever") : localizer.Get("Settings_AutoLockMinutes", Minutes);

    public void RefreshLabel() => this.RaisePropertyChanged(nameof(Label));
}

public class ThemeOption(string code, ILocalizer localizer) : ViewModelBase
{
    public string Code { get; } = code;

    public string Label => localizer.Get(Code switch
    {
        "light" => "Settings_ThemeLight",
        "system" => "Settings_ThemeSystem",
        "catppuccin-mocha" => "Settings_ThemeCatppuccinMocha",
        _ => "Settings_ThemeDark"
    });

    public void RefreshLabel() => this.RaisePropertyChanged(nameof(Label));
}

public record LanguageOption(string Code, string Name)
{
    public override string ToString() => Name;
}

public class SettingsViewModel : ViewModelBase
{
    private readonly IChangePasswordUseCase _changePassword;
    private readonly IRegenerateRecoveryCodeUseCase _regenerateRecoveryCode;
    private readonly IExportKeysUseCase _exportKeys;
    private readonly IImportKeysUseCase _importKeys;
    private readonly IWindowService _windowService;
    private readonly AppSettingsState _settingsState;
    private readonly UserStatusService _userStatusService;
    private readonly NotifyManagerService _notifyManagerService;
    private readonly ILocalizer _localizer;

    private string NotifyTitle => _localizer.Get("Settings_NotifyTitle");

    private string _currentPassword = "";
    private string _newPassword = "";
    private string _repeatPassword = "";
    private string _recoveryCode = "";
    private AutoLockOption _selectedAutoLock;
    private ThemeOption _selectedTheme;
    private bool _privacyMode;
    private LanguageOption _selectedLanguage;
    private bool _runInBackground;
    private bool _hideFromScreenCapture;
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

    public IReadOnlyList<AutoLockOption> AutoLockOptions { get; }

    public IReadOnlyList<LanguageOption> LanguageOptions { get; } =
    [
        new("en", "English"),
        new("es", "Español")
    ];

    public LanguageOption SelectedLanguage
    {
        get => _selectedLanguage;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedLanguage, value);
            OnLanguageChanged(value);
        }
    }

    public bool RunInBackground
    {
        get => _runInBackground;
        set
        {
            this.RaiseAndSetIfChanged(ref _runInBackground, value);
            OnRunInBackgroundChanged(value);
        }
    }

    public bool IsScreenCaptureExclusionSupported { get; }

    public bool ShowScreenCaptureUnsupportedNote => !IsScreenCaptureExclusionSupported;

    public IReadOnlyList<ThemeOption> ThemeOptions { get; }

    public ThemeOption SelectedTheme
    {
        get => _selectedTheme;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedTheme, value);
            if (value.Code != _settingsState.Current.Theme)
                _settingsState.Update(_settingsState.Current with { Theme = value.Code });
        }
    }

    public bool PrivacyMode
    {
        get => _privacyMode;
        set
        {
            this.RaiseAndSetIfChanged(ref _privacyMode, value);
            if (value != _settingsState.Current.PrivacyMode)
                _settingsState.Update(_settingsState.Current with { PrivacyMode = value });
        }
    }

    public bool HideFromScreenCapture
    {
        get => _hideFromScreenCapture;
        set
        {
            if (!IsScreenCaptureExclusionSupported)
                value = false;

            this.RaiseAndSetIfChanged(ref _hideFromScreenCapture, value);
            OnHideFromScreenCaptureChanged(value);
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
        NotifyManagerService notifyManagerService,
        ScreenCaptureService screenCapture,
        ILocalizer localizer
    )
    {
        _localizer = localizer;
        ThemeOptions = AppSettings.SupportedThemes.Select(code => new ThemeOption(code, localizer)).ToList();
        _selectedTheme = ThemeOptions.First(option => option.Code == settingsState.Current.Theme);
        _privacyMode = settingsState.Current.PrivacyMode;
        AutoLockOptions = AppSettings.AllowedAutoLockMinutes.Select(minutes => new AutoLockOption(minutes, localizer)).ToList();
        _selectedLanguage = LanguageOptions.FirstOrDefault(option => option.Code == settingsState.Current.Language) ?? LanguageOptions[0];
        localizer.LanguageChanged += () =>
        {
            foreach (var option in AutoLockOptions)
                option.RefreshLabel();
            foreach (var option in ThemeOptions)
                option.RefreshLabel();
        };
        _settingsState = settingsState;
        _runInBackground = settingsState.Current.RunInBackground;
        IsScreenCaptureExclusionSupported = screenCapture.IsSupported;
        _hideFromScreenCapture = screenCapture.IsSupported && settingsState.Current.HideFromScreenCapture;
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
            Notify(_localizer.Get("Settings_NoUser"), NotificationType.Error);
            return;
        }

        if (string.IsNullOrWhiteSpace(CurrentPassword) || string.IsNullOrWhiteSpace(NewPassword))
        {
            Notify(_localizer.Get("Settings_FillAll"), NotificationType.Error);
            return;
        }

        if (NewPassword.Contains(' ') || NewPassword.Length < 8)
        {
            Notify(_localizer.Get("Settings_NewPasswordInvalid"), NotificationType.Error);
            return;
        }

        if (NewPassword != RepeatPassword)
        {
            Notify(_localizer.Get("Settings_Mismatch"), NotificationType.Error);
            return;
        }

        try
        {
            _changePassword.Execute(user.Id, CurrentPassword, NewPassword);
            CurrentPassword = "";
            NewPassword = "";
            RepeatPassword = "";
            Notify(_localizer.Get("Settings_PasswordUpdated"), NotificationType.Success);
        }
        catch (IdentityAccessException e)
        {
            Notify(_localizer.Get("Error_" + e.Error), NotificationType.Error);
        }
    }

    private void OnRegenerateRecoveryCode()
    {
        RecoveryCode = _regenerateRecoveryCode.Execute();
        Notify(_localizer.Get("Settings_RecoveryGenerated"), NotificationType.Success);
    }

    private async Task OnExportKeys()
    {
        var user = _userStatusService.User;
        var storage = _windowService.GetStorageProvider();
        if (user is null || storage is null)
        {
            Notify(_localizer.Get("Settings_NoFilePicker"), NotificationType.Error);
            return;
        }

        if (string.IsNullOrWhiteSpace(ExportPassword) || ExportPassword.Length < 8)
        {
            Notify(_localizer.Get("Settings_ExportPasswordShort"), NotificationType.Error);
            return;
        }

        try
        {
            var backup = _exportKeys.Execute(user.Id, ExportPassword);

            var file = await storage.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = _localizer.Get("Settings_ExportTitle"),
                SuggestedFileName = "authgatun-backup",
                DefaultExtension = "authgatun",
                FileTypeChoices = [BackupFileType]
            });
            if (file is null) return;

            await using var stream = await file.OpenWriteAsync();
            stream.SetLength(0);
            await stream.WriteAsync(backup);

            ExportPassword = "";
            Notify(_localizer.Get("Settings_Exported"), NotificationType.Success);
        }
        catch (IdentityAccessException e)
        {
            Notify(_localizer.Get("Error_" + e.Error), NotificationType.Error);
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            Notify(_localizer.Get("Error_Unexpected"), NotificationType.Error);
        }
    }

    private async Task OnImportKeys()
    {
        var user = _userStatusService.User;
        var storage = _windowService.GetStorageProvider();
        if (user is null || storage is null)
        {
            Notify(_localizer.Get("Settings_NoFilePicker"), NotificationType.Error);
            return;
        }

        if (string.IsNullOrWhiteSpace(ImportPassword))
        {
            Notify(_localizer.Get("Settings_ImportPasswordMissing"), NotificationType.Error);
            return;
        }

        try
        {
            var files = await storage.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = _localizer.Get("Settings_ImportTitle"),
                AllowMultiple = false,
                FileTypeFilter = [BackupFileType]
            });
            if (files.Count == 0) return;

            await using var stream = await files[0].OpenReadAsync();
            using var memory = new MemoryStream();
            await stream.CopyToAsync(memory);

            var result = _importKeys.Execute(user.Id, memory.ToArray(), ImportPassword);

            ImportPassword = "";
            Notify(_localizer.Get("Settings_Imported", result.Imported, result.Skipped), NotificationType.Success);
        }
        catch (IdentityAccessException e)
        {
            Notify(_localizer.Get("Error_" + e.Error), NotificationType.Error);
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            Notify(_localizer.Get("Error_Unexpected"), NotificationType.Error);
        }
    }

    private void OnRunInBackgroundChanged(bool runInBackground)
    {
        if (runInBackground == _settingsState.Current.RunInBackground)
            return;

        _settingsState.Update(_settingsState.Current with { RunInBackground = runInBackground });
    }

    private void OnHideFromScreenCaptureChanged(bool hide)
    {
        if (!IsScreenCaptureExclusionSupported || hide == _settingsState.Current.HideFromScreenCapture)
            return;

        _settingsState.Update(_settingsState.Current with { HideFromScreenCapture = hide });
        Notify(_localizer.Get(hide ? "Settings_CaptureHidden" : "Settings_CaptureVisible"), NotificationType.Success);
    }

    private void OnLanguageChanged(LanguageOption option)
    {
        if (option.Code == _settingsState.Current.Language)
            return;

        _settingsState.Update(_settingsState.Current with { Language = option.Code });
    }

    private void OnAutoLockChanged(AutoLockOption option)
    {
        if (option.Minutes == _settingsState.Current.AutoLockMinutes)
            return;

        _settingsState.Update(_settingsState.Current with { AutoLockMinutes = option.Minutes });
        Notify(_localizer.Get("Settings_AutoLockUpdated"), NotificationType.Success);
    }

    private void Notify(string message, NotificationType type) =>
        _notifyManagerService.SendNotifyInWindow(message, title: NotifyTitle, type);
}
