using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using AuthGatun.Core.AvaloniaUI;
using AuthGatun.Domains.IdentityAccess.Application.Ports.Inbound;
using AuthGatun.Services;
using AuthGatun.Views;
using CommunityToolkit.Mvvm.Input;

namespace AuthGatun.ViewModels;

public class LoginWindowViewModel(
    IViewModelFactory viewModelFactory,
    IWindowFactory windowFactory,
    DiscordService discordService,
    ILoginUseCase login,
    IRecoverAccessUseCase recoverAccess,
    IWindowService windowService,
    UserStatusService userStatusService
) : ViewModelBase
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private string _username = "";
    private string _password = "";
    private string _messagestatus = "";
    private string _recoveryCode = "";
    private string _newPassword = "";
    private bool _isRecoveryMode;
    private int _failedAttempts;
    private DateTime _lockedUntil = DateTime.MinValue;

    private const int AttemptsBeforeDelay = 3;
    private const int MaxDelaySeconds = 60;

    public string Username
    {
        get => _username;
        set => SetField(ref _username, value);
    }

    public string Password
    {
        get => _password;
        set => SetField(ref _password, value);
    }

    public string Messagestatus
    {
        get => _messagestatus;
        set => SetField(ref _messagestatus, value);
    }

    public string RecoveryCode
    {
        get => _recoveryCode;
        set => SetField(ref _recoveryCode, value);
    }

    public string NewPassword
    {
        get => _newPassword;
        set => SetField(ref _newPassword, value);
    }

    public bool IsRecoveryMode
    {
        get => _isRecoveryMode;
        private set
        {
            if (SetField(ref _isRecoveryMode, value))
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsLoginMode)));
        }
    }

    public bool IsLoginMode => !IsRecoveryMode;

    public ICommand ToggleRecoveryModeCommand => new RelayCommand(() =>
    {
        Messagestatus = "";
        IsRecoveryMode = !IsRecoveryMode;
    });
    public ICommand RecoverAccessCommand => new RelayCommand(OnRecoverAccess);
    public ICommand LoginVerificationCommand => new RelayCommand(OnLogin);
    public ICommand ChangeToRegisterWindowCommand => new RelayCommand(OnChangeToRegisterWindow);

    private void OnLogin()
    {
        if (string.IsNullOrWhiteSpace(Username) || Username.Contains(' '))
        {
            Messagestatus = "El usuario no puede ser nulo o contener espacios";
            return;
        }

        if (string.IsNullOrWhiteSpace(Password) || Password.Contains(' '))
        {
            Messagestatus = "La contraseña no puede ser nula o contener espacios";
            return;
        }
        
        if (IsThrottled())
            return;

        var userLogin = login.Execute(Username, Password);
        if (userLogin is null)
        {
            RegisterFailedAttempt();
            Messagestatus = "Usuario incorrecto o contraseña invalidad";
            return;
        }

        _failedAttempts = 0;
        userStatusService.User = userLogin;
        
        var mainViewModel = viewModelFactory.Create<MainWindowViewModel>();
        var mainWindow = windowFactory.Create<MainWindow, MainWindowViewModel>(mainViewModel);
        mainWindow.Show();
        
        windowService.Close(this);
    }

    private void OnRecoverAccess()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(RecoveryCode))
        {
            Messagestatus = "Ingresa el usuario y el código de recuperación";
            return;
        }

        if (string.IsNullOrWhiteSpace(NewPassword) || NewPassword.Length < 8 || NewPassword.Contains(' '))
        {
            Messagestatus = "La nueva contraseña debe tener al menos 8 caracteres y no contener espacios";
            return;
        }

        if (IsThrottled())
            return;

        var user = recoverAccess.Execute(Username, RecoveryCode, NewPassword);
        if (user is null)
        {
            RegisterFailedAttempt();
            Messagestatus = "Usuario o código de recuperación incorrecto";
            return;
        }

        _failedAttempts = 0;
        userStatusService.User = user;

        var mainViewModel = viewModelFactory.Create<MainWindowViewModel>();
        var mainWindow = windowFactory.Create<MainWindow, MainWindowViewModel>(mainViewModel);
        mainWindow.Show();

        windowService.Close(this);
    }

    private bool IsThrottled()
    {
        var remaining = _lockedUntil - DateTime.UtcNow;
        if (remaining <= TimeSpan.Zero)
            return false;

        Messagestatus = $"Demasiados intentos, espera {Math.Ceiling(remaining.TotalSeconds)} segundos";
        return true;
    }

    private void RegisterFailedAttempt()
    {
        _failedAttempts++;
        if (_failedAttempts < AttemptsBeforeDelay)
            return;

        var delaySeconds = Math.Min(MaxDelaySeconds, 1 << Math.Min(_failedAttempts - AttemptsBeforeDelay + 1, 6));
        _lockedUntil = DateTime.UtcNow.AddSeconds(delaySeconds);
    }

    private void OnChangeToRegisterWindow()
    {

        var registerViewModel = viewModelFactory.Create<RegisterWindowViewModel>();
        var registerWindow = windowFactory.Create<RegisterWindow, RegisterWindowViewModel>(registerViewModel);
        registerWindow.Show();
        
        windowService.Close(this);
        
        discordService.UpdatePresence("AuthGatun", "Registrando un nuevo usuario.");
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }
}