using System.Reactive;
using AuthGatun.Core.AvaloniaUI;
using AuthGatun.Core.Localization;
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
    private readonly IBackgroundModeService _backgroundMode;
    private readonly ILocalizer _localizer;
    
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
        IWindowService windowService,
        IBackgroundModeService backgroundMode,
        ILocalizer localizer
    )
    {
        _discordService = discordService;
        _viewModelFactory = viewModelFactory;
        _windowFactory = windowFactory;
        _userControlFactory = userControlFactory;
        _logout = logout;
        _userStatusService = userStatusService;
        _windowService = windowService;
        _backgroundMode = backgroundMode;
        _localizer = localizer;
        
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
        var wasHidden = _backgroundMode.IsHidden;

        _logout.Execute();
        _userStatusService.User = null;

        var loginViewModel = _viewModelFactory.Create<LoginWindowViewModel>();
        var loginWindow = _windowFactory.Create<LoginWindow, LoginWindowViewModel>(loginViewModel);
        if (!wasHidden)
            loginWindow.Show();

        _windowService.Close(this);
        _discordService.UpdatePresence("AuthGatun", _localizer.Get("Presence_Login"));
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
