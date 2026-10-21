using System;
using AuthGatun.Domains.IdentityAccess.Application.Models;
using AuthGatun.Domains.IdentityAccess.Application.Ports.Inbound;
using AuthGatun.Domains.IdentityAccess.Application.Ports.Outbound;

namespace AuthGatun.Domains.IdentityAccess.Application.UseCases;

public class LoginUseCase(IUserRepository repository, IPasswordHasher passwordHasher) : ILoginUseCase
{
    public AuthenticatedUser? Execute(string userName, string password)
    {
        if (string.IsNullOrWhiteSpace(userName))
            throw new ArgumentException("The username cannot contain spaces or be null.");

        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("The password cannot contain spaces or be null.");

        var user = repository.FromName(userName);

        if (user is null || !passwordHasher.Verify(password, user.Password.Value))
            return null;

        return new AuthenticatedUser(user.Id, user.Username.Value);
    }
}
