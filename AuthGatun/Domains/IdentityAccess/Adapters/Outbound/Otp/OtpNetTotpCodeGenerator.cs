using AuthGatun.Domains.IdentityAccess.Application.Ports.Outbound;
using OtpNet;

namespace AuthGatun.Domains.IdentityAccess.Adapters.Outbound.Otp;

public class OtpNetTotpCodeGenerator : ITotpCodeGenerator
{
    public string Generate(string secretKey)
    {
        var totp = new Totp(Base32Encoding.ToBytes(secretKey));
        return totp.ComputeTotp();
    }
}
