using System.Reactive;
using AuthGatun.Core.AvaloniaUI;
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
    
    public ReactiveCommand<Unit, Unit> ShowHomeViewCommand { get; }
    public ReactiveCommand<Unit, Unit> ShowKeysViewCommand { get; }
    public ReactiveCommand<Unit, Unit> ShowSettingsViewCommand { get; }
    
    public MainWindowViewModel(
        DiscordService discordService,
        IViewModelFactory viewModelFactory,
        IWindowFactory windowFactory,
        IUserControlFactory userControlFactory
    )
    {
        _discordService = discordService;
        _viewModelFactory = viewModelFactory;
        _windowFactory = windowFactory;
        _userControlFactory = userControlFactory;
        
        ShowHomeViewCommand = ReactiveCommand.Create(ShowHomeView);
        ShowKeysViewCommand = ReactiveCommand.Create(ShowKeysView);
        ShowSettingsViewCommand = ReactiveCommand.Create(ShowSettingsView);
        ShowHomeView();
    }
    // Seteamos la View actual
    public UserControl CurrentView
    {
        get => _currentView;
        set => this.RaiseAndSetIfChanged(ref _currentView, value);
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
        var settingsViewUserControl = _userControlFactory.Create<SettingsView>();
        CurrentView = settingsViewUserControl;
    }
}
