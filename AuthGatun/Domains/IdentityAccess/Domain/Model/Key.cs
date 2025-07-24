using System;

namespace AuthGatun.Domains.IdentityAccess.Domain.Model;

public record Key
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string ServiceName { get; init; }
    public string SecretKey { get; init; }

    public Key(Guid id, Guid userId, string serviceName, string secretKey)
    {
        if (string.IsNullOrWhiteSpace(serviceName))
            throw new ArgumentException("Service name cannot be null or whitespace", nameof(serviceName));
        
        if (string.IsNullOrWhiteSpace(secretKey))
            throw new ArgumentException("Secret key cannot be null or whitespace", nameof(secretKey));
        
        Id = id;
        UserId = userId;
        ServiceName = serviceName;
        SecretKey = secretKey;
    }

    public virtual bool Equals(Key? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id.Equals(other.Id);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
};