using System;
using AuthGatun.Domains.IdentityAccess.Domain.Model;
using AuthGatun.Domains.IdentityAccess.Domain.ValueObject;
using Xunit;

namespace AuthGatun.Tests.Domains.IdentityAccess.Domain;

public class TotpParametersTest
{
    [Fact]
    public void DefaultIsSha1SixDigitsThirtySeconds()
    {
        Assert.Equal(new TotpParameters(TotpAlgorithm.Sha1, 6, 30), TotpParameters.Default);
    }

    [Theory]
    [InlineData(5, 30)]
    [InlineData(9, 30)]
    [InlineData(6, 0)]
    [InlineData(6, 301)]
    public void RejectsOutOfRangeValues(int digits, int period)
    {
        Assert.Throws<ArgumentException>(() => new TotpParameters(TotpAlgorithm.Sha1, digits, period));
    }

    [Fact]
    public void KeysUseTheDefaultParametersUnlessGivenOthers()
    {
        var plain = new Key(Guid.NewGuid(), Guid.NewGuid(), "github", "JBSWY3DPEHPK3PXP");
        var custom = new Key(Guid.NewGuid(), Guid.NewGuid(), "bank", "JBSWY3DPEHPK3PXP", new TotpParameters(TotpAlgorithm.Sha512, 8, 30));

        Assert.Equal(TotpParameters.Default, plain.Parameters);
        Assert.False(plain.IsFavorite);
        Assert.Equal(0, plain.SortOrder);
        Assert.Equal(TotpAlgorithm.Sha512, custom.Parameters.Algorithm);
    }
}
