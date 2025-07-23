using AuthGatun.Services;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using AuthGatun.ViewModels;
using AuthGatun.Views;
using Avalonia.Controls;

namespace AuthGatun;

public partial class App : Application
{
    private Window? _window;
    private IClassicDesktopStyleApplicationLifetime? _applicationLifetime;
    
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Line below is needed to remove Avalonia data validation.
            // Without this line you will get duplicate validations from both Avalonia and CT
            BindingPlugins.DataValidators.RemoveAt(0);
            _applicationLifetime = desktop;

            var window = new LoginWindow();
            window.DataContext = new LoginWindowViewModel(this);
            SetWindow(window);
        }

        base.OnFrameworkInitializationCompleted();
    }

    /// <summary>
    /// Set the main application window.
    /// </summary>
    /// <param name="newWindow">Set a new window as the main application window, remembering to set the DataContext before passing the window to this method.</param>
    /// <returns>If the application is already started, the new window is established, otherwise it will return False because the application has not yet started.</returns>
    public bool SetWindow(Window newWindow)
    {
        if (_applicationLifetime is null)
            return false;

        _applicationLifetime.MainWindow = newWindow;
        newWindow.Show();
        
        _window?.Close();
        
        _window = newWindow;
        NotifyManager notifyManager = NotifyManager.GetInstance();
        notifyManager.MainWindow = newWindow;
        return true;
    }

    /// <summary>
    /// Hide the main window if it is not already hidden.
    /// </summary>
    /// <returns>Returns True if the action completes or False if it fails or is already in the state that the method wants to trigger.</returns>
    public bool HideWindow()
    {
        if (_applicationLifetime is null || _window is null)
            return false;
        
        if (!_window.IsVisible)
            return false; // Already hidden
        
        _window.Hide();
        return true;
    }
    
    /// <summary>
    /// Close the main window.
    /// </summary>
    public void CloseWindow()
    {
        if (_applicationLifetime is null || _window is null)
            return;

        _window.Close();
    }
    
    /// <summary>
    /// Show the main window if it is not already visible.
    /// </summary>
    /// <returns>Returns True if the action completes or False if it fails or is already in the state that the method wants to trigger.</returns>
    public bool ShowWindow()
    {
        if (_applicationLifetime is null || _window is null)
            return false;
        
        if (_window.IsVisible)
            return false; // Already visible
        
        _window.Show();
        return true;
    }
}