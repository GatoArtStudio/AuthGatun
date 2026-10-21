using System.Collections.Generic;
using System.Linq;
using AuthGatun.Core.AvaloniaUI;
using AuthGatun.ViewModels;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Platform.Storage;

namespace AuthGatun.Services;

public class WindowService : IWindowService
{
    private readonly object _lock = new();
    private readonly List<(ViewModelBase ViewModel, Window Window)> _windows = [];
    private readonly HashSet<Window> _forceClosing = [];

    public void Register(ViewModelBase viewModel, Window window)
    {
        lock (_lock)
        {
            _windows.RemoveAll(entry => entry.ViewModel == viewModel);
            _windows.Add((viewModel, window));
        }
    }

    public void Unregister(ViewModelBase viewModel)
    {
        lock (_lock)
        {
            var removed = _windows.Where(entry => entry.ViewModel == viewModel).Select(entry => entry.Window).ToList();
            _windows.RemoveAll(entry => entry.ViewModel == viewModel);
            foreach (var window in removed)
                _forceClosing.Remove(window);
        }
    }

    public void Close(ViewModelBase viewModel)
    {
        Window? window;
        lock (_lock)
        {
            window = _windows.FirstOrDefault(entry => entry.ViewModel == viewModel).Window;
            if (window is not null)
                _forceClosing.Add(window);
        }

        window?.Close();
    }

    public bool IsForceClosing(Window window)
    {
        lock (_lock)
        {
            return _forceClosing.Contains(window);
        }
    }

    public Window? CurrentWindow()
    {
        lock (_lock)
        {
            // Return the most recently registered window
            return _windows.Count == 0 ? null : _windows[^1].Window;
        }
    }

    public IClipboard? GetClipboard()
    {
        var currentWindow = CurrentWindow();
        return currentWindow?.Clipboard;
    }

    public IStorageProvider? GetStorageProvider()
    {
        return CurrentWindow()?.StorageProvider;
    }
}
