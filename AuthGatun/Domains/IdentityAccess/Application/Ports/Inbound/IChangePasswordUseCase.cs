using System;

namespace AuthGatun.Domains.IdentityAccess.Application.Ports.Inbound;

public interface IChangePasswordUseCase
{
    void Execute(Guid userId, string currentPassword, string newPassword);
}
