using AuthGatun.Domains.IdentityAccess.Application.Models;

namespace AuthGatun.Domains.IdentityAccess.Application.Ports.Inbound;

public interface ILoginUseCase
{
    AuthenticatedUser? Execute(string userName, string password);
}
