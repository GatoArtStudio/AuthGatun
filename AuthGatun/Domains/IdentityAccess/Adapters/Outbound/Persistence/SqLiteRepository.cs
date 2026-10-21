using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics.CodeAnalysis;
using AuthGatun.Domains.IdentityAccess.Application.Ports.Outbound;
using AuthGatun.Domains.IdentityAccess.Domain.Aggregate;
using AuthGatun.Domains.IdentityAccess.Domain.Model;
using AuthGatun.Domains.IdentityAccess.Domain.ValueObject;
using Microsoft.Data.Sqlite;

namespace AuthGatun.Domains.IdentityAccess.Adapters.Outbound.Persistence;

public class SqLiteRepository(SqlCipherVault vault) : IUserRepository
{
    public User? Read([NotNull] Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentNullException(nameof(id));

        return QueryUsers("WHERE id = @value", id.ToString()).FirstOrDefault();
    }

    public bool Delete([NotNull] Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentNullException(nameof(id));

        using var command = vault.Connection.CreateCommand();
        command.CommandText = "DELETE FROM keys WHERE user_id = @id; DELETE FROM users WHERE id = @id;";
        command.Parameters.AddWithValue("@id", id.ToString());
        return command.ExecuteNonQuery() > 0;
    }

    public bool Save([NotNull] User entity)
    {
        if (entity is null)
            throw new ArgumentNullException(nameof(entity));

        var connection = vault.Connection;
        using var transaction = connection.BeginTransaction();

        try
        {
            using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = @"
                    INSERT INTO users (id, user_name, password, use_password)
                    VALUES (@id, @username, @password, @usePassword)
                    ON CONFLICT(id) DO UPDATE SET
                        user_name = excluded.user_name,
                        password = excluded.password,
                        use_password = excluded.use_password;";
                command.Parameters.AddWithValue("@id", entity.Id.ToString());
                command.Parameters.AddWithValue("@username", entity.Username.Value);
                command.Parameters.AddWithValue("@password", entity.Password.Value);
                command.Parameters.AddWithValue("@usePassword", entity.UsePassword);
                command.ExecuteNonQuery();
            }

            using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = "DELETE FROM keys WHERE user_id = @userId";
                command.Parameters.AddWithValue("@userId", entity.Id.ToString());
                command.ExecuteNonQuery();
            }

            foreach (var key in entity.Keys)
            {
                using var command = connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = @"
                    INSERT INTO keys (id, user_id, service_name, secret_key)
                    VALUES (@id, @userId, @serviceName, @secretKey);";
                command.Parameters.AddWithValue("@id", key.Id.ToString());
                command.Parameters.AddWithValue("@userId", key.UserId.ToString());
                command.Parameters.AddWithValue("@serviceName", key.ServiceName);
                command.Parameters.AddWithValue("@secretKey", key.SecretKey);
                command.ExecuteNonQuery();
            }

            transaction.Commit();
            return true;
        }
        catch (SqliteException)
        {
            transaction.Rollback();
            return false;
        }
    }

    public List<User> ReadAll() => QueryUsers(string.Empty, null);

    public User? FromName(string userName) =>
        QueryUsers("WHERE user_name = @value", userName).FirstOrDefault();

    private List<User> QueryUsers(string whereClause, string? value)
    {
        var connection = vault.Connection;
        var users = new List<User>();

        using (var command = connection.CreateCommand())
        {
            command.CommandText = $"SELECT id, user_name, password, use_password FROM users {whereClause}";
            if (value is not null)
                command.Parameters.AddWithValue("@value", value);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var password = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
                users.Add(new User(
                    Guid.Parse(reader.GetString(0)),
                    new UserName(reader.GetString(1)),
                    new Password(password),
                    reader.GetBoolean(3)));
            }
        }

        foreach (var user in users)
        {
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT id, service_name, secret_key FROM keys WHERE user_id = @userId";
            command.Parameters.AddWithValue("@userId", user.Id.ToString());

            using var reader = command.ExecuteReader();
            while (reader.Read())
                user.AddKey(new Key(Guid.Parse(reader.GetString(0)), user.Id, reader.GetString(1), reader.GetString(2)));
        }

        return users;
    }
}
