using System;
using System.IO;
using AuthGatun.Domains.IdentityAccess;
using AuthGatun.Domains.IdentityAccess.Adapters.Outbound.EventBus;
using AuthGatun.Domains.IdentityAccess.Adapters.Outbound.Persistence;
using AuthGatun.Domains.IdentityAccess.Application.Ports.Inbound;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AuthGatun.Tests.Domains.IdentityAccess;

public class IdentityAccessDomainRegistrationTest : IDisposable
{
    private readonly string _appFolder = Path.Combine(Path.GetTempPath(), "authgatun-tests-" + Guid.NewGuid());

    [Theory]
    [InlineData(typeof(ILoginUseCase))]
    [InlineData(typeof(IRegisterUseCase))]
    [InlineData(typeof(ILogoutUseCase))]
    [InlineData(typeof(INewKeyUseCase))]
    [InlineData(typeof(IDeleteKeyUseCase))]
    [InlineData(typeof(IReadKeysUseCase))]
    [InlineData(typeof(IGenerateTotpCodeUseCase))]
    [InlineData(typeof(IChangePasswordUseCase))]
    [InlineData(typeof(IRecoverAccessUseCase))]
    [InlineData(typeof(IRegenerateRecoveryCodeUseCase))]
    [InlineData(typeof(IExportKeysUseCase))]
    [InlineData(typeof(IImportKeysUseCase))]
    public void EveryUseCaseCanBeResolved(Type useCase)
    {
        var services = new ServiceCollection().AddIdentityAccessDomain(
            new RepositoryOptions(TypeRepository.SqLite, _appFolder, null, null, null, null, null),
            new BusOptions(TypeBus.Default));

        using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true });

        Assert.NotNull(provider.GetRequiredService(useCase));
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        if (Directory.Exists(_appFolder))
            Directory.Delete(_appFolder, true);
    }
}
