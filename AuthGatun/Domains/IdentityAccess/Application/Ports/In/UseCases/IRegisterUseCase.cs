using AuthGatun.Domains.IdentityAccess.Domain.Aggregate;

namespace AuthGatun.Domains.IdentityAccess.Application.Ports.In.UseCases;

public interface IRegisterUseCase
{
    User Execute(string userName, string password);
}