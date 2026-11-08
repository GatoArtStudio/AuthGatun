using System;

namespace AuthGatun.Domains.IdentityAccess.Application.Ports.Inbound;

public interface IGenerateTotpCodeUseCase
{
    string? Execute(Guid keyId);
}
