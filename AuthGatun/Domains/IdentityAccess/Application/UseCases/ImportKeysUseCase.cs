using System;
using System.Collections.Generic;
using AuthGatun.Domains.IdentityAccess.Application.Models;
using AuthGatun.Domains.IdentityAccess.Application.Ports.Inbound;
using AuthGatun.Domains.IdentityAccess.Application.Ports.Outbound;
using AuthGatun.Domains.IdentityAccess.Domain.Model;

namespace AuthGatun.Domains.IdentityAccess.Application.UseCases;

public class ImportKeysUseCase(IUserRepository repository, IKeyBackupCodec codec) : IImportKeysUseCase
{
    public ImportResult Execute(Guid userId, byte[] backup, string backupPassword)
    {
        if (string.IsNullOrWhiteSpace(backupPassword))
            throw new ArgumentException("The backup password cannot be null or contain spaces.");

        var user = repository.Read(userId) ?? throw new ArgumentException("The user is invalid or does not exist.");

        var keys = codec.Decode(backup, backupPassword)
                   ?? throw new ArgumentException("The backup password is incorrect or the file is not a valid backup.");

        var known = new HashSet<(string, string)>();
        foreach (var existing in user.Keys)
            known.Add((existing.ServiceName, existing.SecretKey));

        var imported = 0;
        var skipped = 0;
        foreach (var key in keys)
        {
            if (string.IsNullOrWhiteSpace(key.ServiceName) || string.IsNullOrWhiteSpace(key.SecretKey)
                || !known.Add((key.ServiceName, key.SecretKey)))
            {
                skipped++;
                continue;
            }

            user.AddKey(new Key(Guid.NewGuid(), userId, key.ServiceName, key.SecretKey));
            imported++;
        }

        if (imported > 0 && !repository.Save(user))
            throw new InvalidOperationException("The imported keys could not be saved.");

        return new ImportResult(imported, skipped);
    }
}
