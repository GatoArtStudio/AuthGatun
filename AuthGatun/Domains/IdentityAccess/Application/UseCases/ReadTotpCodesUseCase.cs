using System;
using System.Collections.Generic;
using System.Linq;
using AuthGatun.Domains.IdentityAccess.Application.Models;
using AuthGatun.Domains.IdentityAccess.Application.Ports.Inbound;
using AuthGatun.Domains.IdentityAccess.Application.Ports.Outbound;

namespace AuthGatun.Domains.IdentityAccess.Application.UseCases;

public class ReadTotpCodesUseCase(IUserRepository repository, ITotpCodeGenerator totpCodeGenerator)
    : IReadTotpCodesUseCase
{
    public IReadOnlyList<TotpCodeModel> Execute(Guid userId)
    {
        var user = repository.Read(userId);
        if (user is null)
            throw new IdentityAccessException(IdentityAccessError.UserNotFound, "The user is invalid or does not exist.");

        return user.Keys
            .Select(key => new TotpCodeModel(
                key.Id,
                key.ServiceName,
                totpCodeGenerator.Generate(key.SecretKey, key.Parameters),
                totpCodeGenerator.RemainingSeconds(key.Parameters),
                key.Parameters.PeriodSeconds,
                key.IsFavorite,
                key.SortOrder))
            .ToList();
    }
}
