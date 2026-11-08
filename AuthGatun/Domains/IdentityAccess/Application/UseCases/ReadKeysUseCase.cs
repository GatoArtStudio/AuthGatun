using System;
using System.Collections.Generic;
using System.Linq;
using AuthGatun.Domains.IdentityAccess.Application.Models;
using AuthGatun.Domains.IdentityAccess.Application.Ports.Inbound;
using AuthGatun.Domains.IdentityAccess.Application.Ports.Outbound;

namespace AuthGatun.Domains.IdentityAccess.Application.UseCases;

public class ReadKeysUseCase(IUserRepository repository) : IReadKeysUseCase
{
    public IReadOnlyList<KeySummary> Execute(Guid userId)
    {
        var user = repository.Read(userId);
        if (user is null)
            throw new IdentityAccessException(IdentityAccessError.UserNotFound, "The user is invalid or does not exist.");

        return user.Keys
            .Select(key => new KeySummary(
                key.Id,
                key.ServiceName,
                new TotpParametersModel(key.Parameters.Algorithm.ToString().ToUpperInvariant(), key.Parameters.Digits, key.Parameters.PeriodSeconds),
                key.IsFavorite,
                key.SortOrder))
            .ToList();
    }
}
