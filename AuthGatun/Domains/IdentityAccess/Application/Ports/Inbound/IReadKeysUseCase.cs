using System;
using System.Collections.Generic;
using AuthGatun.Domains.IdentityAccess.Application.Models;

namespace AuthGatun.Domains.IdentityAccess.Application.Ports.Inbound;

public interface IReadKeysUseCase
{
    IReadOnlyList<KeySummary> Execute(Guid userId);
}
