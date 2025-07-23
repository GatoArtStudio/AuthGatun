using System;
using AuthGatun.Domains.IdentityAccess.Application.Ports.In.UseCases;
using AuthGatun.Domains.IdentityAccess.Application.Ports.Out;
using AuthGatun.Domains.IdentityAccess.Application.UseCases.Abstract;
using AuthGatun.Domains.IdentityAccess.Domain.Aggregate;
using AuthGatun.Domains.IdentityAccess.Domain.ValueObject;

namespace AuthGatun.Domains.IdentityAccess.Application.UseCases;

public class RegisterUserCase(IDomainEventPublisher publisher, IUserRepository repository, IPasswordHasher passwordHasher)
    : UseCase(publisher, repository), IRegisterUseCase
{
    private IPasswordHasher _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher), "The password hasher cannot be null.");
    
    public User Execute(string userName, string password)
    {
        if (string.IsNullOrWhiteSpace(userName))
            throw new ArgumentException("The user to be registered cannot be null or contain spaces.");
        
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("The password of the user to be registered cannot be null or contain spaces.");

        User? userRegisted = Repository.FromName(userName);
        if (userRegisted is not null)
            throw new ArgumentException("The user already exists, choose another user.");

        string hashPassword = _passwordHasher.Hash(password);
        var newUser = new User(Guid.NewGuid(), new UserName(userName), new Password(hashPassword), true);
        Repository.Save(newUser);
        
        newUser.UncommittedEvents.ForEach(Publisher.Publish);
        newUser.ClearEvents();
        
        return newUser;
    }
}