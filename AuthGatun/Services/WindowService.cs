using System.Collections.Concurrent;
using System.Linq;
using AuthGatun.Core.AvaloniaUI;
using AuthGatun.ViewModels;
using Avalonia.Controls;
using Avalonia.Input.Platform;

namespace AuthGatun.Services;

public class WindowService : IWindowService
{
    private readonly ConcurrentDictionary<ViewModelBase, Window> _windows = new();

    public void Register(ViewModelBase viewModel, Window window)
    {
        _windows[viewModel] = window;
    }

    public void Unregister(ViewModelBase viewModel)
    {
        _windows.TryRemove(viewModel, out _);
    }

    public void Close(ViewModelBase viewModel)
    {
        if (_windows.TryGetValue(viewModel, out var window))
        {
            window.Close();
        }
    }

    public Window? CurrentWindow()
    {
        if (_windows.IsEmpty)
            return null;

        // Return the most recently registered window
        return _windows.Values.LastOrDefault();
    }

    public IClipboard? GetClipboard()
    {
        var currentWindow = CurrentWindow();
        return currentWindow?.Clipboard;
    }
}