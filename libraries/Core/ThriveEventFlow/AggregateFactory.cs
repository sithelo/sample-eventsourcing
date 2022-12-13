
using System.Collections.Concurrent;

namespace ThriveEventFlow;


public class AggregateFactoryRegistry {
   
    public static readonly AggregateFactoryRegistry Instance = new();

    readonly ConcurrentDictionary<Type, Func<dynamic>> _registry = new();

    public AggregateFactoryRegistry CreateAggregateUsing<T>(AggregateFactory<T> factory)
        where T : Aggregate {
        _registry.TryAdd(typeof(T), () => factory());
        return this;
    }

    public void UnsafeCreateAggregateUsing<T>(Type type, Func<T> factory)
        where T : Aggregate
        => _registry.TryAdd(type, factory);

    internal T CreateInstance<T, TState>()
        where T : Aggregate<TState>
        where TState : State<TState>, new() {
        var instance = _registry.TryGetValue(typeof(T), out var factory)
            ? (T)factory()
            : Activator.CreateInstance<T>();

        return instance;
    }

    internal T CreateInstance<T>() where T : Aggregate {
        var instance = _registry.TryGetValue(typeof(T), out var factory)
            ? (T)factory()
            : Activator.CreateInstance<T>();

        return instance;
    }
}

public delegate T AggregateFactory<out T>() where T : Aggregate;
