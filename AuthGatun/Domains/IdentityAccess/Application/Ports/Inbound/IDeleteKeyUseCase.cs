using AuthGatun.Domains.IdentityAccess.Domain.Model;

namespace AuthGatun.Domains.IdentityAccess.Application.Ports.In.UseCases;

public interface IDeleteKeyUseCase
{
    void Execute(Key key);
}