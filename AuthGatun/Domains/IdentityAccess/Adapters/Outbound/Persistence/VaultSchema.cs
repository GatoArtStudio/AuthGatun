using Microsoft.Data.Sqlite;

namespace AuthGatun.Domains.IdentityAccess.Adapters.Outbound.Persistence;

internal static class VaultSchema
{
    // Version 1 is the original schema (users and keys tables). PRAGMA user_version stays 0 on those vaults.
    private const int BaseVersion = 1;

    // Each entry migrates the schema from the previous version to the one at its index + BaseVersion + 1.
    // Append new migrations at the end; never edit or reorder the existing ones.
    private static readonly string[] Migrations =
    [
        // v2: TOTP parameters per key
        @"ALTER TABLE keys ADD COLUMN algorithm TEXT NOT NULL DEFAULT 'SHA1';
          ALTER TABLE keys ADD COLUMN digits INTEGER NOT NULL DEFAULT 6;
          ALTER TABLE keys ADD COLUMN period INTEGER NOT NULL DEFAULT 30;",

        // v3: favorites and manual order
        @"ALTER TABLE keys ADD COLUMN is_favorite INTEGER NOT NULL DEFAULT 0;
          ALTER TABLE keys ADD COLUMN sort_order INTEGER NOT NULL DEFAULT 0;"
    ];

    public static int LatestVersion => BaseVersion + Migrations.Length;

    public static int ReadVersion(SqliteConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA user_version";
        var version = (long)(command.ExecuteScalar() ?? 0L);
        return version < BaseVersion ? BaseVersion : (int)version;
    }

    public static void Migrate(SqliteConnection connection)
    {
        for (var version = ReadVersion(connection); version < LatestVersion; version++)
        {
            using var transaction = connection.BeginTransaction();
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = $"{Migrations[version - BaseVersion]} PRAGMA user_version = {version + 1};";
            command.ExecuteNonQuery();
            transaction.Commit();
        }
    }
}
