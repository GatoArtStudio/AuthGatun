using System;
using System.Collections.Generic;
using AuthGatun.Domains.IdentityAccess.Domain.Event;
using AuthGatun.Domains.IdentityAccess.Domain.Model;
using AuthGatun.Domains.IdentityAccess.Domain.ValueObject;

namespace AuthGatun.Domains.IdentityAccess.Domain.Aggregate;

public class User
{
    public Guid Id { get; }
    public UserName Username { get; private set; }
    public Password Password { get; private set; }
    public bool UsePassword { get; private set; }
    public List<Key> Keys { get; private set; } = [];
    public List<IDomainEvent> UncommittedEvents { get; private set; } = [];

    public User(Guid id, UserName username, Password password, bool usePassword)
    {
        Id = id;
        Username = username ?? throw new ArgumentNullException(nameof(username), "Username cannot be null");
        Password = password ?? throw new ArgumentNullException(nameof(password), "Password cannot be null");
        UsePassword = usePassword;
    }

    public void ClearEvents() => UncommittedEvents.Clear();
    
    public void AddKey(Key key)
    {
        Keys.Add(key);
    }
    
    public void RemoveKey(Key key)
    {
        Keys.Remove(key);
    }
    
    public void AddKeys(List<Key> keys)
    {
        Keys.AddRange(keys);
    }

    protected bool Equals(User other)
    {
        return Id.Equals(other.Id);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != typeof(User)) return false;
        return Equals((User)obj);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}