using System;
using AuthGatun.Domains.IdentityAccess.Application.Models;
using AuthGatun.Domains.IdentityAccess.Application.Ports.Inbound;
using AuthGatun.Domains.IdentityAccess.Application.Ports.Outbound;
using AuthGatun.Domains.IdentityAccess.Domain.ValueObject;

namespace AuthGatun.Domains.IdentityAccess.Application.UseCases;

public class RecoverAccessUseCase(
    IVaultSession vault,
    IUserRepository repository,
    IPasswordHasher passwordHasher
) : IRecoverAccessUseCase
{
    public AuthenticatedUser? Execute(string userName, string recoveryCode, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(recoveryCode))
            throw new ArgumentException("The username and the recovery code cannot be null or contain spaces.");

        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 8)
            throw new ArgumentException("The new password must be at least 8 characters long.");

        if (!vault.OpenWithRecoveryCode(userName, recoveryCode))
            return null;

        var user = repository.FromName(userName);
        if (user is null)
        {
            vault.Close();
            return null;
        }

        vault.ChangePassword(newPassword);
        user.ChangePassword(new Password(passwordHasher.Hash(newPassword)));
        repository.Save(user);

        return new AuthenticatedUser(user.Id, user.Username.Value);
    }
}
