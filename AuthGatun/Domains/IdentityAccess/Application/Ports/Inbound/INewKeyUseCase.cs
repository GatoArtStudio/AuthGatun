using AuthGatun.Domains.IdentityAccess.Application.Models;

namespace AuthGatun.Domains.IdentityAccess.Application.Ports.Inbound;

public interface INewKeyUseCase
{
    void Execute(NewKeyRequest request);
}
