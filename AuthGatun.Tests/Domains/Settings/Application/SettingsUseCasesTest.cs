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

    private readonly FakeSettingsRepository _repository = new();

    [Fact]
    public void GetReturnsDefaultsWhenNothingWasSaved()
    {
        var settings = new GetSettingsUseCase(_repository).Execute();

        Assert.Equal(new AppSettingsModel(false, 5), settings);
    }

    [Fact]
    public void UpdateSavesTheSettingsAndGetReturnsThem()
    {
        new UpdateSettingsUseCase(_repository).Execute(new AppSettingsModel(true, 15));

        Assert.Equal(new AppSettingsModel(true, 15), new GetSettingsUseCase(_repository).Execute());
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(2)]
    [InlineData(60)]
    public void UpdateRejectsUnsupportedAutoLockTimes(int minutes)
    {
        Assert.Throws<ArgumentException>(() =>
            new UpdateSettingsUseCase(_repository).Execute(new AppSettingsModel(false, minutes)));
        Assert.Null(_repository.Stored);
    }
}
