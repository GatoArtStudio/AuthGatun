using System;
using AuthGatun.Domains.IdentityAccess.Application.Ports.Outbound;
using AuthGatun.Domains.IdentityAccess.Domain.ValueObject;
using OtpNet;

namespace AuthGatun.Domains.IdentityAccess.Adapters.Outbound.Otp;

public class OtpNetTotpCodeGenerator : ITotpCodeGenerator
{
    public string Generate(string secretKey, TotpParameters parameters) =>
        Generate(secretKey, parameters, DateTime.UtcNow);

    public string Generate(string secretKey, TotpParameters parameters, DateTime utcNow)
    {
        var totp = new Totp(
            Base32Encoding.ToBytes(secretKey),
            step: parameters.PeriodSeconds,
            mode: ToHashMode(parameters.Algorithm),
            totpSize: parameters.Digits);
        return totp.ComputeTotp(utcNow);
    }

    public int RemainingSeconds(TotpParameters parameters) =>
        RemainingSeconds(parameters, DateTimeOffset.UtcNow);

    public int RemainingSeconds(TotpParameters parameters, DateTimeOffset utcNow)
    {
        var elapsed = utcNow.ToUnixTimeSeconds() % parameters.PeriodSeconds;
        return (int)(parameters.PeriodSeconds - elapsed);
    }

    private static OtpHashMode ToHashMode(TotpAlgorithm algorithm) => algorithm switch
    {
        TotpAlgorithm.Sha256 => OtpHashMode.Sha256,
        TotpAlgorithm.Sha512 => OtpHashMode.Sha512,
        _ => OtpHashMode.Sha1
    };
}
