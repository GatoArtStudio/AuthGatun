using System;
using System.Collections.Generic;
using AuthGatun.Domains.IdentityAccess.Application.Ports.In.UseCases;
using AuthGatun.Domains.IdentityAccess.Application.Ports.Out;
using AuthGatun.Domains.IdentityAccess.Application.UseCases.Abstract;
using AuthGatun.Domains.IdentityAccess.Domain.Aggregate;
using AuthGatun.Domains.IdentityAccess.Domain.Model;

namespace AuthGatun.Domains.IdentityAccess.Application.UseCases;

public class ReadKeysUseCase(IDomainEventPublisher publisher, IUserRepository repository)
    : UseCase(publisher, repository), IReadKeysUseCase
{
    public List<Key> Execute(Guid userId)
    {
        User? user = Repository.Read(userId);
        if (user is null)
            throw new ArgumentException("The user is invalid or does not exist.");

        return user.Keys;
    }
}