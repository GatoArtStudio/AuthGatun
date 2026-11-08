using AuthGatun.Core.AvaloniaUI;
using Avalonia.Controls.Notifications;

namespace AuthGatun.Services;

public class NotifyManagerService(
    IWindowService windowService
)
{
    public void SendNotifyInWindow(string message, string title = "AuthGatun")
    {
        var window = windowService.CurrentWindow();
        
        if (window is null) return;
        
        Notification notify = new Notification(title: title, message: message);
        WindowNotificationManager manager = new WindowNotificationManager(window);

        if (window.IsActive)
        {
            manager.Show(notify);
        }
    }

    public void SendNotifyInWindow(
        string message,
        string title = "AuthGatun",
        NotificationType type = NotificationType.Information
    )
    {
        var window = windowService.CurrentWindow();
        
        if (window is null) return;
        
        Notification notify = new Notification(title: title, message: message, type: type);
        WindowNotificationManager manager = new WindowNotificationManager(window);

        if (window.IsActive)
        {
            manager.Show(notify);
        }
    }
}