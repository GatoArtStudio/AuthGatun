using System.Linq;
using System;
using AuthGatun.Domains.IdentityAccess.Application.Ports.Inbound;
using AuthGatun.Domains.IdentityAccess.Application.Ports.Outbound;

namespace AuthGatun.Domains.IdentityAccess.Application.UseCases;

public class DeleteKeyUseCase(IDomainEventPublisher publisher, IUserRepository repository) : IDeleteKeyUseCase
{
    public bool Execute(Guid keyId)
    {
        var user = repository.ReadAll().FirstOrDefault(u => u.Keys.Any(k => k.Id == keyId));
        var key = user?.Keys.First(k => k.Id == keyId);
        if (user is null || key is null) return false;

        user.RemoveKey(key);
        repository.Save(user);

        foreach (var domainEvent in user.UncommittedEvents)
            publisher.Publish(domainEvent);
        user.ClearEvents();

        return true;
    }
}
