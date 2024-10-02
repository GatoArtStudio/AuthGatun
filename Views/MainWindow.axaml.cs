using Avalonia.Controls;
using AuthGatun.ViewModels;
using Avalonia.Interactivity;
using App.Helpers;

namespace AuthGatun.Views;

public partial class MainWindow : Window
{
    public TotpManager totpManager;
    public Notify notify = new Notify();

    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
        var mainWindowViewModel = (MainWindowViewModel)DataContext;
        var serviceKeys = mainWindowViewModel.GetServiceKeys();
        totpManager = new TotpManager(serviceKeys);
    }
    private async void CopyToClipboard(object sender, RoutedEventArgs args)
    {
        var clipboard = ClipboardClass.Get();
        var button = sender as Button;
        if (clipboard != null && button != null){
            string serviceName = button.Content?.ToString() ?? string.Empty;
            string secretKey = button.Tag?.ToString() ?? string.Empty;
            string code = totpManager.GetTotpCode(secretKey);
            notify.SendNotify(this, "Codigo copiado con exito.", "Codigo");
            await clipboard.SetTextAsync(code);
        }
    }
}