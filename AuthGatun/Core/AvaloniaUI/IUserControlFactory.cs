using AuthGatun.ViewModels;
using Avalonia.Controls;

namespace AuthGatun.Core.AvaloniaUI;

public interface IUserControlFactory
{
    TControl Create<TControl, TViewModel>(TViewModel viewModel)
        where TControl : UserControl
        where TViewModel : ViewModelBase;
    
    TControl Create<TControl>()
        where TControl : UserControl;
}