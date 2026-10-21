using AuthGatun.Domains.IdentityAccess.Application.Models;

namespace AuthGatun.Domains.IdentityAccess.Application.Ports.Inbound;

public interface IRecoverAccessUseCase
{
    AuthenticatedUser? Execute(string userName, string recoveryCode, string newPassword);
}
