using System;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using AuthGatun.Models;

namespace AuthGatun.Services;

public class DatabaseService
{
    private string _connectionString;
    private string _encryptionKey = "mi_clave_secreta";

    public DatabaseService()
    {
        InitializeDatabase();
    }
    
    public void InitializeDatabase()
    {
        // Obtenemos la ruta de la carpeta de datos de la aplicacion
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        // Creamos la ruta de la base de datos
        var dbPath = Path.Combine(appDataPath, "AuthGatun", "authgatun.db");
        // Verificamos si la base de datos existe
        if (!File.Exists(dbPath))
        {
            // Si no existe el directorio donde debe ir la base de datos, creamos el directorio
            Directory.CreateDirectory(Path.GetDirectoryName(Path.Combine(appDataPath, "AuthGatun"))!);
            // Creamos la base de datos
            SQLiteConnection.CreateFile(dbPath);
            CreateDatabase();
        }
        _connectionString = $"Data Source={dbPath};Version=3;";
    }
    
    public bool CreateDatabase()
    {
        try
        {
            using (SQLiteConnection connection = new SQLiteConnection(_connectionString))
            {
                // Abrimos la conexion
                connection.Open();
                string query = "CREATE TABLE IF NOT EXISTS secrets (id INTEGER PRIMARY KEY, user_name TEXT, service_name TEXT, secret_key TEXT)";
                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
            return true;
        }
        catch (SQLiteException e)
        {
            Console.WriteLine(e);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        return false;
    }
    
    public bool InsertDataIntoDatabase(ServiceKey serviceKey)
    {
        try
        {
            using (SQLiteConnection connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string query = "INSERT INTO secrets (user_name, service_name, secret_key) VALUES (@userName, @serviceName, @secretKey)";
                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@userName", serviceKey.UserName);
                    command.Parameters.AddWithValue("@serviceName", serviceKey.ServiceName);
                    command.Parameters.AddWithValue("@secretKey", serviceKey.SecretKey);
                    command.ExecuteNonQuery();
                }
            }
            return true;
        }
        catch (SQLiteException e)
        {
            Console.WriteLine(e);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        return false;
    }
    
    public ObservableCollection<ServiceKey> LoadDataFromDatabase()
    {
        var serviceKeys = new ObservableCollection<ServiceKey>();

        try
        {
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
        }
        catch (SQLiteException e)
        {
            Console.WriteLine(e);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        return serviceKeys;
    }
    private string Encrypt(string plainText)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes(_encryptionKey);
            aes.IV = new byte[16]; // Vector de inicialización con ceros

            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                using (StreamWriter sw = new StreamWriter(cs))
                {
                    sw.Write(plainText);
                }
                return Convert.ToBase64String(ms.ToArray());
            }
        }
    }

    private string Decrypt(string cipherText)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes(_encryptionKey);
            aes.IV = new byte[16]; // Vector de inicialización con ceros

            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(cipherText)))
            using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
            using (StreamReader sr = new StreamReader(cs))
            {
                return sr.ReadToEnd();
            }
        }
    }
}