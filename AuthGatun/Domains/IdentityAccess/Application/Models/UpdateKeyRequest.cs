using System;

namespace AuthGatun.Domains.IdentityAccess.Application.Models;

public record UpdateKeyRequest(Guid UserId, Guid KeyId, string ServiceName, string? SecretKey);
