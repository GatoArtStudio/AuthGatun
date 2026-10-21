using AuthGatun.Domains.IdentityAccess.Application.Ports.In;
using AuthGatun.Domains.IdentityAccess.Application.Ports.Out;

namespace AuthGatun.Domains.IdentityAccess.Domain.Event;

public interface IDomainEventBus : IDomainEventPublisher
{
    void Register<T>(IDomainEventSubscriber<T> subscriber) where T : IDomainEvent;
}