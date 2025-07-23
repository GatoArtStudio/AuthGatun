using System;

namespace AuthGatun.Domains.IdentityAccess.Domain.ValueObject;

public class Password
{
    public readonly string Value;

    public Password(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be null or whitespace");

        if (password.Length < 8)
            throw new ArgumentException("Password must be at least 8 characters long");

        Value = password;
    }

    protected bool Equals(Password other)
    {
        return Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != typeof(Password)) return false;
        return Equals((Password)obj);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }
}