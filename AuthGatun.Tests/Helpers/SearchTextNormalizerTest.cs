using AuthGatun.Helpers;
using Xunit;

namespace AuthGatun.Tests.Helpers;

public class SearchTextNormalizerTest
{
    [Theory]
    [InlineData("Café", "cafe")]
    [InlineData("  ÑANDÚ ", "nandu")]
    [InlineData("GitHub", "github")]
    [InlineData(null, "")]
    [InlineData("   ", "")]
    public void NormalizeRemovesAccentsCaseAndPadding(string? input, string expected) =>
        Assert.Equal(expected, SearchTextNormalizer.Normalize(input));

    [Theory]
    [InlineData("Café Central", "cafe", true)]
    [InlineData("Cafe", "cafe", true)]
    [InlineData("GitHub", "", true)]
    [InlineData("GitHub", "gitlab", false)]
    public void MatchesComparesAgainstTheNormalizedQuery(string text, string query, bool expected) =>
        Assert.Equal(expected, SearchTextNormalizer.Matches(text, SearchTextNormalizer.Normalize(query)));
}
