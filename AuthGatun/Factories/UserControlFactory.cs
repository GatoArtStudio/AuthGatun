using System;
using AuthGatun.Core.AvaloniaUI;
using AuthGatun.ViewModels;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace AuthGatun.Factories;

public class UserControlFactory(
    IServiceProvider serviceProvider
) : IUserControlFactory
{
    public TControl Create<TControl, TViewModel>(TViewModel viewModel) where TControl : UserControl where TViewModel : ViewModelBase
    {
        var userControl = serviceProvider.GetRequiredService<TControl>();
        userControl.DataContext = viewModel;
        return userControl;
    }

    public TControl Create<TControl>() where TControl : UserControl
    {
        return serviceProvider.GetRequiredService<TControl>();
    }
}