using System;
using AuthGatun.Services;
using Xunit;

namespace AuthGatun.Tests.Services;

public class CodePrivacyTest
{
    private sealed class FakeClock : IClock
    {
        public DateTime UtcNow { get; set; } = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    }

    private const string Code = "123 456";

    [Fact]
    public void ModeOffAlwaysShowsCode()
    {
        var privacy = new CodePrivacy(new FakeClock());
        Assert.Equal(Code, privacy.Display(Code));
    }

    [Fact]
    public void ModeOnMasksByDefault()
    {
        var privacy = new CodePrivacy(new FakeClock(), true);
        Assert.Equal(CodePrivacy.Mask, privacy.Display(Code));
    }

    [Fact]
    public void HoverRevealsAndLeavingHides()
    {
        var privacy = new CodePrivacy(new FakeClock(), true);
        privacy.PointerEntered();
        Assert.Equal(Code, privacy.Display(Code));
        privacy.PointerLeft();
        Assert.Equal(CodePrivacy.Mask, privacy.Display(Code));
    }

    [Fact]
    public void ToggleRevealsThenAutoHidesAfterTimeout()
    {
        var clock = new FakeClock();
        var privacy = new CodePrivacy(clock, true);
        privacy.ToggleReveal();
        clock.UtcNow += CodePrivacy.RevealDuration - TimeSpan.FromSeconds(1);
        Assert.True(privacy.IsRevealed);
        clock.UtcNow += TimeSpan.FromSeconds(2);
        Assert.False(privacy.IsRevealed);
    }

    [Fact]
    public void TogglingTwiceHides()
    {
        var privacy = new CodePrivacy(new FakeClock(), true);
        privacy.ToggleReveal();
        privacy.ToggleReveal();
        Assert.False(privacy.IsRevealed);
    }

    [Fact]
    public void DisplayDoesNotRevealOnItsOwn()
    {
        var privacy = new CodePrivacy(new FakeClock(), true);
        privacy.Display(Code);
        Assert.False(privacy.IsRevealed);
    }

    [Fact]
    public void TurningModeOnHidesARevealedCode()
    {
        var privacy = new CodePrivacy(new FakeClock(), true);
        privacy.ToggleReveal();
        privacy.SetEnabled(false);
        Assert.True(privacy.IsRevealed);
        privacy.SetEnabled(true);
        Assert.False(privacy.IsRevealed);
    }
}
