using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using AuthGatun.Core.AvaloniaUI;
using AuthGatun.Domains.IdentityAccess.Application.Ports.In.UseCases;
using AuthGatun.Domains.IdentityAccess.Application.UseCases;
using AuthGatun.Domains.IdentityAccess.Infrastructure.Eventbus;
using AuthGatun.Domains.IdentityAccess.Infrastructure.Persistence;
using AuthGatun.Domains.IdentityAccess.Infrastructure.Services;
using AuthGatun.Services;
using AuthGatun.Views;
using CommunityToolkit.Mvvm.Input;

namespace AuthGatun.ViewModels;

public class LoginWindowViewModel(
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
    private readonly ILoginUseCase _login = new LoginUseCase(busFactory.CreateBus(),
        repositoryFactory.CreateRepository(), new Argon2PasswordHasher());

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

        userStatusService.User = userLogin;
        
        var mainViewModel = viewModelFactory.Create<MainWindowViewModel>();
        var mainWindow = windowFactory.Create<MainWindow, MainWindowViewModel>(mainViewModel);
        mainWindow.Show();
        
        windowService.Close(this);
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