using System;
using AuthGatun.Domains.IdentityAccess.Application.Ports.Out;

namespace AuthGatun.Domains.IdentityAccess.Application.UseCases.Abstract;

public abstract class UseCase
{
    protected IDomainEventPublisher Publisher { get; }
    protected IUserRepository Repository { get; }

    protected UseCase(IDomainEventPublisher publisher, IUserRepository repository)
    {
        Publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        Repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }
}