using System;
using AuthGatun.Core.AvaloniaUI;
using AuthGatun.Services;
using AuthGatun.ViewModels;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace AuthGatun.Factories;

public class WindowFactory(
    IServiceProvider serviceProvider,
    IWindowService windowService,
    ScreenCaptureService screenCapture,
    IBackgroundModeService backgroundMode
) : IWindowFactory
{
    public TWindow Create<TWindow, TViewModel>(TViewModel viewModel) where TWindow : Window where TViewModel : ViewModelBase
    {
        var window = serviceProvider.GetRequiredService<TWindow>();
        window.DataContext = viewModel;
        
        windowService.Register(viewModel, window);
        window.Closed += (_, _) => windowService.Unregister(viewModel);
        
        screenCapture.Apply(window);
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