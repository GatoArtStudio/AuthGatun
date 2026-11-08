using System;

namespace AuthGatun.Domains.IdentityAccess.Application.Ports.Inbound;

public interface ISetKeyFavoriteUseCase
{
    void Execute(Guid keyId, bool isFavorite);
}
