using System;
using System.Linq;
using AuthGatun.Domains.IdentityAccess.Application.Models;
using AuthGatun.Domains.IdentityAccess.Application.Ports.Inbound;
using AuthGatun.Domains.IdentityAccess.Application.Ports.Outbound;

namespace AuthGatun.Domains.IdentityAccess.Application.UseCases;

public class ExportKeysUseCase(IUserRepository repository, IKeyBackupCodec codec) : IExportKeysUseCase
{
    public byte[] Execute(Guid userId, string backupPassword)
    {
        if (string.IsNullOrWhiteSpace(backupPassword) || backupPassword.Length < 8)
            throw new ArgumentException("The backup password must be at least 8 characters long.");

        var user = repository.Read(userId) ?? throw new ArgumentException("The user is invalid or does not exist.");

        var keys = user.Keys.Select(key => new BackupKey(key.ServiceName, key.SecretKey)).ToList();
        return codec.Encode(keys, backupPassword);
    }
}
