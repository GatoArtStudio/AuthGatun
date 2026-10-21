using System.Reactive;
using AuthGatun.Core.AvaloniaUI;
using AuthGatun.Domains.IdentityAccess.Application.Ports.Inbound;
using AuthGatun.Services;
using AuthGatun.Views;
using Avalonia.Controls;
using ReactiveUI;

namespace AuthGatun.ViewModels;
public class MainWindowViewModel : ViewModelBase
{
    private UserControl _currentView;
    private DiscordService _discordService;
    private IViewModelFactory _viewModelFactory;
    private IWindowFactory _windowFactory;
    private IUserControlFactory _userControlFactory;
    private readonly ILogoutUseCase _logout;
    private readonly UserStatusService _userStatusService;
    private readonly IWindowService _windowService;
    
    public ReactiveCommand<Unit, Unit> ShowHomeViewCommand { get; }
    public ReactiveCommand<Unit, Unit> ShowKeysViewCommand { get; }
    public ReactiveCommand<Unit, Unit> ShowSettingsViewCommand { get; }
    public ReactiveCommand<Unit, Unit> LogoutCommand { get; }
    
    public MainWindowViewModel(
        DiscordService discordService,
        IViewModelFactory viewModelFactory,
        IWindowFactory windowFactory,
        IUserControlFactory userControlFactory,
        ILogoutUseCase logout,
        UserStatusService userStatusService,
        IWindowService windowService
    )
    {
        _discordService = discordService;
        _viewModelFactory = viewModelFactory;
        _windowFactory = windowFactory;
        _userControlFactory = userControlFactory;
        _logout = logout;
        _userStatusService = userStatusService;
        _windowService = windowService;
        
        ShowHomeViewCommand = ReactiveCommand.Create(ShowHomeView);
        ShowKeysViewCommand = ReactiveCommand.Create(ShowKeysView);
        ShowSettingsViewCommand = ReactiveCommand.Create(ShowSettingsView);
        LogoutCommand = ReactiveCommand.Create(Logout);
        ShowHomeView();
    }
    // Seteamos la View actual
    public UserControl CurrentView
    {
        get => _currentView;
        set => this.RaiseAndSetIfChanged(ref _currentView, value);
    }
    
    public void Lock() => Logout();

    private void Logout()
    {
        _logout.Execute();
        _userStatusService.User = null;

        var loginViewModel = _viewModelFactory.Create<LoginWindowViewModel>();
        var loginWindow = _windowFactory.Create<LoginWindow, LoginWindowViewModel>(loginViewModel);
        loginWindow.Show();

        _windowService.Close(this);
        _discordService.UpdatePresence("AuthGatun", "Iniciemos sesión!.");
    }

    private void ShowHomeView()
    {
        var homeViewModel = _viewModelFactory.Create<HomeViewModel>();
        var homeViewUserControl = _userControlFactory.Create<HomeView, HomeViewModel>(homeViewModel);
        CurrentView = homeViewUserControl;
    }

    private void ShowKeysView()
    {
        var keyViewModel = _viewModelFactory.Create<KeyViewModel>();
        var keysViewUserControl = _userControlFactory.Create<KeysView, KeyViewModel>(keyViewModel);
        CurrentView = keysViewUserControl;
    }

    private void ShowSettingsView()
    {
        var settingsViewModel = _viewModelFactory.Create<SettingsViewModel>();
        var settingsViewUserControl = _userControlFactory.Create<SettingsView, SettingsViewModel>(settingsViewModel);
        CurrentView = settingsViewUserControl;
    }
}
