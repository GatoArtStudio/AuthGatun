using AuthGatun.ViewModels;
using Avalonia.Controls;
using Avalonia.Input.Platform;

namespace AuthGatun.Core.AvaloniaUI;

public interface IWindowService
{
    void Register(ViewModelBase viewModel, Window window);
    void Unregister(ViewModelBase viewModel);
    void Close(ViewModelBase viewModel);
    Window? CurrentWindow();
    IClipboard? GetClipboard();
}