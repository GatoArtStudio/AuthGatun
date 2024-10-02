using Avalonia.Controls;
using System.Data.SQLite;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Security.Authentication.ExtendedProtection;
using System;
using System.Windows;
using Avalonia;
using Avalonia.Input.Platform;
using Avalonia.Input;
using Avalonia.Interactivity;
using App.Helpers;
using System.Threading.Tasks;

namespace AuthGatun.ViewModels;
public partial class MainWindowViewModel : ViewModelBase
{
    public ObservableCollection<ServiceKey> ServiceKeys { get; set; }

    public MainWindowViewModel()
    {
        ServiceKeys = new ObservableCollection<ServiceKey>();
        LoadDataFromDatabase();
    }
    private void LoadDataFromDatabase()
    {
        string connectionString = "Data Source=authgatun.db";
        using (SQLiteConnection connection = new SQLiteConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT * FROM secrets";
            using (SQLiteCommand command = new SQLiteCommand(query, connection))
            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var id = reader.GetInt32(0);
                    var userName = reader.GetString(1);
                    var serviceName = reader.GetString(2);
                    var secretKey = reader.GetString(3);
                    ServiceKeys.Add(new ServiceKey
                    {
                        Id = id,
                        UserName = userName,
                        ServiceName = serviceName,
                        SecretKey = secretKey
                    });
                }
            }
        }
    }

    public ObservableCollection<ServiceKey> GetServiceKeys()
    {
        return ServiceKeys;
    }
}
