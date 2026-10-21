using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using AuthGatun.Domains.IdentityAccess.Application.Ports.Outbound;
using Konscious.Security.Cryptography;
using Microsoft.Data.Sqlite;

namespace AuthGatun.Domains.IdentityAccess.Adapters.Outbound.Persistence;

public sealed class SqlCipherVault : IVaultSession, IDisposable
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 4;
    private const int MemorySize = 65536;
    private const int DegreeOfParallelism = 1;

    private readonly string _vaultsFolder;
    private readonly LegacyDatabaseMigrator _legacyMigrator;
    private SqliteConnection? _connection;

    static SqlCipherVault()
    {
        SQLitePCL.Batteries_V2.Init();
    }

    public SqlCipherVault(RepositoryOptions options, LegacyDatabaseMigrator legacyMigrator)
    {
        if (string.IsNullOrWhiteSpace(options.AppFolderPathData))
            throw new ArgumentException("AppFolderPathData cannot be null or empty.", nameof(options));

        _vaultsFolder = Path.Combine(options.AppFolderPathData, "AuthGatun", "vaults");
        _legacyMigrator = legacyMigrator;
    }

    public bool IsOpen => _connection is not null;

    public SqliteConnection Connection =>
        _connection ?? throw new InvalidOperationException("The vault is locked.");

    public bool Exists(string userName) =>
        File.Exists(DatabasePath(userName)) || _legacyMigrator.HasUser(userName);

    public void Create(string userName, string password)
    {
        if (Exists(userName))
            throw new InvalidOperationException("The vault already exists.");

        CreateVault(userName, password);
    }

    public bool Open(string userName, string password)
    {
        Close();

        var databasePath = DatabasePath(userName);
        if (!File.Exists(databasePath))
            return MigrateLegacyUser(userName, password);

        var saltPath = SaltPath(userName);
        if (!File.Exists(saltPath))
            return false;

        var connection = OpenWithPassword(databasePath, password, File.ReadAllBytes(saltPath));
        if (connection is null)
            return false;

        _connection = connection;
        return true;
    }

    public void Close()
    {
        _connection?.Dispose();
        _connection = null;
    }

    public void Dispose() => Close();

    private bool MigrateLegacyUser(string userName, string password)
    {
        var legacyUser = _legacyMigrator.FindVerified(userName, password);
        if (legacyUser is null)
            return false;

        CreateVault(userName, password);

        using (var transaction = Connection.BeginTransaction())
        {
            using (var command = Connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = @"
                    INSERT INTO users (id, user_name, password, use_password)
                    VALUES (@id, @username, @password, @usePassword)";
                command.Parameters.AddWithValue("@id", legacyUser.Id.ToString());
                command.Parameters.AddWithValue("@username", legacyUser.UserName);
                command.Parameters.AddWithValue("@password", legacyUser.PasswordHash);
                command.Parameters.AddWithValue("@usePassword", legacyUser.UsePassword);
                command.ExecuteNonQuery();
            }

            foreach (var key in legacyUser.Keys)
            {
                using var command = Connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = @"
                    INSERT INTO keys (id, user_id, service_name, secret_key)
                    VALUES (@id, @userId, @serviceName, @secretKey)";
                command.Parameters.AddWithValue("@id", key.Id.ToString());
                command.Parameters.AddWithValue("@userId", legacyUser.Id.ToString());
                command.Parameters.AddWithValue("@serviceName", key.ServiceName);
                command.Parameters.AddWithValue("@secretKey", key.SecretKey);
                command.ExecuteNonQuery();
            }

            transaction.Commit();
        }

        _legacyMigrator.Remove(legacyUser.Id);
        return true;
    }

    private void CreateVault(string userName, string password)
    {
        Directory.CreateDirectory(_vaultsFolder);

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        File.WriteAllBytes(SaltPath(userName), salt);

        var connection = OpenWithPassword(DatabasePath(userName), password, salt)
                         ?? throw new InvalidOperationException("The vault could not be created.");

        using (var command = connection.CreateCommand())
        {
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS users (
                    id TEXT PRIMARY KEY,
                    user_name TEXT NOT NULL,
                    password TEXT,
                    use_password BOOLEAN NOT NULL CHECK (use_password IN (0, 1))
                );
                CREATE TABLE IF NOT EXISTS keys (
                    id TEXT PRIMARY KEY,
                    user_id TEXT NOT NULL,
                    service_name TEXT NOT NULL,
                    secret_key TEXT NOT NULL,
                    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
                );";
            command.ExecuteNonQuery();
        }

        _connection = connection;
    }

    private static SqliteConnection? OpenWithPassword(string databasePath, string password, byte[] salt)
    {
        var key = DeriveKey(password, salt);
        var connection = new SqliteConnection($"Data Source={databasePath};Mode=ReadWriteCreate;Pooling=False");

        try
        {
            connection.Open();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = $"PRAGMA key = \"x'{Convert.ToHexString(key)}'\"";
                command.ExecuteNonQuery();
            }

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT COUNT(*) FROM sqlite_master";
                command.ExecuteScalar();
            }

            return connection;
        }
        catch (SqliteException)
        {
            connection.Dispose();
            return null;
        }
        finally
        {
            CryptographicOperations.ZeroMemory(key);
        }
    }

    private static byte[] DeriveKey(string password, byte[] salt)
    {
        var passwordBytes = Encoding.UTF8.GetBytes(password);
        try
        {
            var argon2 = new Argon2id(passwordBytes)
            {
                Salt = salt,
                DegreeOfParallelism = DegreeOfParallelism,
                MemorySize = MemorySize,
                Iterations = Iterations
            };
            return argon2.GetBytes(KeySize);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(passwordBytes);
        }
    }

    private string DatabasePath(string userName) => Path.Combine(_vaultsFolder, VaultId(userName) + ".db");

    private string SaltPath(string userName) => Path.Combine(_vaultsFolder, VaultId(userName) + ".salt");

    private static string VaultId(string userName) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(userName)))[..32].ToLowerInvariant();
}
