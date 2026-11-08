namespace AuthGatun.Domains.IdentityAccess.Application.Ports.Outbound;

public interface IQrCodeReader
{
    // Returns the text of the first QR code found, or null when the image has none.
    string? Read(byte[] image);
}
