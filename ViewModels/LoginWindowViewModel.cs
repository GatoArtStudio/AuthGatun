using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using AuthGatun.Domains.IdentityAccess.Application.Ports.In.UseCases;
using AuthGatun.Domains.IdentityAccess.Application.UseCases;
using AuthGatun.Domains.IdentityAccess.Infrastructure.Eventbus;
using AuthGatun.Domains.IdentityAccess.Infrastructure.Persistence;
using AuthGatun.Domains.IdentityAccess.Infrastructure.Services;
using AuthGatun.Services;
using AuthGatun.Views;
using CommunityToolkit.Mvvm.Input;
using ReactiveUI;

namespace AuthGatun.ViewModels;

public class LoginWindowViewModel(App app) : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    private readonly App _app = app ?? throw new ArgumentNullException(nameof(app), "App cannot be null");
    private readonly ILoginUseCase _login = new LoginUseCase(BusFactory.GetInstance().CreateBus(),
        RepositoryFactory.GetInstance().CreateRepository(), new Argon2PasswordHasher());

    private string _username = "";
    private string _password = "";
    private string _messagestatus = "";

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
        }
        
        var userLogin = _login.Execute(Username, Password);
        if (userLogin is null)
        {
            Messagestatus = "Usuario incorrecto o contraseña invalidad";
            return;
        }

        UserStatus.GetInstance().User = userLogin;
        var windows = new MainWindow();
        windows.DataContext = new MainWindowViewModel(windows);
        _app.SetWindow(windows);
    }

    private void OnChangeToRegisterWindow()
    {
        var windows = new RegisterWindow();
        windows.DataContext = new RegisterWindowViewModel(_app);
        _app.SetWindow(windows);
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