using System;

namespace AuthGatun.Domains.IdentityAccess.Application;

public class IdentityAccessException(IdentityAccessError error, string message) : Exception(message)
{
    public IdentityAccessError Error { get; } = error;
}
