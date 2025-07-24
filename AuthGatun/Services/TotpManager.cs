using OtpNet;

namespace AuthGatun.Services;

public class TotpManager
{
    public string GetTotpCode(string secretKey)
    {
        var totp = new Totp(Base32Encoding.ToBytes(secretKey));
        string code = totp.ComputeTotp();
        return code;
    }
}