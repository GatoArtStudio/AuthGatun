using AuthGatun.ViewModels;

namespace AuthGatun.Core.AvaloniaUI;

public interface IViewModelFactory
{
    T Create<T>() where T : ViewModelBase;
}