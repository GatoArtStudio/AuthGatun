using System;

namespace AuthGatun.Domains.IdentityAccess.Application.Ports.Inbound;

public interface IDeleteKeyUseCase
{
    bool Execute(Guid keyId);
}
