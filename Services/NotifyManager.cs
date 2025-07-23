using Avalonia.Controls;
using Avalonia.Controls.Notifications;

namespace AuthGatun.Services;

public class NotifyManager
{
    private NotifyManager() { }

    private static NotifyManager? _instance;

    private static readonly object Lock = new object();
    
    public Window? MainWindow { get; set; }

    public static NotifyManager GetInstance()
    {
        if (_instance is null)
        {
            lock (Lock)
            {
                if (_instance is null)
                {
                    _instance = new NotifyManager();
                }
            }
        }

        return _instance;
    }
    

    public void SendNotifyInWindow(string message, string title = "AuthGatun")
    {
        if (MainWindow is null) return;
        
        Notification notify = new Notification(title: title, message: message);
        WindowNotificationManager manager = new WindowNotificationManager(MainWindow);

        if (MainWindow.IsActive)
        {
            manager.Show(notify);
        }
    }

    public void SendNotifyInWindow(string message, string title = "AuthGatun",
        NotificationType type = NotificationType.Information)
    {
        if (MainWindow is null) return;
        
        Notification notify = new Notification(title: title, message: message, type: type);
        WindowNotificationManager manager = new WindowNotificationManager(MainWindow);

        if (MainWindow.IsActive)
        {
            manager.Show(notify);
        }
    }
}