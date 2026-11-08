using System;
using AuthGatun.Core.AvaloniaUI;
using AuthGatun.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace AuthGatun.Factories;

public class ViewModelFactory(
    IServiceProvider serviceProvider
) : IViewModelFactory
{
    public T Create<T>() where T : ViewModelBase
    {
        return serviceProvider.GetRequiredService<T>();
    }
}