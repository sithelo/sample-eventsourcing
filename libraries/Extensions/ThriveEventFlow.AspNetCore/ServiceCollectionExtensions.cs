// ReSharper disable CheckNamespace

using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ThriveEventFlow;
using ThriveEventFlow.Diagnostics;
using ThriveEventFlow.Diagnostics.Tracing;

namespace Microsoft.Extensions.DependencyInjection;

[PublicAPI]
public static class ServiceCollectionExtensions {
   
    public static IServiceCollection AddApplicationService<T, TAggregate>(this IServiceCollection services)
        where T : class, IApplicationService<TAggregate>
        where TAggregate : Aggregate {
        services.TryAddSingleton<AggregateFactoryRegistry>();
        services.AddSingleton<T>();

        if (ThriveEventFlowDiagnostics.Enabled) {
            services.AddSingleton(
                sp => TracedApplicationService<TAggregate>.Trace(sp.GetRequiredService<T>())
            );
        }
        else {
            services.AddSingleton<IApplicationService<TAggregate>>(sp => sp.GetRequiredService<T>());
        }

        return services;
    }

    
    public static IServiceCollection AddApplicationService<T, TAggregate, TState, TId>(
        this IServiceCollection services,
        bool                    throwOnError = false
    )
        where T : class, IApplicationService<TAggregate, TState, TId>
        where TState : State<TState>, new()
        where TId : AggregateId
        where TAggregate : Aggregate<TState> {
        services.TryAddSingleton<AggregateFactoryRegistry>();
        services.AddSingleton<T>();

        services.AddSingleton(sp => GetThrowingService(GetTracedService(sp)));

        return services;

        IApplicationService<TAggregate, TState, TId> GetThrowingService(
            IApplicationService<TAggregate, TState, TId> inner
        )
            => throwOnError
                ? new ThrowingApplicationService<TAggregate, TState, TId>(inner)
                : inner;

        IApplicationService<TAggregate, TState, TId> GetTracedService(IServiceProvider serviceProvider)
            => ThriveEventFlowDiagnostics.Enabled
                ? TracedApplicationService<TAggregate, TState, TId>.Trace(serviceProvider.GetRequiredService<T>())
                : serviceProvider.GetRequiredService<T>();
    }

    
    public static IServiceCollection AddApplicationService<T, TAggregate>(
        this IServiceCollection   services,
        Func<IServiceProvider, T> getService
    )
        where T : class, IApplicationService<TAggregate>
        where TAggregate : Aggregate {
        services.TryAddSingleton<AggregateFactoryRegistry>();
        services.AddSingleton(getService);

        if (ThriveEventFlowDiagnostics.Enabled) {
            services.AddSingleton(
                sp => TracedApplicationService<TAggregate>.Trace(sp.GetRequiredService<T>())
            );
        }
        else {
            services.AddSingleton<IApplicationService<TAggregate>>(sp => sp.GetRequiredService<T>());
        }

        return services;
    }

   
    public static IServiceCollection AddAggregateStore<T>(this IServiceCollection services)
        where T : class, IEventStore {
        services.TryAddSingleton<AggregateFactoryRegistry>();

        if (ThriveEventFlowDiagnostics.Enabled) {
            services
                .AddSingleton<T>()
                .AddSingleton(sp => TracedEventStore.Trace(sp.GetRequiredService<T>()));
        }
        else {
            services.AddSingleton<IEventStore, T>();
        }

        services.AddSingleton<IAggregateStore, AggregateStore>();
        return services;
    }

   
    public static IServiceCollection AddAggregateStore<T>(
        this IServiceCollection   services,
        Func<IServiceProvider, T> getService
    ) where T : class, IEventStore {
        services.TryAddSingleton<AggregateFactoryRegistry>();

        if (ThriveEventFlowDiagnostics.Enabled) {
            services
                .AddSingleton(getService)
                .AddSingleton(sp => TracedEventStore.Trace(sp.GetRequiredService<T>()));
        }
        else {
            services.AddSingleton<IEventStore>(getService);
        }

        services.AddSingleton<IAggregateStore, AggregateStore>();
        return services;
    }

    public static IServiceCollection AddAggregateStore<T, TArchive>(this IServiceCollection services)
        where T : class, IEventStore
        where TArchive : class, IEventReader {
        services.TryAddSingleton<AggregateFactoryRegistry>();
        
        if (ThriveEventFlowDiagnostics.Enabled) {
            services
                .AddSingleton<T>()
                .AddSingleton(sp => TracedEventStore.Trace(sp.GetRequiredService<T>()));
        }
        else {
            services.AddSingleton<IEventStore, T>();
        }

        services.AddSingleton<TArchive>();
        services.AddSingleton<IAggregateStore, AggregateStore<TArchive>>();

        return services;
    }
}
