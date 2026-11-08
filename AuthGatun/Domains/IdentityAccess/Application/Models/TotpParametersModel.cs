namespace AuthGatun.Domains.IdentityAccess.Application.Models;

public record TotpParametersModel(string Algorithm, int Digits, int PeriodSeconds)
{
    public static TotpParametersModel Default { get; } = new("SHA1", 6, 30);
}
