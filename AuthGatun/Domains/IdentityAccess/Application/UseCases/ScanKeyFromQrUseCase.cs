using System.Linq;
using AuthGatun.Domains.IdentityAccess.Application.Models;
using AuthGatun.Domains.IdentityAccess.Application.Ports.Inbound;
using AuthGatun.Domains.IdentityAccess.Application.Ports.Outbound;
using AuthGatun.Domains.IdentityAccess.Domain.ValueObject;

namespace AuthGatun.Domains.IdentityAccess.Application.UseCases;

public class ScanKeyFromQrUseCase(IQrCodeReader qrCodeReader) : IScanKeyFromQrUseCase
{
    public ScannedKeyModel ScanImage(byte[] image) => ParseLink(ReadText(image));

    public ScannedKeyModel ParseLink(string text)
    {
        var result = ParseLinkForKeys(text);
        if (result.Keys.Count != 1)
            throw new IdentityAccessException(
                IdentityAccessError.QrNotOtpAuth, "The text does not contain exactly one TOTP key.");

        return result.Keys[0];
    }

    public ScanResultModel ScanImageForKeys(byte[] image) => ParseLinkForKeys(ReadText(image));

    public ScanResultModel ParseLinkForKeys(string text)
    {
        if (OtpMigrationUri.IsMigration(text))
            return ParseMigration(text);

        var problem = OtpAuthUri.TryParse(text, out var uri);

        return problem switch
        {
            OtpAuthUriProblem.None => new ScanResultModel([
                new ScannedKeyModel(uri!.ServiceName, uri.SecretKey, TotpParametersModel.FromDomain(uri.Parameters))
            ]),
            OtpAuthUriProblem.InvalidSecret => throw new IdentityAccessException(
                IdentityAccessError.QrInvalidSecret, "The link does not contain a valid secret key."),
            OtpAuthUriProblem.UnsupportedParameters => throw new IdentityAccessException(
                IdentityAccessError.QrUnsupportedParameters, "The key uses an unsupported algorithm, digit count, period or type."),
            _ => throw new IdentityAccessException(
                IdentityAccessError.QrNotOtpAuth, "The text is not an otpauth TOTP link.")
        };
    }

    private string ReadText(byte[] image) =>
        qrCodeReader.Read(image)
        ?? throw new IdentityAccessException(IdentityAccessError.QrNotFound, "No QR code was found in the image.");

    private static ScanResultModel ParseMigration(string text)
    {
        if (OtpMigrationUri.TryParse(text, out var migration) != OtpMigrationProblem.None)
            throw new IdentityAccessException(
                IdentityAccessError.MigrationInvalidData, "The migration data is not valid.");

        if (migration!.Entries.Count == 0)
            throw new IdentityAccessException(
                IdentityAccessError.MigrationNoSupportedKeys, "The migration contains no supported TOTP keys.");

        var keys = migration.Entries
            .Select(entry => new ScannedKeyModel(entry.ServiceName, entry.SecretKey, TotpParametersModel.FromDomain(entry.Parameters)))
            .ToList();
        return new ScanResultModel(keys, migration.Unsupported);
    }
}
