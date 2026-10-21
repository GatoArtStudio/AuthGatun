using System;
using System.Linq;
using AuthGatun.Domains.IdentityAccess.Application.Ports.Inbound;
using AuthGatun.Domains.IdentityAccess.Application.Ports.Outbound;

namespace AuthGatun.Domains.IdentityAccess.Application.UseCases;

public class GenerateTotpCodeUseCase(IUserRepository repository, ITotpCodeGenerator totpCodeGenerator)
    : IGenerateTotpCodeUseCase
{
    public string? Execute(Guid keyId)
    {
        var key = repository.ReadAll()
            .SelectMany(user => user.Keys)
            .FirstOrDefault(k => k.Id == keyId);

        return key is null ? null : totpCodeGenerator.Generate(key.SecretKey);
    }
}
