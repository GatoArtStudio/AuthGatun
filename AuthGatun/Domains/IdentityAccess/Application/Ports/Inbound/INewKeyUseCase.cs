using AuthGatun.Domains.IdentityAccess.Domain.Model;

namespace AuthGatun.Domains.IdentityAccess.Application.Ports.In.UseCases;

public interface INewKeyUseCase
{
    void Execute(Key key);
}