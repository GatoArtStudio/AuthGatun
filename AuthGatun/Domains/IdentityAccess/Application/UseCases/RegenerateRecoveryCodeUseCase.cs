using AuthGatun.Domains.IdentityAccess.Application.Ports.Inbound;
using AuthGatun.Domains.IdentityAccess.Application.Ports.Outbound;

namespace AuthGatun.Domains.IdentityAccess.Application.UseCases;

public class RegenerateRecoveryCodeUseCase(IVaultSession vault) : IRegenerateRecoveryCodeUseCase
{
    public string Execute() => vault.RegenerateRecoveryCode();
}
