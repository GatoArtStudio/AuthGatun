using System;
using AuthGatun.Core.AvaloniaUI;
using AuthGatun.Core.Localization;
using AuthGatun.Domains.IdentityAccess.Application.Ports.Inbound;
using AuthGatun.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform;

namespace AuthGatun.Services;

public class BackgroundModeService : IBackgroundModeService
{
    private readonly AppSettingsState _settings;
    private readonly IWindowService _windowService;
    private readonly ILogoutUseCase _logout;
    private readonly ILocalizer _localizer;
    private TrayIcon? _trayIcon;
    private NativeMenuItem? _openItem;
    private NativeMenuItem? _lockItem;
    private NativeMenuItem? _exitItem;
    private bool _exiting;

    public BackgroundModeService(AppSettingsState settings, IWindowService windowService, ILogoutUseCase logout, ILocalizer localizer)
    {
        _localizer = localizer;
        _settings = settings;
        _windowService = windowService;
        _logout = logout;
        _settings.Changed += Apply;
        _localizer.LanguageChanged += UpdateTrayLabels;
    }

    public bool IsEnabled => _settings.Current.RunInBackground;

    public bool IsHidden => _windowService.CurrentWindow() is not { IsVisible: true };

    public void Attach(Window window)
    {
        window.Closing += (_, e) =>
        {
            if (!IsEnabled || _exiting || _windowService.IsForceClosing(window))
                return;

            if (e.CloseReason is WindowCloseReason.ApplicationShutdown or WindowCloseReason.OSShutdown
                or WindowCloseReason.OwnerWindowClosing)
                return;

            e.Cancel = true;
            window.Hide();
        };
    }

    public void Apply()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.ShutdownMode = IsEnabled ? ShutdownMode.OnExplicitShutdown : ShutdownMode.OnLastWindowClose;
        }

        if (IsEnabled)
            EnsureTrayIcon().IsVisible = true;
        else if (_trayIcon is not null)
            _trayIcon.IsVisible = false;
    }

    public void ShowCurrent()
    {
        var window = _windowService.CurrentWindow();
        if (window is null)
            return;

        window.Show();
        if (window.WindowState == WindowState.Minimized)
            window.WindowState = WindowState.Normal;
        window.Activate();
    }

    public void Exit()
    {
        _exiting = true;
        _logout.Execute();
        _trayIcon?.Dispose();
        _trayIcon = null;

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.Shutdown();
    }

    private void UpdateTrayLabels()
    {
        if (_openItem is not null) _openItem.Header = _localizer.Get("Tray_Open");
        if (_lockItem is not null) _lockItem.Header = _localizer.Get("Tray_Lock");
        if (_exitItem is not null) _exitItem.Header = _localizer.Get("Tray_Quit");
    }

    private TrayIcon EnsureTrayIcon()
    {
        if (_trayIcon is not null)
            return _trayIcon;

        var openItem = _openItem = new NativeMenuItem(_localizer.Get("Tray_Open"));
        openItem.Click += (_, _) => ShowCurrent();

        var lockItem = _lockItem = new NativeMenuItem(_localizer.Get("Tray_Lock"));
        lockItem.Click += (_, _) => (_windowService.CurrentWindow()?.DataContext as MainWindowViewModel)?.Lock();

        var exitItem = _exitItem = new NativeMenuItem(_localizer.Get("Tray_Quit"));
        exitItem.Click += (_, _) => Exit();

        _trayIcon = new TrayIcon
        {
            Icon = new WindowIcon(AssetLoader.Open(new Uri("avares://AuthGatun/Assets/logo.ico"))),
            ToolTipText = "AuthGatun",
            Menu = [openItem, lockItem, exitItem]
        };
        _trayIcon.Clicked += (_, _) => ShowCurrent();

        TrayIcon.SetIcons(Application.Current!, [_trayIcon]);
        return _trayIcon;
    }
}
