using Avalonia;
using System;
using AuthGatun.Domains.IdentityAccess.Infrastructure.Enums;
using AuthGatun.Domains.IdentityAccess.Infrastructure.Eventbus;
using AuthGatun.Domains.IdentityAccess.Infrastructure.Eventbus.Model;
using AuthGatun.Domains.IdentityAccess.Infrastructure.Persistence;
using AuthGatun.Domains.IdentityAccess.Infrastructure.Persistence.Model;
using AuthGatun.Services;
using Avalonia.ReactiveUI;

namespace AuthGatun;

sealed class Program
{
    private static RepositoryFactory? _repositoryFactory;
    private static BusFactory? _busFactory;
    
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        _repositoryFactory = RepositoryFactory.GetInstance(
            new RepositoryOptions(
                TypeRepository.SqLite, // Change this to TypeRepository.SqLite for SQLite));
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), // Use the base directory of the application
                null, // DbHost
                null, // DbPort
                null, // DbDatabase
                null, // DbUser
                null  // DbPassword
            )
        );
        
        _busFactory = BusFactory.GetInstance(
            new BusOptions(TypeBus.Default) // Change this to TypeBus.RabbitMQ for RabbitMQ
        );
        
        // Run the application UI
        RunUi(args);
    }

    private static void RunUi(string[] args)
    {
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .UseReactiveUI()
            .LogToTrace();
}
