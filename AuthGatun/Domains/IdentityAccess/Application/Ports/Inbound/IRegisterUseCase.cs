using AuthGatun.Domains.IdentityAccess.Application.Models;

namespace AuthGatun.Domains.IdentityAccess.Application.Ports.Inbound;

public interface IRegisterUseCase
{
    AuthenticatedUser Execute(string userName, string password);
}
