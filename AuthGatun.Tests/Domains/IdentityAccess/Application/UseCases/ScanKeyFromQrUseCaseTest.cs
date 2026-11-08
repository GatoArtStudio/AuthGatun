using AuthGatun.Domains.IdentityAccess.Application;
using AuthGatun.Domains.IdentityAccess.Application.Models;
using AuthGatun.Domains.IdentityAccess.Application.Ports.Outbound;
using AuthGatun.Domains.IdentityAccess.Application.UseCases;
using Xunit;

namespace AuthGatun.Tests.Domains.IdentityAccess.Application.UseCases;

public class ScanKeyFromQrUseCaseTest
{
    private sealed class FakeQrCodeReader(string? text) : IQrCodeReader
    {
        public string? Read(byte[] image) => text;
    }

    private static ScanKeyFromQrUseCase UseCase(string? qrText) => new(new FakeQrCodeReader(qrText));

    [Fact]
    public void ScanImageReturnsTheServiceAndTheSecretOfTheQr()
    {
        var key = UseCase("otpauth://totp/GitHub:gato?secret=JBSWY3DPEHPK3PXP&issuer=GitHub").ScanImage([1, 2, 3]);

        Assert.Equal(new ScannedKeyModel("GitHub", "JBSWY3DPEHPK3PXP"), key);
    }

    [Fact]
    public void ScanImageReportsAnImageWithoutQr()
    {
        var error = Assert.Throws<IdentityAccessException>(() => UseCase(null).ScanImage([1]));

        Assert.Equal(IdentityAccessError.QrNotFound, error.Error);
    }

    [Theory]
    [InlineData("https://example.com", IdentityAccessError.QrNotOtpAuth)]
    [InlineData("otpauth://totp/A:b?secret=", IdentityAccessError.QrInvalidSecret)]
    [InlineData("otpauth://totp/A:b?secret=JBSWY3DPEHPK3PXP&digits=8", IdentityAccessError.QrUnsupportedParameters)]
    public void ParseLinkMapsEachProblemToAnError(string text, IdentityAccessError expected)
    {
        var error = Assert.Throws<IdentityAccessException>(() => UseCase(null).ParseLink(text));

        Assert.Equal(expected, error.Error);
    }
}
