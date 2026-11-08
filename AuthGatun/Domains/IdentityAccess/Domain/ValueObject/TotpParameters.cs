using System;

namespace AuthGatun.Domains.IdentityAccess.Domain.ValueObject;

public enum TotpAlgorithm
{
    Sha1,
    Sha256,
    Sha512
}

public sealed record TotpParameters
{
    public const int MinDigits = 6;
    public const int MaxDigits = 8;
    public const int MinPeriodSeconds = 1;
    public const int MaxPeriodSeconds = 300;

    public static TotpParameters Default { get; } = new(TotpAlgorithm.Sha1, 6, 30);

    public TotpAlgorithm Algorithm { get; }
    public int Digits { get; }
    public int PeriodSeconds { get; }

    public TotpParameters(TotpAlgorithm algorithm, int digits, int periodSeconds)
    {
        if (!Enum.IsDefined(algorithm))
            throw new ArgumentException("The algorithm is not supported.", nameof(algorithm));

        if (digits is < MinDigits or > MaxDigits)
            throw new ArgumentException("The digits must be between 6 and 8.", nameof(digits));

        if (periodSeconds is < MinPeriodSeconds or > MaxPeriodSeconds)
            throw new ArgumentException("The period must be between 1 and 300 seconds.", nameof(periodSeconds));

        Algorithm = algorithm;
        Digits = digits;
        PeriodSeconds = periodSeconds;
    }
}
