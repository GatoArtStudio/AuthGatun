using System;
using AuthGatun.Domains.IdentityAccess.Application.Ports.In.UseCases;
using AuthGatun.Domains.IdentityAccess.Application.Ports.Out;
using AuthGatun.Domains.IdentityAccess.Application.UseCases.Abstract;
using AuthGatun.Domains.IdentityAccess.Domain.Model;

namespace AuthGatun.Domains.IdentityAccess.Application.UseCases;

public class NewKeyUseCase(IDomainEventPublisher publisher, IUserRepository repository)
    : UseCase(publisher, repository), INewKeyUseCase
{
    public void Execute(Key key)
    {
        var user = Repository.Read(key.UserId);
        if (user is null) throw new Exception("IdentityAccess not found");
        
        user.AddKey(key);
        Repository.Save(user);
        
        user.UncommittedEvents.ForEach(Publisher.Publish);
        user.ClearEvents();
    }
}