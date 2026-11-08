using AuthGatun.Domains.IdentityAccess.Application.Models;

namespace AuthGatun.Domains.IdentityAccess.Application.Ports.Inbound;

public interface IScanKeyFromQrUseCase
{
    ScannedKeyModel ScanImage(byte[] image);

    ScannedKeyModel ParseLink(string text);
}
