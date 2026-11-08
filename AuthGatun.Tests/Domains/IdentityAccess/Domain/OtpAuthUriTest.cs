using AuthGatun.Domains.IdentityAccess.Domain.ValueObject;
using Xunit;

namespace AuthGatun.Tests.Domains.IdentityAccess.Domain;

public class OtpAuthUriTest
{
    [Theory]
    [InlineData("otpauth://totp/GitHub:gato?secret=JBSWY3DPEHPK3PXP&issuer=GitHub", "GitHub", "JBSWY3DPEHPK3PXP")]
    [InlineData("otpauth://totp/Example:alice@example.com?secret=jbswy3dpehpk3pxp", "Example", "JBSWY3DPEHPK3PXP")]
    [InlineData("otpauth://totp/alice@example.com?secret=JBSWY3DPEHPK3PXP", "alice@example.com", "JBSWY3DPEHPK3PXP")]
    [InlineData("otpauth://totp/My%20Service:bob?secret=JBSW%20Y3DP-EHPK3PXP%3D%3D&issuer=My%20Service", "My Service", "JBSWY3DPEHPK3PXP")]
    [InlineData("  OTPAUTH://TOTP/Acme:a?secret=JBSWY3DPEHPK3PXP&algorithm=sha1&digits=6&period=30  ", "Acme", "JBSWY3DPEHPK3PXP")]
    [InlineData("otpauth://totp/?secret=JBSWY3DPEHPK3PXP", "TOTP", "JBSWY3DPEHPK3PXP")]
    public void ParsesValidLinks(string text, string serviceName, string secret)
    {
        Assert.Equal(OtpAuthUriProblem.None, OtpAuthUri.TryParse(text, out var uri));
        Assert.Equal(new OtpAuthUri(serviceName, secret, TotpParameters.Default), uri);
    }

    [Fact]
    public void IssuerParameterWinsOverTheLabelPrefix()
    {
        OtpAuthUri.TryParse("otpauth://totp/Old:me?secret=JBSWY3DPEHPK3PXP&issuer=New", out var uri);

        Assert.Equal("New", uri!.ServiceName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("https://example.com")]
    [InlineData("otpauth-migration://offline?data=abc")]
    [InlineData("just some text")]
    public void RejectsTextThatIsNotAnOtpAuthLink(string? text)
    {
        Assert.Equal(OtpAuthUriProblem.NotOtpAuth, OtpAuthUri.TryParse(text, out var uri));
        Assert.Null(uri);
    }

    [Theory]
    [InlineData("otpauth://totp/A:b")]
    [InlineData("otpauth://totp/A:b?secret=")]
    [InlineData("otpauth://totp/A:b?secret=NOT*BASE32!")]
    [InlineData("otpauth://totp/A:b?secret=0189")]
    public void RejectsMissingOrInvalidSecrets(string text)
    {
        Assert.Equal(OtpAuthUriProblem.InvalidSecret, OtpAuthUri.TryParse(text, out _));
    }

    [Theory]
    [InlineData("algorithm=SHA256", TotpAlgorithm.Sha256, 6, 30)]
    [InlineData("algorithm=sha512&digits=8", TotpAlgorithm.Sha512, 8, 30)]
    [InlineData("digits=7&period=60", TotpAlgorithm.Sha1, 7, 60)]
    [InlineData("period=300", TotpAlgorithm.Sha1, 6, 300)]
    public void AcceptsNonDefaultParameters(string query, TotpAlgorithm algorithm, int digits, int period)
    {
        Assert.Equal(OtpAuthUriProblem.None, OtpAuthUri.TryParse("otpauth://totp/A:b?secret=JBSWY3DPEHPK3PXP&" + query, out var uri));
        Assert.Equal(new TotpParameters(algorithm, digits, period), uri!.Parameters);
    }

    [Theory]
    [InlineData("otpauth://totp/A:b?secret=JBSWY3DPEHPK3PXP&algorithm=MD5")]
    [InlineData("otpauth://totp/A:b?secret=JBSWY3DPEHPK3PXP&digits=5")]
    [InlineData("otpauth://totp/A:b?secret=JBSWY3DPEHPK3PXP&digits=9")]
    [InlineData("otpauth://totp/A:b?secret=JBSWY3DPEHPK3PXP&digits=abc")]
    [InlineData("otpauth://totp/A:b?secret=JBSWY3DPEHPK3PXP&period=0")]
    [InlineData("otpauth://totp/A:b?secret=JBSWY3DPEHPK3PXP&period=301")]
    [InlineData("otpauth://hotp/A:b?secret=JBSWY3DPEHPK3PXP&counter=1")]
    public void RejectsParametersThatCannotBeGenerated(string text)
    {
        Assert.Equal(OtpAuthUriProblem.UnsupportedParameters, OtpAuthUri.TryParse(text, out _));
    }
}
