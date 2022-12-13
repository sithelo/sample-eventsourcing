// ReSharper disable CheckNamespace

using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ThriveEventFlow;

namespace Microsoft.Extensions.DependencyInjection;

[PublicAPI]
public static class AggregateFactoryContainerExtensions {
    
    public static IServiceCollection AddAggregate<T>(
        this IServiceCollection   services,
        Func<IServiceProvider, T> createInstance
    ) where T : Aggregate {
        services.TryAddSingleton<AggregateFactoryRegistry>();
        services.AddSingleton(new ResolveAggregateFactory(typeof(T), createInstance));
        return services;
    }

    /// <summary>
    /// Add a default aggregate factory to the container, allowing to resolve aggregate dependencies.
    /// Do not use this if your aggregate has no dependencies and has a parameterless constructor.
    /// Must be followed by builder.UseAggregateFactory() in Startup.Configure.
    /// </summary>
    /// <param name="services"></param>
    /// <typeparam name="T">Aggregate type</typeparam>
    /// <returns></returns>
    public static IServiceCollection AddAggregate<T>(this IServiceCollection services) where T : Aggregate {
        services.TryAddSingleton<AggregateFactoryRegistry>();
        services.AddTransient<T>();
        // ReSharper disable once ConvertToLocalFunction
        Func<IServiceProvider, T> createInstance = sp => sp.GetRequiredService<T>();
        return services.AddSingleton(new ResolveAggregateFactory(typeof(T), createInstance));
    }
}

public record ResolveAggregateFactory(Type Type, Func<IServiceProvider, Aggregate> CreateInstance);