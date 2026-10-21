using System;
using AuthGatun.Core.AvaloniaUI;
using AuthGatun.Core.Platform;
using AuthGatun.ViewModels;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace AuthGatun.Factories;

public class WindowFactory(
    IServiceProvider serviceProvider,
    IWindowService windowService,
    IPlatformService platformService,
    IBackgroundModeService backgroundMode
) : IWindowFactory
{
    public TWindow Create<TWindow, TViewModel>(TViewModel viewModel) where TWindow : Window where TViewModel : ViewModelBase
    {
        var window = serviceProvider.GetRequiredService<TWindow>();
        window.DataContext = viewModel;
        
        windowService.Register(viewModel, window);
        window.Closed += (_, _) => windowService.Unregister(viewModel);
        
        // Hide the current window
        platformService.HiddenCurrentWindowByExcludeFromCapture(window);
        backgroundMode.Attach(window);
        
        return window;
    }

    public TWindow Create<TWindow>() where TWindow : Window
    {
        var window = serviceProvider.GetRequiredService<TWindow>();
        backgroundMode.Attach(window);
        return window;
    }
}