using System;
using System.Collections.Generic;
using System.Linq;
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
    private readonly List<Key> _keys = [];
    private readonly List<IDomainEvent> _uncommittedEvents = [];
    public IReadOnlyList<Key> Keys => _keys;
    public IReadOnlyList<IDomainEvent> UncommittedEvents => _uncommittedEvents;

    public User(Guid id, UserName username, Password password, bool usePassword)
    {
        Id = id;
        Username = username ?? throw new ArgumentNullException(nameof(username), "Username cannot be null");
        Password = password ?? throw new ArgumentNullException(nameof(password), "Password cannot be null");
        UsePassword = usePassword;
    }

    public void ClearEvents() => _uncommittedEvents.Clear();
    
    public void ChangePassword(Password password)
    {
        Password = password ?? throw new ArgumentNullException(nameof(password), "Password cannot be null");
    }

    public void AddKey(Key key)
    {
        _keys.Add(key);
    }
    
    public void RemoveKey(Key key)
    {
        _keys.Remove(key);
    }
    
    public bool UpdateKey(Guid keyId, string serviceName, string? secretKey)
    {
        var index = _keys.FindIndex(k => k.Id == keyId);
        if (index < 0) return false;

        var current = _keys[index];
        _keys[index] = new Key(
            current.Id,
            current.UserId,
            serviceName,
            secretKey ?? current.SecretKey,
            current.Parameters,
            current.IsFavorite,
            current.SortOrder);
        return true;
    }

    public IReadOnlyList<Key> OrderedKeys() =>
        BaseOrder().OrderByDescending(key => key.IsFavorite).ToList();

    public bool SetKeyFavorite(Guid keyId, bool isFavorite)
    {
        var index = _keys.FindIndex(key => key.Id == keyId);
        if (index < 0) return false;

        _keys[index] = _keys[index] with { IsFavorite = isFavorite };
        RenumberKeys(BaseOrder());
        return true;
    }

    public bool MoveKey(Guid keyId, int offset)
    {
        var order = BaseOrder();
        var index = order.FindIndex(key => key.Id == keyId);
        if (index < 0) return false;

        var step = Math.Sign(offset);
        var target = index + step;
        while (target >= 0 && target < order.Count && order[target].IsFavorite != order[index].IsFavorite)
            target += step;
        if (step == 0 || target < 0 || target >= order.Count) return false;

        (order[index], order[target]) = (order[target], order[index]);
        RenumberKeys(order);
        return true;
    }

    private List<Key> BaseOrder() =>
        _keys.Select((key, index) => (key, index))
            .OrderBy(entry => entry.key.SortOrder)
            .ThenBy(entry => entry.index)
            .Select(entry => entry.key)
            .ToList();

    private void RenumberKeys(IReadOnlyList<Key> order)
    {
        for (var position = 0; position < order.Count; position++)
        {
            var index = _keys.FindIndex(key => key.Id == order[position].Id);
            _keys[index] = order[position] with { SortOrder = position };
        }
    }

    public void AddKeys(List<Key> keys)
    {
        _keys.AddRange(keys);
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