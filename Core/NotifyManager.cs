using AuthGatun.Views;
using Avalonia.Controls.Notifications;

namespace AuthGatun.Core;

public sealed class NotifyManager
{
    public NotifyManager() { }

    public static NotifyManager _instance;

    public static NotifyManager GetInstance()
    {
        if (_instance == null)
        {
            _instance = new NotifyManager
            {
                MainWindow = null
            };
        }

        return _instance;
    }
    
    public required MainWindow MainWindow { get; set; }

    public void SendNotifyInWindow(string message, string title = "AuthGatun")
    {
        if (MainWindow == null) return;
        
        Notification notify = new Notification(title: title, message: message);
        WindowNotificationManager manager = new WindowNotificationManager(MainWindow);

        if (MainWindow.IsActive)
        {
            manager.Show(notify);
        }
    }
     
}