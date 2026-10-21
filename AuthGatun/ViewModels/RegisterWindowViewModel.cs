using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using AuthGatun.Core.AvaloniaUI;
using AuthGatun.Domains.IdentityAccess.Application.Ports.Inbound;
using AuthGatun.Domains.IdentityAccess.Application.Models;
using AuthGatun.Services;
using AuthGatun.Views;
using CommunityToolkit.Mvvm.Input;

namespace AuthGatun.ViewModels;

public class RegisterWindowViewModel(
    IViewModelFactory viewModelFactory,
    IWindowFactory windowFactory,
    DiscordService discordService,
    IRegisterUseCase register,
    IWindowService windowService,
    UserStatusService userStatusService
) : ViewModelBase
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private string _username = "";
    private string _newpassword = "";
    private string _repeatpassword = "";
    private string _messagestatus = "";
    private string _recoveryCode = "";
    private bool _showRecoveryCode;

    public string Username
    {
        get => _username;
        set => SetField(ref _username, value);
    }

    public string Newpassword
    {
        get => _newpassword;
        set => SetField(ref _newpassword, value);
    }

    public string Repeatpassword
    {
        get => _repeatpassword;
        set => SetField(ref _repeatpassword, value);
    }

    public string Messagestatus
    {
        get => _messagestatus;
        set => SetField(ref _messagestatus, value);
    }

    public string RecoveryCode
    {
        get => _recoveryCode;
        private set => SetField(ref _recoveryCode, value);
    }

    public bool ShowRecoveryCode
    {
        get => _showRecoveryCode;
        private set
        {
            if (SetField(ref _showRecoveryCode, value))
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowForm)));
        }
    }

    public bool ShowForm => !ShowRecoveryCode;

    public ICommand ContinueCommand => new RelayCommand(OnContinue);
    public ICommand RegisterCommand => new RelayCommand(OnRegister);
    public ICommand ChangeToLoginWindowCommand => new RelayCommand(OnChangeToLoginWindow);

    private void OnRegister()
    {
        if (string.IsNullOrWhiteSpace(Username) || Username.Contains(' '))
        {
            Messagestatus = "El usuario no puede ser nulo o con espacios.";
            return;
        }
        
        if (string.IsNullOrWhiteSpace(Newpassword) || Newpassword.Contains(' '))
        {
            Messagestatus = "La contraseña no puede ser nulo o con espacios.";
            return;
        }
        
        if (string.IsNullOrWhiteSpace(Newpassword) || Repeatpassword.Contains(' '))
        {
            Messagestatus = "La contraseña repetida no puede ser nulo o con espacios.";
            return;
        }

        if (Newpassword != Repeatpassword)
        {
            Messagestatus = "La contraseña no coincide.";
            return;
        }

        if (Newpassword.Length < 8)
        {
            Messagestatus = "La contraseña no puede contener menos de 8 caracteres";
            return;
        }

        try
        {
            RegistrationResult result = register.Execute(Username, Newpassword);
            userStatusService.User = result.User;
            RecoveryCode = result.RecoveryCode;
            Newpassword = "";
            Repeatpassword = "";
            Messagestatus = "";
            ShowRecoveryCode = true;
        }
        catch (Exception e)
        {
            Messagestatus = $"Hubo un error, {e.Message}";
        }
    }
    private void OnContinue()
    {
        var mainViewModel = viewModelFactory.Create<MainWindowViewModel>();
        var mainWindow = windowFactory.Create<MainWindow, MainWindowViewModel>(mainViewModel);
        mainWindow.Show();

        windowService.Close(this);
    }

    private void OnChangeToLoginWindow()
    {
        var loginViewModel = viewModelFactory.Create<LoginWindowViewModel>();
        var loginWindow = windowFactory.Create<LoginWindow, LoginWindowViewModel>(loginViewModel);
        loginWindow.Show();
        
        windowService.Close(this);
        
        discordService.UpdatePresence("AuthGatun", "Iniciemos sesión!.");
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