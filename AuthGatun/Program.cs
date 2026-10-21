using Avalonia;
using System;
using AuthGatun.Services;
using Avalonia.ReactiveUI;

namespace AuthGatun;

sealed class Program
{
    
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    internal static SingleInstanceService? SingleInstance { get; private set; }

    [STAThread]
    public static void Main(string[] args)
    {
        using var singleInstance = new SingleInstanceService("AuthGatun-" + Environment.UserName);
        if (!singleInstance.TryAcquire())
        {
            singleInstance.NotifyRunningInstance();
            return;
        }

        SingleInstance = singleInstance;
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .UseReactiveUI()
            .LogToTrace();
}
