using System;

namespace AuthGatun.Domains.IdentityAccess.Application.Models;

public record AuthenticatedUser(Guid Id, string UserName);
