using AuthGatun.Domains.IdentityAccess.Application.Models;

namespace AuthGatun.Domains.IdentityAccess.Application.Ports.Inbound;

public interface IUpdateKeyUseCase
{
    void Execute(UpdateKeyRequest request);
}
