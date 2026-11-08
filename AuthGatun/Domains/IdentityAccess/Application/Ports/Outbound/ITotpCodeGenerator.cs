namespace AuthGatun.Domains.IdentityAccess.Application.Ports.Outbound;

public interface ITotpCodeGenerator
{
    int PeriodSeconds { get; }

    string Generate(string secretKey);

    int RemainingSeconds();
}
