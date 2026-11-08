using AuthGatun.Domains.IdentityAccess.Application.Ports.Inbound;
using AuthGatun.Domains.IdentityAccess.Domain.Event;

namespace AuthGatun.Domains.IdentityAccess.Application.Ports.Outbound;

public interface IDomainEventBus : IDomainEventPublisher
{
    void Register<T>(IDomainEventSubscriber<T> subscriber) where T : IDomainEvent;
}
