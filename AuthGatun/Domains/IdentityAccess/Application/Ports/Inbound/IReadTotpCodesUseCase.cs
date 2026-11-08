using System;
using System.Collections.Generic;
using AuthGatun.Domains.IdentityAccess.Application.Models;

namespace AuthGatun.Domains.IdentityAccess.Application.Ports.Inbound;

public interface IReadTotpCodesUseCase
{
    IReadOnlyList<TotpCodeModel> Execute(Guid userId);
}
