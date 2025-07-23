using System;

namespace AuthGatun.Domains.IdentityAccess.Domain.ValueObject;

public class UserName
{
    public readonly string Value;

    public UserName(string userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
            throw new ArgumentException("User name cannot be null or whitespace");

        Value = userName;
    }

    protected bool Equals(UserName other)
    {
        return Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != typeof(UserName)) return false;
        return Equals((UserName)obj);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }
}