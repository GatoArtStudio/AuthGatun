using Avalonia.Controls;

namespace AuthGatun.Core.AvaloniaUI;

public interface IBackgroundModeService
{
    bool IsEnabled { get; }
    bool IsHidden { get; }
    void Attach(Window window);
    void Apply();
    void ShowCurrent();
    void Exit();
}
