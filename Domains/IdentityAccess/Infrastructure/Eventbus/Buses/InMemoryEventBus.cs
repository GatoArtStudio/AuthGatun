using System;
using System.Collections.Generic;
using AuthGatun.Domains.IdentityAccess.Application.Ports.In;
using AuthGatun.Domains.IdentityAccess.Domain.Event;

namespace AuthGatun.Domains.IdentityAccess.Infrastructure.Eventbus.Buses;

public class InMemoryEventBus : IDomainEventBus
{
    private readonly Dictionary<Type, List<object>> _subscribers = new();
    
    public void Publish(IDomainEvent @event)
    {
        var eventType = @event.GetType();

        if (!_subscribers.TryGetValue(eventType, out var subscribers))
            return;
        
        foreach (var subscriber in subscribers)
        {
            ((IDomainEventSubscriber<IDomainEvent>)subscriber).Handle(@event);
        }
    }

    public void Register<T>(IDomainEventSubscriber<T> subscriber) where T : IDomainEvent
    {
        var eventType = typeof(T);
        
        if (!_subscribers.ContainsKey(eventType))
            _subscribers[eventType] = new List<object>();
        
        _subscribers[eventType].Add(subscriber);
    }
}