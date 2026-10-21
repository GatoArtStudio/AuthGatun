using System;
using AuthGatun.Core.AvaloniaUI;
using AuthGatun.Core.Platform;
using AuthGatun.Domains.IdentityAccess;
using AuthGatun.Domains.Settings;
using AuthGatun.Domains.IdentityAccess.Adapters.Outbound.EventBus;
using AuthGatun.Domains.IdentityAccess.Adapters.Outbound.Persistence;
using AuthGatun.Factories;
using AuthGatun.Services;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using AuthGatun.ViewModels;
using AuthGatun.Views;
using Microsoft.Extensions.DependencyInjection;

namespace AuthGatun;

public partial class App : Application
{
    public static IServiceProvider ServiceProvider { get; private set; }
    
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        var services = new ServiceCollection();
        
        services.AddIdentityAccessDomain(
            new RepositoryOptions(
                TypeRepository.SqLite,
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                null,
                null,
                null,
                null,
                null
            ),
            new BusOptions(TypeBus.Default)
        );
        services.AddSettingsDomain(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
        services.AddSingleton<AppSettingsState>();
        services.AddSingleton<DiscordService>();
        services.AddSingleton<IWindowService, WindowService>();
        services.AddSingleton<NotifyManagerService>();
        services.AddSingleton<UserStatusService>();

        if (OperatingSystem.IsWindows())
        {
            services.AddSingleton<IPlatformService, PlatformNativeWindowsService>();
        }
        else
        {
            services.AddSingleton<IPlatformService, PlatformFallbackService>();
        }
        
        // View models, They implement ViewModelBase
        services.AddTransient<LoginWindowViewModel>();
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<RegisterWindowViewModel>();
        services.AddTransient<HomeViewModel>();
        services.AddTransient<KeyViewModel>();
        services.AddTransient<SettingsViewModel>();
        
        // Windows, They implement Window
        services.AddTransient<LoginWindow>();
        services.AddTransient<MainWindow>();
        services.AddTransient<RegisterWindow>();
        
        // User control, They implement UserControl
        services.AddTransient<HomeView>();
        services.AddTransient<KeysView>();
        services.AddTransient<SettingsView>();
        

        services.AddSingleton<IViewModelFactory, ViewModelFactory>();
        services.AddSingleton<IWindowFactory, WindowFactory>();
        services.AddSingleton<IUserControlFactory, UserControlFactory>();
        
        ServiceProvider = services.BuildServiceProvider();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Line below is needed to remove Avalonia data validation.
            // Without this line you will get duplicate validations from both Avalonia and CT
            var viewModelFactory = ServiceProvider.GetRequiredService<IViewModelFactory>();
            var windowFactory = ServiceProvider.GetRequiredService<IWindowFactory>();
            var loginViewModel = viewModelFactory.Create<LoginWindowViewModel>();
            var loginWindow = windowFactory.Create<LoginWindow, LoginWindowViewModel>(loginViewModel);
            var discordService = ServiceProvider.GetRequiredService<DiscordService>();
            
            desktop.MainWindow = loginWindow;
            discordService.UpdatePresence();
        }

        base.OnFrameworkInitializationCompleted();
    }
}