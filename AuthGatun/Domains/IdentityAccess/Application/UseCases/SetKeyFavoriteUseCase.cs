using System;
using System.Linq;
using AuthGatun.Domains.IdentityAccess.Application.Ports.Inbound;
using AuthGatun.Domains.IdentityAccess.Application.Ports.Outbound;

namespace AuthGatun.Domains.IdentityAccess.Application.UseCases;

public class SetKeyFavoriteUseCase(IUserRepository repository) : ISetKeyFavoriteUseCase
{
    public void Execute(Guid keyId, bool isFavorite)
    {
        var user = repository.ReadAll().FirstOrDefault(u => u.Keys.Any(k => k.Id == keyId));
        if (user is null || !user.SetKeyFavorite(keyId, isFavorite))
            throw new IdentityAccessException(IdentityAccessError.KeyNotFound, "The key does not exist.");

        if (!repository.Save(user))
            throw new IdentityAccessException(IdentityAccessError.SaveFailed, "The key could not be saved.");
    }
}
