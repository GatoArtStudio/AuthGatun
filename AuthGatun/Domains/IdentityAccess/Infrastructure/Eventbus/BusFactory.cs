using System;
using System.Collections.Concurrent;
using AuthGatun.Domains.IdentityAccess.Domain.Event;
using AuthGatun.Domains.IdentityAccess.Infrastructure.Enums;
using AuthGatun.Domains.IdentityAccess.Infrastructure.Eventbus.Buses;
using AuthGatun.Domains.IdentityAccess.Infrastructure.Eventbus.Model;

namespace AuthGatun.Domains.IdentityAccess.Infrastructure.Eventbus;

public class BusFactory
{
    private static BusFactory? _instance;
    private static readonly object Lock = new object();
    
    private readonly BusOptions _options;
    private readonly ConcurrentDictionary<TypeBus, IDomainEventBus> _buses = new();

    private BusFactory(BusOptions options)
    {
        _options = options;
    }

    public static BusFactory GetInstance()
    {
        return GetInstance(null);
    }

    public static BusFactory GetInstance(BusOptions? options)
    {
        if (_instance == null)
        {
            lock (Lock)
            {
                if (_instance == null)
                {
                    if (options == null)
                        throw new ArgumentNullException(nameof(options), "Bus options cannot be null.");
                    
                    _instance = new BusFactory(options);
                }
            }
        }

        return _instance;
    }

    public IDomainEventBus CreateBus()
    {
        if (_options.Type is TypeBus.Default or TypeBus.InMemory)
        {
            if (_buses.TryGetValue(_options.Type, out var existingBus))
            {
                return existingBus;
            }
            
            IDomainEventBus bus = new InMemoryEventBus();
            _buses.TryAdd(TypeBus.Default, bus);
            _buses.TryAdd(TypeBus.InMemory, bus);
            return bus;
        }

        return _buses[TypeBus.Default];
    }
}