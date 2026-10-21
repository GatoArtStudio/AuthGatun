namespace AuthGatun.Domains.IdentityAccess.Application.Ports.Outbound;

public interface ITotpCodeGenerator
{
    string Generate(string secretKey);
}
