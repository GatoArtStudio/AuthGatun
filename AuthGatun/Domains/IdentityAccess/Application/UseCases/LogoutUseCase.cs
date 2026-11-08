using AuthGatun.Domains.IdentityAccess.Application.Ports.Inbound;
using AuthGatun.Domains.IdentityAccess.Application.Ports.Outbound;

namespace AuthGatun.Domains.IdentityAccess.Application.UseCases;

public class LogoutUseCase(IVaultSession vault) : ILogoutUseCase
{
    public void Execute() => vault.Close();
}
