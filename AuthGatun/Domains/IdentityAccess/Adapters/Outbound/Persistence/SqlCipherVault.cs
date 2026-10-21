using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using AuthGatun.Domains.IdentityAccess.Application.Ports.Outbound;
using Konscious.Security.Cryptography;
using Microsoft.Data.Sqlite;
using OtpNet;

namespace AuthGatun.Domains.IdentityAccess.Adapters.Outbound.Persistence;

public sealed class SqlCipherVault : IVaultSession, IDisposable
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int NonceSize = 12;
    private const int TagSize = 16;
    private const int Iterations = 4;
    private const int MemorySize = 65536;
    private const int DegreeOfParallelism = 1;
    private static readonly byte[] HeaderMagic = "AGV1"u8.ToArray();

    private readonly string _vaultsFolder;
    private readonly LegacyDatabaseMigrator _legacyMigrator;
    private SqliteConnection? _connection;
    private VaultHeader? _header;
    private byte[]? _dataKey;
    private string? _userName;

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

    public string Create(string userName, string password)
    {
        if (Exists(userName))
            throw new InvalidOperationException("The vault already exists.");

        return CreateVault(userName, password);
    }

    public bool Open(string userName, string password)
    {
        Close();

        if (!File.Exists(DatabasePath(userName)))
            return MigrateLegacyUser(userName, password);

        var header = ReadHeader(userName);
        if (header is null)
            return false;

        var passwordKey = DeriveKey(password, header.PasswordSalt);
        try
        {
            var dataKey = Unwrap(passwordKey, header.PasswordWrap);
            return dataKey is not null && OpenSession(userName, header, dataKey);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(passwordKey);
        }
    }

    public bool OpenWithRecoveryCode(string userName, string recoveryCode)
    {
        Close();

        if (!File.Exists(DatabasePath(userName)))
            return false;

        var header = ReadHeader(userName);
        if (header?.RecoveryWrap is null)
            return false;

        var recoveryKey = ParseRecoveryCode(recoveryCode);
        if (recoveryKey is null)
            return false;

        try
        {
            var dataKey = Unwrap(recoveryKey, header.RecoveryWrap);
            return dataKey is not null && OpenSession(userName, header, dataKey);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(recoveryKey);
        }
    }

    public void ChangePassword(string newPassword)
    {
        var (header, dataKey, userName) = RequireSession();

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var passwordKey = DeriveKey(newPassword, salt);
        try
        {
            _header = header with { PasswordSalt = salt, PasswordWrap = Wrap(passwordKey, dataKey) };
            WriteHeader(userName, _header);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(passwordKey);
        }
    }

    public string RegenerateRecoveryCode()
    {
        var (header, dataKey, userName) = RequireSession();

        var recoveryKey = RandomNumberGenerator.GetBytes(KeySize);
        try
        {
            _header = header with { RecoveryWrap = Wrap(recoveryKey, dataKey) };
            WriteHeader(userName, _header);
            return FormatRecoveryCode(recoveryKey);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(recoveryKey);
        }
    }

    public void Close()
    {
        _connection?.Dispose();
        _connection = null;

        if (_dataKey is not null)
            CryptographicOperations.ZeroMemory(_dataKey);

        _dataKey = null;
        _header = null;
        _userName = null;
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

    private string CreateVault(string userName, string password)
    {
        Directory.CreateDirectory(_vaultsFolder);

        var dataKey = RandomNumberGenerator.GetBytes(KeySize);
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var recoveryKey = RandomNumberGenerator.GetBytes(KeySize);
        var passwordKey = DeriveKey(password, salt);

        try
        {
            var header = new VaultHeader(salt, Wrap(passwordKey, dataKey), Wrap(recoveryKey, dataKey));
            WriteHeader(userName, header);

            var connection = OpenDatabase(DatabasePath(userName), dataKey)
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
            _header = header;
            _dataKey = dataKey;
            _userName = userName;

            return FormatRecoveryCode(recoveryKey);
        }
        catch
        {
            CryptographicOperations.ZeroMemory(dataKey);
            throw;
        }
        finally
        {
            CryptographicOperations.ZeroMemory(passwordKey);
            CryptographicOperations.ZeroMemory(recoveryKey);
        }
    }

    private bool OpenSession(string userName, VaultHeader header, byte[] dataKey)
    {
        var connection = OpenDatabase(DatabasePath(userName), dataKey);
        if (connection is null)
        {
            CryptographicOperations.ZeroMemory(dataKey);
            return false;
        }

        _connection = connection;
        _header = header;
        _dataKey = dataKey;
        _userName = userName;
        return true;
    }

    private (VaultHeader Header, byte[] DataKey, string UserName) RequireSession()
    {
        if (_connection is null || _header is null || _dataKey is null || _userName is null)
            throw new InvalidOperationException("The vault is locked.");

        return (_header, _dataKey, _userName);
    }

    private static SqliteConnection? OpenDatabase(string databasePath, byte[] dataKey)
    {
        var connection = new SqliteConnection($"Data Source={databasePath};Mode=ReadWriteCreate;Pooling=False");

        try
        {
            connection.Open();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = $"PRAGMA key = \"x'{Convert.ToHexString(dataKey)}'\"";
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

    private static WrappedKey Wrap(byte[] keyEncryptionKey, byte[] dataKey)
    {
        var nonce = RandomNumberGenerator.GetBytes(NonceSize);
        var cipher = new byte[dataKey.Length];
        var tag = new byte[TagSize];

        using var aes = new AesGcm(keyEncryptionKey, TagSize);
        aes.Encrypt(nonce, dataKey, cipher, tag);
        return new WrappedKey(nonce, tag, cipher);
    }

    private static byte[]? Unwrap(byte[] keyEncryptionKey, WrappedKey wrapped)
    {
        var dataKey = new byte[wrapped.Cipher.Length];
        try
        {
            using var aes = new AesGcm(keyEncryptionKey, TagSize);
            aes.Decrypt(wrapped.Nonce, wrapped.Cipher, wrapped.Tag, dataKey);
            return dataKey;
        }
        catch (CryptographicException)
        {
            CryptographicOperations.ZeroMemory(dataKey);
            return null;
        }
    }

    private static string FormatRecoveryCode(byte[] recoveryKey)
    {
        var encoded = Base32Encoding.ToString(recoveryKey).TrimEnd('=');
        var groups = new string[(encoded.Length + 3) / 4];
        for (var i = 0; i < groups.Length; i++)
            groups[i] = encoded.Substring(i * 4, Math.Min(4, encoded.Length - i * 4));

        return string.Join('-', groups);
    }

    private static byte[]? ParseRecoveryCode(string recoveryCode)
    {
        try
        {
            var normalized = recoveryCode.Replace("-", "").Replace(" ", "").ToUpperInvariant();
            var key = Base32Encoding.ToBytes(normalized);
            return key.Length == KeySize ? key : null;
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    private VaultHeader? ReadHeader(string userName)
    {
        var path = HeaderPath(userName);
        if (!File.Exists(path))
            return null;

        try
        {
            using var reader = new BinaryReader(File.OpenRead(path));
            if (!reader.ReadBytes(HeaderMagic.Length).AsSpan().SequenceEqual(HeaderMagic))
                return null;

            var salt = reader.ReadBytes(SaltSize);
            var passwordWrap = ReadWrapped(reader);
            var recoveryWrap = reader.ReadBoolean() ? ReadWrapped(reader) : null;
            return new VaultHeader(salt, passwordWrap, recoveryWrap);
        }
        catch (EndOfStreamException)
        {
            return null;
        }
    }

    private void WriteHeader(string userName, VaultHeader header)
    {
        var path = HeaderPath(userName);
        var temporaryPath = path + ".tmp";

        using (var writer = new BinaryWriter(File.Create(temporaryPath)))
        {
            writer.Write(HeaderMagic);
            writer.Write(header.PasswordSalt);
            WriteWrapped(writer, header.PasswordWrap);
            writer.Write(header.RecoveryWrap is not null);
            if (header.RecoveryWrap is not null)
                WriteWrapped(writer, header.RecoveryWrap);
            writer.Flush();
            ((FileStream)writer.BaseStream).Flush(true);
        }

        File.Move(temporaryPath, path, true);
    }

    private static WrappedKey ReadWrapped(BinaryReader reader) =>
        new(reader.ReadBytes(NonceSize), reader.ReadBytes(TagSize), reader.ReadBytes(KeySize));

    private static void WriteWrapped(BinaryWriter writer, WrappedKey wrapped)
    {
        writer.Write(wrapped.Nonce);
        writer.Write(wrapped.Tag);
        writer.Write(wrapped.Cipher);
    }

    private string DatabasePath(string userName) => Path.Combine(_vaultsFolder, VaultId(userName) + ".db");

    private string HeaderPath(string userName) => Path.Combine(_vaultsFolder, VaultId(userName) + ".vault");

    private static string VaultId(string userName) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(userName)))[..32].ToLowerInvariant();

    private sealed record WrappedKey(byte[] Nonce, byte[] Tag, byte[] Cipher);

    private sealed record VaultHeader(byte[] PasswordSalt, WrappedKey PasswordWrap, WrappedKey? RecoveryWrap);
}
