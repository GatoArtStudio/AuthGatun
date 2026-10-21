using AuthGatun.Domains.IdentityAccess.Adapters.Outbound.EventBus;
using AuthGatun.Domains.IdentityAccess.Adapters.Outbound.Persistence;
using AuthGatun.Domains.IdentityAccess.Adapters.Outbound.Security;
using AuthGatun.Domains.IdentityAccess.Adapters.Outbound.Otp;
using AuthGatun.Domains.IdentityAccess.Application.Ports.Inbound;
using AuthGatun.Domains.IdentityAccess.Application.Ports.Outbound;
using AuthGatun.Domains.IdentityAccess.Application.UseCases;
using Microsoft.Extensions.DependencyInjection;

namespace AuthGatun.Domains.IdentityAccess;

public static class IdentityAccessDomainRegistration
{
    public static IServiceCollection AddIdentityAccessDomain(
        this IServiceCollection services,
        RepositoryOptions repositoryOptions,
        BusOptions busOptions
    )
    {
        services.AddSingleton(repositoryOptions);
        services.AddSingleton<LegacyDatabaseMigrator>();
        services.AddSingleton<SqlCipherVault>();
        services.AddSingleton<IVaultSession>(sp => sp.GetRequiredService<SqlCipherVault>());
        services.AddSingleton<RepositoryFactory>();
        services.AddSingleton(sp => sp.GetRequiredService<RepositoryFactory>().CreateRepository());

        services.AddSingleton(busOptions);
        services.AddSingleton<BusFactory>();
        services.AddSingleton(sp => sp.GetRequiredService<BusFactory>().CreateBus());
        services.AddSingleton<IDomainEventPublisher>(sp => sp.GetRequiredService<IDomainEventBus>());

        services.AddSingleton<IPasswordHasher, Argon2PasswordHasher>();
        services.AddSingleton<ITotpCodeGenerator, OtpNetTotpCodeGenerator>();

        services.AddTransient<ILoginUseCase, LoginUseCase>();
        services.AddTransient<IRegisterUseCase, RegisterUserUseCase>();
        services.AddTransient<ILogoutUseCase, LogoutUseCase>();
        services.AddTransient<INewKeyUseCase, NewKeyUseCase>();
        services.AddTransient<IDeleteKeyUseCase, DeleteKeyUseCase>();
        services.AddTransient<IReadKeysUseCase, ReadKeysUseCase>();
        services.AddTransient<IGenerateTotpCodeUseCase, GenerateTotpCodeUseCase>();

        return services;
    }
}
