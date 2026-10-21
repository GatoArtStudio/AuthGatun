using AuthGatun.ViewModels;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Platform.Storage;

namespace AuthGatun.Core.AvaloniaUI;

public interface IWindowService
{
    void Register(ViewModelBase viewModel, Window window);
    void Unregister(ViewModelBase viewModel);
    void Close(ViewModelBase viewModel);
    bool IsForceClosing(Window window);
    Window? CurrentWindow();
    IClipboard? GetClipboard();
    IStorageProvider? GetStorageProvider();
}