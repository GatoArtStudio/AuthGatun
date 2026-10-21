using System;
using System.IO;
using AuthGatun.Domains.Settings;
using AuthGatun.Domains.Settings.Adapters.Outbound.JsonFile;
using AuthGatun.Domains.Settings.Application.Ports.Inbound;
using AuthGatun.Domains.Settings.Domain;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AuthGatun.Tests.Domains.Settings.Adapters;

public class JsonFileSettingsRepositoryTest : IDisposable
{
    private readonly string _appFolder = Path.Combine(Path.GetTempPath(), "authgatun-tests-" + Guid.NewGuid());
    private string SettingsPath => Path.Combine(_appFolder, "AuthGatun", "settings.json");

    public void Dispose()
    {
        if (Directory.Exists(_appFolder))
            Directory.Delete(_appFolder, true);
    }

    [Fact]
    public void LoadReturnsNullWhenTheFileDoesNotExist()
    {
        Assert.Null(new JsonFileSettingsRepository(SettingsPath).Load());
    }

    [Fact]
    public void SavedSettingsAreLoadedBack()
    {
        var repository = new JsonFileSettingsRepository(SettingsPath);

        repository.Save(new AppSettings(true, 30));

        Assert.Equal(new AppSettings(true, 30), repository.Load());
    }

    [Theory]
    [InlineData("not json at all")]
    [InlineData("{\"RunInBackground\":true,\"AutoLockMinutes\":7}")]
    public void LoadReturnsNullWhenTheFileIsCorruptOrInvalid(string content)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
        File.WriteAllText(SettingsPath, content);

        Assert.Null(new JsonFileSettingsRepository(SettingsPath).Load());
    }

    [Fact]
    public void EveryUseCaseCanBeResolved()
    {
        using var provider = new ServiceCollection()
            .AddSettingsDomain(_appFolder)
            .BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true });

        Assert.NotNull(provider.GetRequiredService<IGetSettingsUseCase>());
        Assert.NotNull(provider.GetRequiredService<IUpdateSettingsUseCase>());
    }
}
