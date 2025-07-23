using System;
using AuthGatun.Domains.IdentityAccess.Application.Ports.In.UseCases;
using AuthGatun.Domains.IdentityAccess.Application.Ports.Out;
using AuthGatun.Domains.IdentityAccess.Application.UseCases.Abstract;
using AuthGatun.Domains.IdentityAccess.Domain.Aggregate;

namespace AuthGatun.Domains.IdentityAccess.Application.UseCases;

public class LoginUseCase(IDomainEventPublisher publisher, IUserRepository repository, IPasswordHasher passwordHasher)
    : UseCase(publisher, repository), ILoginUseCase
{
    private readonly IPasswordHasher _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher), "The password hasher cannot be null.");

    public User? Execute(string userName, string password)
    {
        if (string.IsNullOrWhiteSpace(userName))
            throw new ArgumentException( "The username cannot contain spaces or be null.");

        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("The password cannot contain spaces or be null.");

        var user = Repository.FromName(userName);

        if (user is null)
            return null;

        var passwordIsValid = _passwordHasher.Verify(password, user.Password.Value);
        return passwordIsValid ? user : null;
    }
}