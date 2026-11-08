using System;
using System.Collections.Generic;
using System.IO;
using AuthGatun.Domains.IdentityAccess.Application.Ports.Outbound;
using Microsoft.Data.Sqlite;

namespace AuthGatun.Domains.IdentityAccess.Adapters.Outbound.Persistence;

public sealed record LegacyKey(Guid Id, string ServiceName, string SecretKey);

public sealed record LegacyUser(Guid Id, string UserName, string PasswordHash, bool UsePassword, IReadOnlyList<LegacyKey> Keys);

public sealed class LegacyDatabaseMigrator(RepositoryOptions options, IPasswordHasher passwordHasher)
{
    private string LegacyPath => Path.Combine(options.AppFolderPathData, "AuthGatun", "authgatun.db");

    public bool HasUser(string userName)
    {
        if (!File.Exists(LegacyPath)) return false;

        using var connection = OpenLegacy();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM users WHERE user_name = @name";
        command.Parameters.AddWithValue("@name", userName);
        return Convert.ToInt64(command.ExecuteScalar()) > 0;
    }

    public LegacyUser? FindVerified(string userName, string password)
    {
        if (!File.Exists(LegacyPath)) return null;

        using var connection = OpenLegacy();
        Guid id;
        string hash;
        bool usePassword;

        using (var command = connection.CreateCommand())
        {
            command.CommandText = "SELECT id, password, use_password FROM users WHERE user_name = @name";
            command.Parameters.AddWithValue("@name", userName);
            using var reader = command.ExecuteReader();
            if (!reader.Read()) return null;
            id = Guid.Parse(reader.GetString(0));
            hash = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
            usePassword = reader.GetBoolean(2);
        }

        if (!IsPasswordValid(password, hash)) return null;

        var keys = new List<LegacyKey>();
        using (var command = connection.CreateCommand())
        {
            command.CommandText = "SELECT id, service_name, secret_key FROM keys WHERE user_id = @id";
            command.Parameters.AddWithValue("@id", id.ToString());
            using var reader = command.ExecuteReader();
            while (reader.Read())
                keys.Add(new LegacyKey(Guid.Parse(reader.GetString(0)), reader.GetString(1), reader.GetString(2)));
        }

        return new LegacyUser(id, userName, hash, usePassword, keys);
    }

    public void Remove(Guid userId)
    {
        if (!File.Exists(LegacyPath)) return;

        long remainingUsers;
        using (var connection = OpenLegacy())
        {
            Execute(connection, "PRAGMA secure_delete = ON");
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "DELETE FROM keys WHERE user_id = @id; DELETE FROM users WHERE id = @id;";
                command.Parameters.AddWithValue("@id", userId.ToString());
                command.ExecuteNonQuery();
            }

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT COUNT(*) FROM users";
                remainingUsers = Convert.ToInt64(command.ExecuteScalar());
            }

            if (remainingUsers > 0)
                Execute(connection, "VACUUM");
        }

        if (remainingUsers == 0)
            OverwriteAndDelete(LegacyPath);
    }

    private bool IsPasswordValid(string password, string hash)
    {
        try
        {
            return !string.IsNullOrEmpty(hash) && passwordHasher.Verify(password, hash);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private SqliteConnection OpenLegacy()
    {
        var connection = new SqliteConnection($"Data Source={LegacyPath};Mode=ReadWrite;Pooling=False");
        connection.Open();
        return connection;
    }

    private static void Execute(SqliteConnection connection, string sql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    private static void OverwriteAndDelete(string path)
    {
        var length = new FileInfo(path).Length;
        using (var stream = new FileStream(path, FileMode.Open, FileAccess.Write))
        {
            var zeros = new byte[4096];
            for (long written = 0; written < length; written += zeros.Length)
                stream.Write(zeros, 0, (int)Math.Min(zeros.Length, length - written));
            stream.Flush(true);
        }

        File.Delete(path);
    }
}
