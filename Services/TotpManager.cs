using System.Collections.ObjectModel;
using AuthGatun.Models;
using OtpNet;

namespace AuthGatun.ViewModels;

public class TotpManager
{
    public ObservableCollection<ServiceKey> ServiceKeys { get; set; }

    public TotpManager(ObservableCollection<ServiceKey> serviceKeys)
    {
        ServiceKeys = serviceKeys;
    }

    public string GetTotpCode(string secretKey)
    {
        var totp = new Totp(Base32Encoding.ToBytes(secretKey));
        string code = totp.ComputeTotp();
        return code;
    }
}