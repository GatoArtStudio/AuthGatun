using System;
using System.Linq;
using AuthGatun.Domains.IdentityAccess.Application.Ports.In.UseCases;
using AuthGatun.Domains.IdentityAccess.Application.Ports.Out;
using AuthGatun.Domains.IdentityAccess.Application.UseCases.Abstract;
using AuthGatun.Domains.IdentityAccess.Domain.Model;

namespace AuthGatun.Domains.IdentityAccess.Application.UseCases;

public class ReadKeyUseCase(IDomainEventPublisher publisher, IUserRepository repository) : UseCase(publisher, repository), IReadKeyUseCase
{
    public Key? Execute(Guid id)
    {
        var key = Repository.ReadAll()
            .SelectMany(user => user.Keys)
            .FirstOrDefault(k => k.Id.Equals(id));

        return key;
    }
}