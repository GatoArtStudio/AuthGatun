using System.Collections.ObjectModel;
using AuthGatun.Models;
using AuthGatun.Services;

namespace AuthGatun.ViewModels;

public partial class HomeViewModel : ViewModelBase
{
    public ObservableCollection<ServiceKey> ServiceKeys { get; set; }
    private readonly DatabaseService _databaseService;
    
    public HomeViewModel()
    {
        _databaseService = new DatabaseService();
        ServiceKeys = _databaseService.LoadDataFromDatabase();
    }
    // Obtenemos las claves de los servicios TOTP
    public ObservableCollection<ServiceKey> GetServiceKeys()
    {
        return ServiceKeys;
    }
}