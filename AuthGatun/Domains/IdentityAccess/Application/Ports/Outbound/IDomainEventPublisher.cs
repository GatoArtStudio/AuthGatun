using AuthGatun.Domains.IdentityAccess.Domain.Event;

namespace AuthGatun.Domains.IdentityAccess.Application.Ports.Outbound;

public interface IDomainEventPublisher
{
    void Publish(IDomainEvent @event);
}