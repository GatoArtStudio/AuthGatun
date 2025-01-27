using AuthGatun.Helpers;
using AuthGatun.ViewModels;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace AuthGatun.Views;

public partial class HomeView : UserControl
{
    public TotpManager totpManager;
    public Notify notify = new Notify();
    private readonly MainWindow _mainWindow; 
    
    public HomeView(MainWindow mainWindow)
    {
        InitializeComponent();
        _mainWindow = mainWindow;
        DataContext = new HomeViewModel();
        var homeViewModel = (HomeViewModel)DataContext;
        var serviceKeys = homeViewModel.GetServiceKeys();
        totpManager = new TotpManager(serviceKeys);
    }
    // este metodo se ejecutando cuando se hace click en el boton de una clave
    private async void CopyToClipboard(object sender, RoutedEventArgs args)
    {
        var clipboard = ClipboardClass.Get();
        var button = sender as Button;
        if (clipboard != null && button != null){
            string serviceName = button.Content?.ToString() ?? string.Empty;
            string secretKey = button.Tag?.ToString() ?? string.Empty;
            string code = totpManager.GetTotpCode(secretKey);
            notify.SendNotify(_mainWindow, "Codigo copiado con exito.", "Codigo");
            await clipboard.SetTextAsync(code);
        }
    }
}