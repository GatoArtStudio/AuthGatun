using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AuthGatun.Domains.IdentityAccess.Application.Ports.Out;
using AuthGatun.Domains.IdentityAccess.Domain.Aggregate;

namespace AuthGatun.Domains.IdentityAccess.Infrastructure.Persistence.Repositories;

public class InMemoryRepository : IUserRepository
{
    private readonly ConcurrentDictionary<Guid, User> _data = new ConcurrentDictionary<Guid, User>();
    
    public User? Read([NotNull] Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentNullException(nameof(id));
        }
        return _data.ContainsKey(id) ? _data[id] : null;
    }

    public bool Delete([NotNull] Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentNullException(nameof(id));
        }
        if (!_data.ContainsKey(id))
        {
            throw new KeyNotFoundException();
        }
        return _data.Remove(id, out _);
    }

    public bool Save([NotNull] User entity)
    {
        if (entity is null)
        {
            throw new ArgumentNullException(nameof(entity));
        }
        _data[entity.Id] = entity;
        return true;
    }

    public List<User> ReadAll()
    {
        return new List<User>(_data.Values);
    }

    public User? FromName(string userName)
    {
        return _data
            .Values
            .FirstOrDefault(usr => usr.Username.Value.Equals(userName));
    }
}