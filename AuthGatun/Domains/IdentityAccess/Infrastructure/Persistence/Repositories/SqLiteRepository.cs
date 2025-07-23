using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using AuthGatun.Domains.IdentityAccess.Application.Ports.Out;
using AuthGatun.Domains.IdentityAccess.Domain.Aggregate;
using AuthGatun.Domains.IdentityAccess.Domain.Model;
using AuthGatun.Domains.IdentityAccess.Domain.ValueObject;
using AuthGatun.Domains.IdentityAccess.Infrastructure.Persistence.Model;

namespace AuthGatun.Domains.IdentityAccess.Infrastructure.Persistence.Repositories;

public class SqLiteRepository : IUserRepository
{
    private readonly string _dbPath;
    private readonly SQLiteConnection _connection;
    private readonly string _appFolderPathData;
    private readonly ConcurrentDictionary<Guid, User> _data = new ConcurrentDictionary<Guid, User>();

    public SqLiteRepository(RepositoryOptions options)
    {
        if (options is null)
            throw new ArgumentNullException(nameof(options));
        
        if (string.IsNullOrWhiteSpace(options.AppFolderPathData))
            throw new ArgumentException("AppFolderPathData cannot be null or empty.", nameof(options.AppFolderPathData));
        
        _dbPath = Path.Combine(options.AppFolderPathData, "AuthGatun", "authgatun.db");
        _appFolderPathData = options.AppFolderPathData;
        
        // Ensure the directory exists and create the database file if it does not exist
        if (!File.Exists(_dbPath))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Path.Combine(_appFolderPathData, "AuthGatun"))!);
            SQLiteConnection.CreateFile(_dbPath);
        }
        _connection = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
        BuildSchema();
        LoadData();
    }
    
    private void BuildSchema()
    {
        // Implement schema creation logic here
        // This could involve creating tables, indexes, etc.
        if (_connection.State != System.Data.ConnectionState.Open)
            _connection.Open();
        
        using (var command = _connection.CreateCommand())
        {
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS users (
                    id TEXT PRIMARY KEY,
                    user_name TEXT NOT NULL,
                    password TEXT,
                    use_password BOOLEAN NOT NULL CHECK (use_password IN (0, 1))
                );
            ";
            command.ExecuteNonQuery();
        }

        using (var command = _connection.CreateCommand())
        {
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS keys (
                    id TEXT PRIMARY KEY,
                    user_id TEXT NOT NULL,
                    service_name TEXT NOT NULL,
                    secret_key TEXT NOT NULL,
                    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
                );
            ";
            command.ExecuteNonQuery();
        }
    }

    private void LoadData()
    {
        if (_connection.State != System.Data.ConnectionState.Open)
            _connection.Open();
        
        using (var command = _connection.CreateCommand())
        {
            command.CommandText = "SELECT * FROM users";
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    // Load user data into memory or a collection
                    // Example: var user = new User(reader.GetGuid(0), reader.GetString(1), reader.GetString(2), reader.GetBoolean(3));
                    Guid id = Guid.Parse(reader.GetString(0));
                    string userName = reader.GetString(1);
                    string password = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
                    bool usePassword = reader.GetBoolean(3);
                    var user = new User(id, new UserName(userName), new Password(password), usePassword);
                    
                    _data[id] = user;
                }
            }
        }

        using (var command = _connection.CreateCommand())
        {
            command.CommandText = "SELECT * FROM keys";
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    // Load key data into memory or a collection
                    // Example: var key = new Key(reader.GetGuid(0), reader.GetGuid(1), reader.GetString(2), reader.GetString(3));
                    Guid id = Guid.Parse(reader.GetString(0));
                    Guid userId = Guid.Parse(reader.GetString(1));
                    string serviceName = reader.GetString(2);
                    string secretKey = reader.GetString(3);
                    
                    if (_data.TryGetValue(userId, out var user))
                    {
                        var key = new Key(id, userId, serviceName, secretKey);
                        user.AddKey(key);
                    }
                }
            }
        }
    }

    public User? Read([NotNull] Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentNullException(nameof(id));
        
        return _data.ContainsKey(id) ? _data[id] : null;
    }

    public bool Delete([NotNull] Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentNullException(nameof(id));

        if (_connection.State != System.Data.ConnectionState.Open)
            _connection.Open();

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = "DELETE FROM users WHERE id = @id";
        cmd.Parameters.AddWithValue("@id", id.ToString());

        int affected = cmd.ExecuteNonQuery();
    
        _data.TryRemove(id, out _);
        return affected > 0;
    }

    public bool Save([NotNull] User entity)
    {
        if (entity is null)
            throw new ArgumentNullException(nameof(entity));
        
        if (_connection.State != System.Data.ConnectionState.Open)
            _connection.Open();
        
        using var transaction = _connection.BeginTransaction();

        try
        {
            // UPSERT user
            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = @"
                    INSERT INTO users (id, user_name, password, use_password)
                    VALUES (@id, @username, @password, @usePassword)
                    ON CONFLICT(id) DO UPDATE SET
                        user_name = excluded.user_name,
                        password = excluded.password,
                        use_password = excluded.use_password;
                ";
                cmd.Parameters.AddWithValue("@id", entity.Id.ToString());
                cmd.Parameters.AddWithValue("@username", entity.Username.Value);
                cmd.Parameters.AddWithValue("@password", entity.Password.Value ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@usePassword", entity.UsePassword);
                cmd.ExecuteNonQuery();
            }

            // Clear existing keys
            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = "DELETE FROM keys WHERE user_id = @userId";
                cmd.Parameters.AddWithValue("@userId", entity.Id.ToString());
                cmd.ExecuteNonQuery();
            }

            // Insert keys
            foreach (var key in entity.Keys)
            {
                using var cmd = _connection.CreateCommand();
                cmd.CommandText = @"
                    INSERT INTO keys (id, user_id, service_name, secret_key)
                    VALUES (@id, @userId, @serviceName, @secretKey);
                ";
                cmd.Parameters.AddWithValue("@id", key.Id.ToString());
                cmd.Parameters.AddWithValue("@userId", key.UserId.ToString());
                cmd.Parameters.AddWithValue("@serviceName", key.ServiceName);
                cmd.Parameters.AddWithValue("@secretKey", key.SecretKey);
                cmd.ExecuteNonQuery();
            }

            transaction.Commit();

            _data[entity.Id] = entity;
            return true;
        }
        catch
        {
            transaction.Rollback();
            return false;
        }
    }

    public List<User> ReadAll()
    {
        return new List<User>(_data.Values);
    }

    public User? FromName(string userName)
    {
        return _data
            .Values
            .FirstOrDefault(usr => usr.Username.Value.Equals(userName));
    }
}