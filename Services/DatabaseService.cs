using System;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.IO;
using AuthGatun.Models;

namespace AuthGatun.Services;

public class DatabaseService
{
    private readonly string _connectionString;

    public DatabaseService()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var dbPath = Path.Combine(appDataPath, "AuthGatun", "authgatun.db");
        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
        _connectionString = $"Data Source={dbPath}";
    }
    
    public bool InitializeDatabase()
    {
        return false;
    }
    
    public bool InsertDataIntoDatabase()
    {
        return false;
    }
    
    public ObservableCollection<ServiceKey> LoadDataFromDatabase()
    {
        var serviceKeys = new ObservableCollection<ServiceKey>();

        using (SQLiteConnection connection = new SQLiteConnection(_connectionString))
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
                    serviceKeys.Add(new ServiceKey
                    {
                        Id = id,
                        UserName = userName,
                        ServiceName = serviceName,
                        SecretKey = secretKey
                    });
                }
            }
        }

        return serviceKeys;
    }
}