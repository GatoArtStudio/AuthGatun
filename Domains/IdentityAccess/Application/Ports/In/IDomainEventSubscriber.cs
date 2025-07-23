using System;
using AuthGatun.Domains.IdentityAccess.Domain.Event;

namespace AuthGatun.Domains.IdentityAccess.Application.Ports.In;

public interface IDomainEventSubscriber<T> where T : IDomainEvent
{
    Type SubscribedTo();
    void Handle(T @event);
}