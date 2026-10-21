using System;
using AuthGatun.Domains.IdentityAccess.Application.Ports.Inbound;
using AuthGatun.Domains.IdentityAccess.Application.Ports.Outbound;
using AuthGatun.Domains.IdentityAccess.Domain.ValueObject;

namespace AuthGatun.Domains.IdentityAccess.Application.UseCases;

public class ChangePasswordUseCase(
    IVaultSession vault,
    IUserRepository repository,
    IPasswordHasher passwordHasher
) : IChangePasswordUseCase
{
    public void Execute(Guid userId, string currentPassword, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(currentPassword))
            throw new ArgumentException("The current password cannot be null or contain spaces.");

        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 8)
            throw new ArgumentException("The new password must be at least 8 characters long.");

        var user = repository.Read(userId) ?? throw new ArgumentException("The user is invalid or does not exist.");

        if (!vault.Open(user.Username.Value, currentPassword))
            throw new ArgumentException("The current password is incorrect.");

        user = repository.Read(userId) ?? throw new ArgumentException("The user is invalid or does not exist.");

        vault.ChangePassword(newPassword);
        user.ChangePassword(new Password(passwordHasher.Hash(newPassword)));
        repository.Save(user);
    }
}
