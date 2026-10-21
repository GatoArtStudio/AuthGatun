using System;
using AuthGatun.Domains.IdentityAccess.Application.Models;

namespace AuthGatun.Domains.IdentityAccess.Application.Ports.Inbound;

public interface IImportKeysUseCase
{
    ImportResult Execute(Guid userId, byte[] backup, string backupPassword);
}
