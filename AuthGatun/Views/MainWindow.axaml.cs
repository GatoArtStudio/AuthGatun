using System;
using AuthGatun.ViewModels;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace AuthGatun.Views;

public partial class MainWindow : Window
{
    private static readonly TimeSpan InactivityTimeout = TimeSpan.FromMinutes(5);

    private readonly DispatcherTimer _inactivityTimer = new() { Interval = InactivityTimeout };

    public MainWindow()
    {
        InitializeComponent();

        _inactivityTimer.Tick += OnInactivityTimeout;
        AddHandler(PointerMovedEvent, OnActivity, RoutingStrategies.Tunnel, handledEventsToo: true);
        AddHandler(PointerPressedEvent, OnActivity, RoutingStrategies.Tunnel, handledEventsToo: true);
        AddHandler(KeyDownEvent, OnActivity, RoutingStrategies.Tunnel, handledEventsToo: true);

        Opened += (_, _) => _inactivityTimer.Start();
        Closed += (_, _) => _inactivityTimer.Stop();
    }

    private void OnActivity(object? sender, RoutedEventArgs e)
    {
        _inactivityTimer.Stop();
        _inactivityTimer.Start();
    }

    private void OnInactivityTimeout(object? sender, EventArgs e)
    {
        _inactivityTimer.Stop();
        (DataContext as MainWindowViewModel)?.Lock();
    }
}
