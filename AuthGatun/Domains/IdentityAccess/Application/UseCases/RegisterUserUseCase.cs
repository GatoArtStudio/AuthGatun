using System;
using AuthGatun.Domains.IdentityAccess.Application.Models;
using AuthGatun.Domains.IdentityAccess.Application.Ports.Inbound;
using AuthGatun.Domains.IdentityAccess.Application.Ports.Outbound;
using AuthGatun.Domains.IdentityAccess.Domain.Aggregate;
using AuthGatun.Domains.IdentityAccess.Domain.ValueObject;

namespace AuthGatun.Domains.IdentityAccess.Application.UseCases;

public class RegisterUserUseCase(
    IDomainEventPublisher publisher,
    IVaultSession vault,
    IUserRepository repository,
    IPasswordHasher passwordHasher
) : IRegisterUseCase
{
    public AuthenticatedUser Execute(string userName, string password)
    {
        if (string.IsNullOrWhiteSpace(userName))
            throw new ArgumentException("The user to be registered cannot be null or contain spaces.");

        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("The password of the user to be registered cannot be null or contain spaces.");

        if (vault.Exists(userName))
            throw new ArgumentException("The user already exists, choose another user.");

        var newUser = new User(Guid.NewGuid(), new UserName(userName), new Password(passwordHasher.Hash(password)), true);
        vault.Create(userName, password);
        if (!repository.Save(newUser))
        {
            vault.Close();
            throw new InvalidOperationException("The user could not be saved.");
        }

        foreach (var domainEvent in newUser.UncommittedEvents)
            publisher.Publish(domainEvent);
        newUser.ClearEvents();

        return new AuthenticatedUser(newUser.Id, newUser.Username.Value);
    }
}
