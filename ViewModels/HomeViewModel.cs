using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;
using System.Windows.Input;
using AuthGatun.Core;
using AuthGatun.Helpers;
using AuthGatun.Models;
using AuthGatun.Services;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using ReactiveUI;

namespace AuthGatun.ViewModels;

public partial class HomeViewModel : ReactiveObject
{
    public ObservableCollection<ServiceKey> ServiceKeys { get; set; }
    private readonly DatabaseService _databaseService;
    private TotpManager _totpManager;

    private NotifyManager _notifyManager = NotifyManager.GetInstance();

    public ReactiveCommand<string, Unit> CopyToClipboardCodeCommand { get; }

    public HomeViewModel()
    {
        _databaseService = new DatabaseService();
        ServiceKeys = _databaseService.LoadDataFromDatabase();
        
        ObservableCollection<ServiceKey> serviceKeys = GetServiceKeys();
        _totpManager = new TotpManager(serviceKeys);

        CopyToClipboardCodeCommand = ReactiveCommand.CreateFromTask<string>(OnCopyToClipboardCodeCommand);
    }

    // Obtenemos las claves de los servicios TOTP
    public ObservableCollection<ServiceKey> GetServiceKeys()
    {
        return ServiceKeys;
    }

    // Callback que es llamado cuando se hace clic en el boton de la clave
    private async Task OnCopyToClipboardCodeCommand(string secretKey)
    {
        IClipboard clipboard = ClipboardClass.Get();
        
        if (clipboard != null)
        {
            string code = _totpManager.GetTotpCode(secretKey);
        
            _notifyManager.SendNotifyInWindow($"Código copiado con éxito, código: {code}", title: "Gestor de claves TOTP");
            await clipboard.SetTextAsync(code);
        }
    }
}