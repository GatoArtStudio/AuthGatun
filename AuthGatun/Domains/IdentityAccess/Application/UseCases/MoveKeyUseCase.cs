using System;
using System.Linq;
using AuthGatun.Domains.IdentityAccess.Application.Models;
using AuthGatun.Domains.IdentityAccess.Application.Ports.Inbound;
using AuthGatun.Domains.IdentityAccess.Application.Ports.Outbound;

namespace AuthGatun.Domains.IdentityAccess.Application.UseCases;

public class MoveKeyUseCase(IUserRepository repository) : IMoveKeyUseCase
{
    public bool Execute(Guid keyId, KeyMoveDirection direction)
    {
        var user = repository.ReadAll().FirstOrDefault(u => u.Keys.Any(k => k.Id == keyId));
        if (user is null)
            throw new IdentityAccessException(IdentityAccessError.KeyNotFound, "The key does not exist.");

        if (!user.MoveKey(keyId, direction == KeyMoveDirection.Up ? -1 : 1))
            return false;

        if (!repository.Save(user))
            throw new IdentityAccessException(IdentityAccessError.SaveFailed, "The key order could not be saved.");
        return true;
    }
}
