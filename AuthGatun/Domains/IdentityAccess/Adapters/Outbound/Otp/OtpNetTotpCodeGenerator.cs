using System;
using AuthGatun.Domains.IdentityAccess.Application.Ports.Outbound;
using OtpNet;

namespace AuthGatun.Domains.IdentityAccess.Adapters.Outbound.Otp;

public class OtpNetTotpCodeGenerator : ITotpCodeGenerator
{
    private const int DefaultStepSeconds = 30;

    public int PeriodSeconds => DefaultStepSeconds;

    public string Generate(string secretKey)
    {
        var totp = new Totp(Base32Encoding.ToBytes(secretKey), step: DefaultStepSeconds);
        return totp.ComputeTotp();
    }

    public int RemainingSeconds()
    {
        var elapsed = DateTimeOffset.UtcNow.ToUnixTimeSeconds() % DefaultStepSeconds;
        return (int)(DefaultStepSeconds - elapsed);
    }
}
