using System;
using AuthGatun.Domains.IdentityAccess.Application.Models;

namespace AuthGatun.Domains.IdentityAccess.Application.Ports.Inbound;

public interface IMoveKeyUseCase
{
    bool Execute(Guid keyId, KeyMoveDirection direction);
}
