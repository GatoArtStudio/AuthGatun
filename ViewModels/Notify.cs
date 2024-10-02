using Avalonia.Controls.Notifications;

namespace AuthGatun.ViewModels;


public class Notify
{
    public void SendNotify(Views.MainWindow mainWindow, string message , string title = "AuthGatun")
    {
        var notify = new Notification(title: title, message: message);
        var Manager = new Avalonia.Controls.Notifications.WindowNotificationManager(mainWindow);
        if (mainWindow.IsActive) {
            Manager.Show(notify);
        }

    }
}