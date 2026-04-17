using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using AuthGatun.Core.AvaloniaUI;
using AuthGatun.Domains.IdentityAccess.Application.Ports.In.UseCases;
using AuthGatun.Domains.IdentityAccess.Application.UseCases;
using AuthGatun.Domains.IdentityAccess.Domain.Aggregate;
using AuthGatun.Domains.IdentityAccess.Infrastructure.Eventbus;
using AuthGatun.Domains.IdentityAccess.Infrastructure.Persistence;
using AuthGatun.Domains.IdentityAccess.Infrastructure.Services;
using AuthGatun.Services;
using AuthGatun.Views;
using CommunityToolkit.Mvvm.Input;

namespace AuthGatun.ViewModels;

public class RegisterWindowViewModel(
    IViewModelFactory viewModelFactory,
    IWindowFactory windowFactory,
    DiscordService discordService,
    RepositoryFactory repositoryFactory,
    BusFactory busFactory,
    IWindowService windowService,
    UserStatusService userStatusService
) : ViewModelBase
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private readonly IRegisterUseCase _register = new RegisterUserCase(busFactory.CreateBus(),
        repositoryFactory.CreateRepository(), new Argon2PasswordHasher());

    private string _username = "";
    private string _newpassword = "";
    private string _repeatpassword = "";
    private string _messagestatus = "";

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
            User newUser = _register.Execute(Username, Newpassword);
            userStatusService.User = newUser;
            
            var mainViewModel = viewModelFactory.Create<MainWindowViewModel>();
            var mainWindow = windowFactory.Create<MainWindow, MainWindowViewModel>(mainViewModel);
            mainWindow.Show();
            
            windowService.Close(this);
        }
        catch (Exception e)
        {
            Messagestatus = $"Hubo un error, {e.Message}";
        }
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