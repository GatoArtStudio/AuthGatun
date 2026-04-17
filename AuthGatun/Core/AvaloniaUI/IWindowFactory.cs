using AuthGatun.ViewModels;
using Avalonia.Controls;

namespace AuthGatun.Core.AvaloniaUI;

public interface IWindowFactory
{
    TWindow Create<TWindow, TViewModel>(TViewModel viewModel)
        where TWindow : Window
        where TViewModel : ViewModelBase;
    
    TWindow Create<TWindow>()
        where TWindow : Window;
}