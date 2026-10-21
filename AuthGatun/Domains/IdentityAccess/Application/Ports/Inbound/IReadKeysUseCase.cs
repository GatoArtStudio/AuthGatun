using System;
using System.Collections.Generic;
using AuthGatun.Domains.IdentityAccess.Domain.Model;

namespace AuthGatun.Domains.IdentityAccess.Application.Ports.In.UseCases;

public interface IReadKeysUseCase
{
    List<Key> Execute(Guid userId);
}