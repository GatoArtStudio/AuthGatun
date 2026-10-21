using System;
using System.Reactive;
using AuthGatun.Domains.IdentityAccess.Application.Ports.Inbound;
using AuthGatun.Services;
using Avalonia.Controls.Notifications;
using ReactiveUI;

namespace AuthGatun.ViewModels;

public class SettingsViewModel : ViewModelBase
{
    private const string NotifyTitle = "Ajustes";

    private readonly IChangePasswordUseCase _changePassword;
    private readonly IRegenerateRecoveryCodeUseCase _regenerateRecoveryCode;
    private readonly UserStatusService _userStatusService;
    private readonly NotifyManagerService _notifyManagerService;

    private string _currentPassword = "";
    private string _newPassword = "";
    private string _repeatPassword = "";
    private string _recoveryCode = "";

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

    public ReactiveCommand<Unit, Unit> ChangePasswordCommand { get; }
    public ReactiveCommand<Unit, Unit> RegenerateRecoveryCodeCommand { get; }

    public SettingsViewModel(
        IChangePasswordUseCase changePassword,
        IRegenerateRecoveryCodeUseCase regenerateRecoveryCode,
        UserStatusService userStatusService,
        NotifyManagerService notifyManagerService
    )
    {
        _changePassword = changePassword;
        _regenerateRecoveryCode = regenerateRecoveryCode;
        _userStatusService = userStatusService;
        _notifyManagerService = notifyManagerService;

        ChangePasswordCommand = ReactiveCommand.Create(OnChangePassword);
        RegenerateRecoveryCodeCommand = ReactiveCommand.Create(OnRegenerateRecoveryCode);
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

    private void Notify(string message, NotificationType type) =>
        _notifyManagerService.SendNotifyInWindow(message, title: NotifyTitle, type);
}
