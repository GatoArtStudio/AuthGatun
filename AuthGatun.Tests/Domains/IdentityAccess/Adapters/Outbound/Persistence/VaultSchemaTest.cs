using System;
using AuthGatun.Domains.IdentityAccess.Adapters.Outbound.Persistence;
using Microsoft.Data.Sqlite;
using Xunit;

namespace AuthGatun.Tests.Domains.IdentityAccess.Adapters.Outbound.Persistence;

public class VaultSchemaTest : IDisposable
{
    private readonly SqliteConnection _connection;

    public VaultSchemaTest()
    {
        SQLitePCL.Batteries_V2.Init();
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
    }

    public void Dispose() => _connection.Dispose();

    private void Execute(string sql)
    {
        using var command = _connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    private T Scalar<T>(string sql)
    {
        using var command = _connection.CreateCommand();
        command.CommandText = sql;
        return (T)Convert.ChangeType(command.ExecuteScalar()!, typeof(T));
    }

    private void CreateOriginalSchema() => Execute(@"
        CREATE TABLE users (id TEXT PRIMARY KEY, user_name TEXT NOT NULL, password TEXT, use_password BOOLEAN NOT NULL);
        CREATE TABLE keys (
            id TEXT PRIMARY KEY, user_id TEXT NOT NULL, service_name TEXT NOT NULL, secret_key TEXT NOT NULL,
            FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE);
        INSERT INTO users VALUES ('u1', 'gato', 'hash', 1);
        INSERT INTO keys VALUES ('k1', 'u1', 'github', 'JBSWY3DPEHPK3PXP');");

    [Fact]
    public void OriginalVaultIsUpgradedKeepingItsKeys()
    {
        CreateOriginalSchema();

        VaultSchema.Migrate(_connection);

        Assert.Equal(VaultSchema.LatestVersion, Scalar<int>("PRAGMA user_version"));
        Assert.Equal("github", Scalar<string>("SELECT service_name FROM keys WHERE id = 'k1'"));
        Assert.Equal("SHA1", Scalar<string>("SELECT algorithm FROM keys WHERE id = 'k1'"));
        Assert.Equal(6, Scalar<int>("SELECT digits FROM keys WHERE id = 'k1'"));
        Assert.Equal(30, Scalar<int>("SELECT period FROM keys WHERE id = 'k1'"));
        Assert.Equal(0, Scalar<int>("SELECT is_favorite FROM keys WHERE id = 'k1'"));
        Assert.Equal(0, Scalar<int>("SELECT sort_order FROM keys WHERE id = 'k1'"));
    }

    [Fact]
    public void MigratingTwiceChangesNothing()
    {
        CreateOriginalSchema();

        VaultSchema.Migrate(_connection);
        VaultSchema.Migrate(_connection);

        Assert.Equal(VaultSchema.LatestVersion, Scalar<int>("PRAGMA user_version"));
        Assert.Equal(1, Scalar<int>("SELECT COUNT(*) FROM keys"));
    }

    [Fact]
    public void VaultWithoutAVersionIsTreatedAsTheOriginalSchema()
    {
        CreateOriginalSchema();

        Assert.Equal(1, VaultSchema.ReadVersion(_connection));
    }
}
