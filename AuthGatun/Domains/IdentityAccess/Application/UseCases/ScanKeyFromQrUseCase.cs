using AuthGatun.Domains.IdentityAccess.Application.Models;
using AuthGatun.Domains.IdentityAccess.Application.Ports.Inbound;
using AuthGatun.Domains.IdentityAccess.Application.Ports.Outbound;
using AuthGatun.Domains.IdentityAccess.Domain.ValueObject;

namespace AuthGatun.Domains.IdentityAccess.Application.UseCases;

public class ScanKeyFromQrUseCase(IQrCodeReader qrCodeReader) : IScanKeyFromQrUseCase
{
    public ScannedKeyModel ScanImage(byte[] image)
    {
        var text = qrCodeReader.Read(image)
                   ?? throw new IdentityAccessException(IdentityAccessError.QrNotFound, "No QR code was found in the image.");

        return ParseLink(text);
    }

    public ScannedKeyModel ParseLink(string text)
    {
        var problem = OtpAuthUri.TryParse(text, out var uri);

        return problem switch
        {
            OtpAuthUriProblem.None => new ScannedKeyModel(uri!.ServiceName, uri.SecretKey),
            OtpAuthUriProblem.InvalidSecret => throw new IdentityAccessException(
                IdentityAccessError.QrInvalidSecret, "The link does not contain a valid secret key."),
            OtpAuthUriProblem.UnsupportedParameters => throw new IdentityAccessException(
                IdentityAccessError.QrUnsupportedParameters, "Only 6 digit, 30 second, SHA1 TOTP keys are supported."),
            _ => throw new IdentityAccessException(
                IdentityAccessError.QrNotOtpAuth, "The text is not an otpauth TOTP link.")
        };
    }
}
