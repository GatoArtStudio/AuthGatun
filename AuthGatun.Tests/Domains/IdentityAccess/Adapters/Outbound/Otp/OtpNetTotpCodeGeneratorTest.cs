using System;
using System.Text;
using AuthGatun.Domains.IdentityAccess.Adapters.Outbound.Otp;
using AuthGatun.Domains.IdentityAccess.Domain.ValueObject;
using OtpNet;
using Xunit;

namespace AuthGatun.Tests.Domains.IdentityAccess.Adapters.Outbound.Otp;

public class OtpNetTotpCodeGeneratorTest
{
    private const string Sha1Seed = "12345678901234567890";
    private const string Sha256Seed = "12345678901234567890123456789012";
    private const string Sha512Seed = "1234567890123456789012345678901234567890123456789012345678901234";

    private readonly OtpNetTotpCodeGenerator _generator = new();

    [Theory]
    [InlineData(TotpAlgorithm.Sha1, Sha1Seed, 59, "94287082")]
    [InlineData(TotpAlgorithm.Sha256, Sha256Seed, 59, "46119246")]
    [InlineData(TotpAlgorithm.Sha512, Sha512Seed, 59, "90693936")]
    [InlineData(TotpAlgorithm.Sha1, Sha1Seed, 1111111109, "07081804")]
    [InlineData(TotpAlgorithm.Sha256, Sha256Seed, 1111111109, "68084774")]
    [InlineData(TotpAlgorithm.Sha512, Sha512Seed, 1111111109, "25091201")]
    [InlineData(TotpAlgorithm.Sha1, Sha1Seed, 1234567890, "89005924")]
    [InlineData(TotpAlgorithm.Sha256, Sha256Seed, 1234567890, "91819424")]
    [InlineData(TotpAlgorithm.Sha512, Sha512Seed, 1234567890, "93441116")]
    public void MatchesTheRfc6238Vectors(TotpAlgorithm algorithm, string seed, long unixTime, string expected)
    {
        var secret = Base32Encoding.ToString(Encoding.ASCII.GetBytes(seed));
        var at = DateTimeOffset.FromUnixTimeSeconds(unixTime).UtcDateTime;

        Assert.Equal(expected, _generator.Generate(secret, new TotpParameters(algorithm, 8, 30), at));
    }

    [Fact]
    public void ProducesSixDigitsByDefault()
    {
        var secret = Base32Encoding.ToString(Encoding.ASCII.GetBytes(Sha1Seed));
        var at = DateTimeOffset.FromUnixTimeSeconds(59).UtcDateTime;

        Assert.Equal("287082", _generator.Generate(secret, TotpParameters.Default, at));
    }

    [Theory]
    [InlineData(30, 100, 20)]
    [InlineData(60, 100, 20)]
    [InlineData(60, 120, 60)]
    [InlineData(10, 59, 1)]
    public void RemainingSecondsFollowsThePeriodOfTheKey(int period, long unixTime, int expected)
    {
        var parameters = new TotpParameters(TotpAlgorithm.Sha1, 6, period);

        Assert.Equal(expected, _generator.RemainingSeconds(parameters, DateTimeOffset.FromUnixTimeSeconds(unixTime)));
    }
}
