using System;
using AuthGatun.Domains.IdentityAccess.Domain.ValueObject;

namespace AuthGatun.Domains.IdentityAccess.Application.Models;

public record TotpParametersModel(string Algorithm, int Digits, int PeriodSeconds)
{
    public static TotpParametersModel Default { get; } = new("SHA1", 6, 30);

    public bool IsDefault => Equals(Default);

    public static TotpParametersModel FromDomain(TotpParameters parameters) =>
        new(parameters.Algorithm.ToString().ToUpperInvariant(), parameters.Digits, parameters.PeriodSeconds);

    public TotpParameters ToDomain()
    {
        var algorithm = Algorithm.Trim().ToUpperInvariant() switch
        {
            "SHA1" => TotpAlgorithm.Sha1,
            "SHA256" => TotpAlgorithm.Sha256,
            "SHA512" => TotpAlgorithm.Sha512,
            _ => throw new ArgumentException("The algorithm is not supported.", nameof(Algorithm))
        };

        return new TotpParameters(algorithm, Digits, PeriodSeconds);
    }
}
