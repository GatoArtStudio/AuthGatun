using AuthGatun.Services;
using Avalonia.Styling;
using Xunit;

namespace AuthGatun.Tests.Services;

public class ThemeServiceTest
{
    [Theory]
    [InlineData("dark")]
    [InlineData(null)]
    [InlineData("unknown")]
    public void DarkIsTheFallback(string? code) => Assert.Equal(ThemeVariant.Dark, ThemeService.ResolveVariant(code));

    [Fact]
    public void LightMapsToLight() => Assert.Equal(ThemeVariant.Light, ThemeService.ResolveVariant("light"));

    [Fact]
    public void SystemMapsToDefault() => Assert.Equal(ThemeVariant.Default, ThemeService.ResolveVariant("system"));
}
