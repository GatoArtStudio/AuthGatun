using System;
using System.IO;
using System.Linq;
using AuthGatun.Domains.IdentityAccess.Adapters.Outbound.Persistence;
using AuthGatun.Domains.IdentityAccess.Adapters.Outbound.Security;
using AuthGatun.Domains.IdentityAccess.Domain.Aggregate;
using AuthGatun.Domains.IdentityAccess.Domain.Model;
using AuthGatun.Domains.IdentityAccess.Domain.ValueObject;
using Microsoft.Data.Sqlite;
using Xunit;

namespace AuthGatun.Tests.Domains.IdentityAccess.Adapters.Outbound.Persistence;

public class SqlCipherVaultTest : IDisposable
{
    private const string Password = "Password-testing";

    private readonly string _appFolder = Path.Combine(Path.GetTempPath(), "authgatun-tests-" + Guid.NewGuid());
    private readonly RepositoryOptions _options;
    private readonly Argon2PasswordHasher _hasher = new();

    public SqlCipherVaultTest()
    {
        _options = new RepositoryOptions(TypeRepository.SqLite, _appFolder, null, null, null, null, null);
    }

    private SqlCipherVault NewVault() => new(_options, new LegacyDatabaseMigrator(_options, _hasher));

    private static User NewUser(string name) => new(Guid.NewGuid(), new UserName(name), new Password("hashed-password"), true);

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        if (Directory.Exists(_appFolder))
            Directory.Delete(_appFolder, true);
    }

    [Fact]
    public void CreatedVaultOpensOnlyWithTheRightPassword()
    {
        using (var vault = NewVault())
        {
            vault.Create("gato", Password);
            new SqLiteRepository(vault).Save(NewUser("gato"));
        }

        using var reopened = NewVault();
        Assert.False(reopened.Open("gato", "wrong-password"));
        Assert.False(reopened.IsOpen);
        Assert.True(reopened.Open("gato", Password));
        Assert.NotNull(new SqLiteRepository(reopened).FromName("gato"));
    }

    [Fact]
    public void VaultFileDoesNotContainPlainText()
    {
        using (var vault = NewVault())
        {
            vault.Create("gato", Password);
            var user = NewUser("gato");
            user.AddKey(new Key(Guid.NewGuid(), user.Id, "github-service", "JBSWY3DPEHPK3PXP"));
            new SqLiteRepository(vault).Save(user);
        }

        var bytes = File.ReadAllBytes(Directory.GetFiles(Path.Combine(_appFolder, "AuthGatun", "vaults"), "*.db").Single());
        var text = System.Text.Encoding.Latin1.GetString(bytes);

        Assert.DoesNotContain("SQLite format", text);
        Assert.DoesNotContain("github-service", text);
        Assert.DoesNotContain("JBSWY3DPEHPK3PXP", text);
    }

    [Fact]
    public void LegacyPlainDatabaseIsMigratedOnLogin()
    {
        SQLitePCL.Batteries_V2.Init();
        var legacyPath = Path.Combine(_appFolder, "AuthGatun", "authgatun.db");
        Directory.CreateDirectory(Path.GetDirectoryName(legacyPath)!);
        var userId = Guid.NewGuid();
        var keyId = Guid.NewGuid();

        using (var connection = new SqliteConnection($"Data Source={legacyPath};Pooling=False"))
        {
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE users (id TEXT PRIMARY KEY, user_name TEXT NOT NULL, password TEXT, use_password BOOLEAN NOT NULL);
                CREATE TABLE keys (id TEXT PRIMARY KEY, user_id TEXT NOT NULL, service_name TEXT NOT NULL, secret_key TEXT NOT NULL);";
            command.ExecuteNonQuery();
            command.CommandText = "INSERT INTO users VALUES (@id, 'gato', @hash, 1); INSERT INTO keys VALUES (@keyId, @id, 'github', 'SECRET');";
            command.Parameters.AddWithValue("@id", userId.ToString());
            command.Parameters.AddWithValue("@keyId", keyId.ToString());
            command.Parameters.AddWithValue("@hash", _hasher.Hash(Password));
            command.ExecuteNonQuery();
        }

        using var vault = NewVault();
        Assert.True(vault.Exists("gato"));
        Assert.False(vault.Open("gato", "wrong-password"));
        Assert.True(File.Exists(legacyPath));

        Assert.True(vault.Open("gato", Password));

        var user = new SqLiteRepository(vault).FromName("gato");
        Assert.NotNull(user);
        Assert.Equal(userId, user.Id);
        Assert.Equal("SECRET", Assert.Single(user.Keys).SecretKey);
        Assert.False(File.Exists(legacyPath));
    }

    [Fact]
    public void ChangedPasswordReplacesTheOldOneAndKeepsTheData()
    {
        using (var vault = NewVault())
        {
            vault.Create("gato", Password);
            new SqLiteRepository(vault).Save(NewUser("gato"));
            vault.ChangePassword("New-password-1");
        }

        using var reopened = NewVault();
        Assert.False(reopened.Open("gato", Password));
        Assert.True(reopened.Open("gato", "New-password-1"));
        Assert.NotNull(new SqLiteRepository(reopened).FromName("gato"));
    }

    [Fact]
    public void RecoveryCodeOpensTheVaultUntilItIsRegenerated()
    {
        string firstCode;
        string secondCode;
        using (var vault = NewVault())
        {
            firstCode = vault.Create("gato", Password);
            new SqLiteRepository(vault).Save(NewUser("gato"));
            secondCode = vault.RegenerateRecoveryCode();
        }

        using var reopened = NewVault();
        Assert.False(reopened.OpenWithRecoveryCode("gato", "not-a-code"));
        Assert.False(reopened.OpenWithRecoveryCode("gato", firstCode));
        Assert.True(reopened.OpenWithRecoveryCode("gato", secondCode.ToLowerInvariant()));
        Assert.NotNull(new SqLiteRepository(reopened).FromName("gato"));
    }
}
