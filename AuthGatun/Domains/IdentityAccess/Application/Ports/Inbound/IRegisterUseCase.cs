using AuthGatun.Domains.IdentityAccess.Application.Models;

namespace AuthGatun.Domains.IdentityAccess.Application.Ports.Inbound;

public interface IRegisterUseCase
{
    RegistrationResult Execute(string userName, string password);
}
