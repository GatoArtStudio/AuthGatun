using AuthGatun.Domains.IdentityAccess.Application.Models;
using AuthGatun.Domains.IdentityAccess.Application.Ports.Inbound;
using AuthGatun.Domains.IdentityAccess.Application.Ports.Outbound;

namespace AuthGatun.Domains.IdentityAccess.Application.UseCases;

public class UpdateKeyUseCase(IDomainEventPublisher publisher, IUserRepository repository) : IUpdateKeyUseCase
{
    public void Execute(UpdateKeyRequest request)
    {
        var user = repository.Read(request.UserId);
        if (user is null) throw new IdentityAccessException(IdentityAccessError.UserNotFound, "IdentityAccess not found");

        var secretKey = string.IsNullOrWhiteSpace(request.SecretKey) ? null : request.SecretKey.Trim();
        if (!user.UpdateKey(request.KeyId, request.ServiceName, secretKey))
            throw new IdentityAccessException(IdentityAccessError.KeyNotFound, "Key not found");

        repository.Save(user);

        foreach (var domainEvent in user.UncommittedEvents)
            publisher.Publish(domainEvent);
        user.ClearEvents();
    }
}
