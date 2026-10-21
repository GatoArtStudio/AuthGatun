using System;

namespace AuthGatun.Domains.IdentityAccess.Application.Models;

public record NewKeyRequest(Guid UserId, string ServiceName, string SecretKey);
