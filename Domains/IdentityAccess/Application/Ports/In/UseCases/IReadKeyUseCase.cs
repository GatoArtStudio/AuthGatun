using System;
using AuthGatun.Domains.IdentityAccess.Domain.Model;

namespace AuthGatun.Domains.IdentityAccess.Application.Ports.In.UseCases;

public interface IReadKeyUseCase
{
    Key? Execute(Guid id);
}