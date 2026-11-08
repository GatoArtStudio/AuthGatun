using System;
using AuthGatun.Domains.Settings.Application.Models;
using AuthGatun.Domains.Settings.Application.Ports.Outbound;
using AuthGatun.Domains.Settings.Application.UseCases;
using AuthGatun.Domains.Settings.Domain;
using Xunit;

namespace AuthGatun.Tests.Domains.Settings.Application;

public class SettingsUseCasesTest
{
    private sealed class FakeSettingsRepository : ISettingsRepository
    {
        public AppSettings? Stored { get; set; }
        public AppSettings? Load() => Stored;
        public void Save(AppSettings settings) => Stored = settings;
    }

    private sealed class FakeSystemLanguageProvider(string? language) : ISystemLanguageProvider
    {
        public string? GetSystemLanguage() => language;
    }

    private readonly FakeSettingsRepository _repository = new();

    private GetSettingsUseCase GetSettings(string? systemLanguage = null) =>
        new(_repository, new FakeSystemLanguageProvider(systemLanguage));

    [Fact]
    public void GetReturnsDefaultsWhenNothingWasSaved()
    {
        var settings = GetSettings().Execute();

        Assert.Equal(new AppSettingsModel(false, 5, true, "en"), settings);
    }

    [Theory]
    [InlineData("es", "es")]
    [InlineData("en", "en")]
    [InlineData("fr", "en")]
    [InlineData(null, "en")]
    public void FirstRunUsesTheSystemLanguageWhenSupportedAndEnglishOtherwise(string? system, string expected)
    {
        var settings = GetSettings(system).Execute();

        Assert.Equal(expected, settings.Language);
        Assert.Equal(expected, _repository.Stored!.Language);
    }

    [Fact]
    public void SettingsSavedWithoutALanguageGetOneDetectedOnce()
    {
        _repository.Stored = new AppSettings(true, 15, false, null);

        var settings = GetSettings("es").Execute();

        Assert.Equal(new AppSettingsModel(true, 15, false, "es"), settings);
    }

    [Fact]
    public void ALanguageAlreadyChosenIsNotDetectedAgain()
    {
        _repository.Stored = new AppSettings(false, 5, true, "en");

        Assert.Equal("en", GetSettings("es").Execute().Language);
    }

    [Fact]
    public void UpdateRejectsUnsupportedLanguages()
    {
        Assert.Throws<ArgumentException>(() =>
            new UpdateSettingsUseCase(_repository).Execute(new AppSettingsModel(false, 5, true, "fr")));
        Assert.Null(_repository.Stored);
    }

    [Fact]
    public void UpdateSavesTheSettingsAndGetReturnsThem()
    {
        new UpdateSettingsUseCase(_repository).Execute(new AppSettingsModel(true, 15, false, "es"));

        Assert.Equal(new AppSettingsModel(true, 15, false, "es"), GetSettings().Execute());
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(2)]
    [InlineData(60)]
    public void UpdateRejectsUnsupportedAutoLockTimes(int minutes)
    {
        Assert.Throws<ArgumentException>(() =>
            new UpdateSettingsUseCase(_repository).Execute(new AppSettingsModel(false, minutes, true, "en")));
        Assert.Null(_repository.Stored);
    }

    [Fact]
    public void NewSettingsDefaultToVisibleCodesAndTheDarkTheme()
    {
        var settings = GetSettings().Execute();

        Assert.False(settings.PrivacyMode);
        Assert.Equal("dark", settings.Theme);
    }

    [Fact]
    public void PrivacyModeAndThemeAreSavedAndKeptWhenTheLanguageIsDetected()
    {
        _repository.Stored = new AppSettings(true, 15, false, null, privacyMode: true, theme: "light");

        var settings = GetSettings("es").Execute();

        Assert.True(settings.PrivacyMode);
        Assert.Equal("light", settings.Theme);
        Assert.Equal("light", _repository.Stored!.Theme);
    }

    [Fact]
    public void UpdateRejectsUnsupportedThemes()
    {
        Assert.Throws<ArgumentException>(() =>
            new UpdateSettingsUseCase(_repository).Execute(new AppSettingsModel(false, 5, true, "en", false, "sepia")));
        Assert.Null(_repository.Stored);
    }
}
