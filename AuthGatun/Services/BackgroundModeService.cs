using System;
using AuthGatun.Core.AvaloniaUI;
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
    private TrayIcon? _trayIcon;
    private bool _exiting;

    public BackgroundModeService(AppSettingsState settings, IWindowService windowService, ILogoutUseCase logout)
    {
        _settings = settings;
        _windowService = windowService;
        _logout = logout;
        _settings.Changed += Apply;
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

    private TrayIcon EnsureTrayIcon()
    {
        if (_trayIcon is not null)
            return _trayIcon;

        var openItem = new NativeMenuItem("Abrir");
        openItem.Click += (_, _) => ShowCurrent();

        var lockItem = new NativeMenuItem("Cerrar sesión");
        lockItem.Click += (_, _) => (_windowService.CurrentWindow()?.DataContext as MainWindowViewModel)?.Lock();

        var exitItem = new NativeMenuItem("Salir");
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
