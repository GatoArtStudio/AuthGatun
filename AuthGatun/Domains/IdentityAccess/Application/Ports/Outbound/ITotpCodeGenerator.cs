using AuthGatun.Domains.IdentityAccess.Domain.ValueObject;

namespace AuthGatun.Domains.IdentityAccess.Application.Ports.Outbound;

public interface ITotpCodeGenerator
{
    string Generate(string secretKey, TotpParameters parameters);

    int RemainingSeconds(TotpParameters parameters);
}
