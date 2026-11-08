using System;
using AuthGatun.Services;
using AuthGatun.ViewModels;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace AuthGatun.Views;

public partial class MainWindow : Window
{
    private const int DefaultAutoLockMinutes = 5;

    private readonly DispatcherTimer _inactivityTimer = new();
    private readonly AppSettingsState? _settings = App.ServiceProvider?.GetService<AppSettingsState>();

    public MainWindow()
    {
        InitializeComponent();

        _inactivityTimer.Tick += OnInactivityTimeout;
        AddHandler(PointerMovedEvent, OnActivity, RoutingStrategies.Tunnel, handledEventsToo: true);
        AddHandler(PointerPressedEvent, OnActivity, RoutingStrategies.Tunnel, handledEventsToo: true);
        AddHandler(KeyDownEvent, OnActivity, RoutingStrategies.Tunnel, handledEventsToo: true);

        Opened += (_, _) =>
        {
            ApplyAutoLock();
            if (_settings is not null) _settings.Changed += ApplyAutoLock;
        };
        Closed += (_, _) =>
        {
            _inactivityTimer.Stop();
            if (_settings is not null) _settings.Changed -= ApplyAutoLock;
        };
    }

    private void ApplyAutoLock()
    {
        _inactivityTimer.Stop();

        var minutes = _settings?.Current.AutoLockMinutes ?? DefaultAutoLockMinutes;
        if (minutes == 0)
            return;

        _inactivityTimer.Interval = TimeSpan.FromMinutes(minutes);
        _inactivityTimer.Start();
    }

    private void OnActivity(object? sender, RoutedEventArgs e)
    {
        if (_inactivityTimer.Interval == TimeSpan.Zero || !_inactivityTimer.IsEnabled)
            return;

        _inactivityTimer.Stop();
        _inactivityTimer.Start();
    }

    private void OnInactivityTimeout(object? sender, EventArgs e)
    {
        _inactivityTimer.Stop();
        (DataContext as MainWindowViewModel)?.Lock();
    }
}
