using System;
using AuthGatun.Domains.IdentityAccess.Application.Models;
using AuthGatun.Domains.IdentityAccess.Application.Ports.Inbound;
using AuthGatun.Domains.IdentityAccess.Application.Ports.Outbound;
using AuthGatun.Domains.IdentityAccess.Domain.Model;

namespace AuthGatun.Domains.IdentityAccess.Application.UseCases;

public class NewKeyUseCase(IDomainEventPublisher publisher, IUserRepository repository) : INewKeyUseCase
{
    public void Execute(NewKeyRequest request)
    {
        var user = repository.Read(request.UserId);
        if (user is null) throw new Exception("IdentityAccess not found");

        user.AddKey(new Key(Guid.NewGuid(), request.UserId, request.ServiceName, request.SecretKey));
        repository.Save(user);

        foreach (var domainEvent in user.UncommittedEvents)
            publisher.Publish(domainEvent);
        user.ClearEvents();
    }
}
