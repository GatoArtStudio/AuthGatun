using System;

namespace AuthGatun.Domains.IdentityAccess.Application.Ports.Inbound;

public interface IExportKeysUseCase
{
    byte[] Execute(Guid userId, string backupPassword);
}
