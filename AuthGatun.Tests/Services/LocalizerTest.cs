using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AuthGatun.Domains.Settings.Application.Models;
using AuthGatun.Domains.Settings.Application.Ports.Inbound;
using AuthGatun.Services;
using AuthGatun.Services.Localization;
using Xunit;

namespace AuthGatun.Tests.Services;

public class LocalizerTest
{
    private sealed class FakeGetSettings(string language) : IGetSettingsUseCase
    {
        public AppSettingsModel Execute() => new(false, 5, true, language);
    }

    private sealed class FakeUpdateSettings : IUpdateSettingsUseCase
    {
        public AppSettingsModel Execute(AppSettingsModel settings) => settings;
    }

    private static AppSettingsState State(string language) => new(new FakeGetSettings(language), new FakeUpdateSettings());

    private static IEnumerable<string> Placeholders(string text) =>
        Regex.Matches(text, @"\{\d+\}").Select(match => match.Value).OrderBy(value => value);

    [Fact]
    public void BothLanguagesDefineTheSameKeys()
    {
        Assert.Equal(
            StringsEnglish.Table.Keys.OrderBy(key => key),
            StringsSpanish.Table.Keys.OrderBy(key => key));
    }

    [Fact]
    public void BothLanguagesUseTheSamePlaceholders()
    {
        foreach (var (key, english) in StringsEnglish.Table)
            Assert.Equal(Placeholders(english), Placeholders(StringsSpanish.Table[key]));
    }

    [Fact]
    public void EveryIdentityAccessErrorHasATranslation()
    {
        foreach (var error in System.Enum.GetNames<AuthGatun.Domains.IdentityAccess.Application.IdentityAccessError>())
            Assert.True(StringsEnglish.Table.ContainsKey("Error_" + error), error);
    }

    [Fact]
    public void ReturnsTextInTheConfiguredLanguage()
    {
        Assert.Equal("Sign in", new Localizer(State("en")).Get("Login_SignIn"));
        Assert.Equal("Iniciar sesión", new Localizer(State("es")).Get("Login_SignIn"));
    }

    [Fact]
    public void FormatsArguments()
    {
        Assert.Equal("Too many attempts, wait 5 seconds", new Localizer(State("en")).Get("Login_Throttled", 5));
    }

    [Fact]
    public void UnknownKeysReturnTheKey()
    {
        Assert.Equal("Missing_Key", new Localizer(State("en")).Get("Missing_Key"));
    }
}
