
using ThriveEventFlow.Diagnostics;
using ThriveEventFlow.Subscriptions;
using ThriveEventFlow.Subscriptions.Checkpoints;
using ThriveEventFlow.Subscriptions.Diagnostics;
using ThriveEventFlow.Subscriptions.Registrations;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ThriveEventFlow;
using ThriveEventFlow.Subscriptions.Checkpoints;
using ThriveEventFlow.Subscriptions.Registrations;

// ReSharper disable CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

[PublicAPI]
public static class SubscriptionRegistrationExtensions {
    public static IServiceCollection AddSubscription<T, TOptions>(
        this IServiceCollection                  services,
        string                                   subscriptionId,
        Action<SubscriptionBuilder<T, TOptions>> configureSubscription
    ) where T : EventSubscription<TOptions> where TOptions : SubscriptionOptions {
        Ensure.NotNull(configureSubscription);

        var builder = new SubscriptionBuilder<T, TOptions>(
            Ensure.NotNull(services),
            Ensure.NotEmptyString(subscriptionId)
        );

        configureSubscription(builder);

        services.TryAddSingleton<ISubscriptionHealth, SubscriptionHealthCheck>();

        if (typeof(IMeasuredSubscription).IsAssignableFrom(typeof(T))) services.AddSingleton(GetGapMeasure);

        return services
            .AddSubscriptionBuilder(builder)
            .AddSingleton(sp => GetBuilder(sp).ResolveSubscription(sp))
            .AddSingleton<IHostedService>(
                sp =>
                    new SubscriptionHostedService(
                        GetBuilder(sp).ResolveSubscription(sp),
                        sp.GetService<ISubscriptionHealth>(),
                        sp.GetService<ILoggerFactory>()
                    )
            );

        SubscriptionBuilder<T, TOptions> GetBuilder(IServiceProvider sp)
            => sp.GetSubscriptionBuilder<T, TOptions>(subscriptionId);

        GetSubscriptionGap GetGapMeasure(IServiceProvider sp) {
            var subscription = GetBuilder(sp).ResolveSubscription(sp) as IMeasuredSubscription;
            return subscription!.GetMeasure();
        }
    }

   
    public static IHealthChecksBuilder AddSubscriptionsHealthCheck(
        this IHealthChecksBuilder builder,
        string                    checkName,
        HealthStatus?             failureStatus,
        string[]                  tags
    ) {
        builder.Services.TryAddSingleton<SubscriptionHealthCheck>();

        builder.Services.TryAddSingleton<ISubscriptionHealth>(
            sp => sp.GetRequiredService<SubscriptionHealthCheck>()
        );

        return builder.AddCheck<SubscriptionHealthCheck>(checkName, failureStatus, tags);
    }

    public static IServiceCollection AddCheckpointStore<T>(this IServiceCollection services)
        where T : class, ICheckpointStore {
        services.AddSingleton<T>();

        return ThriveEventFlowDiagnostics.Enabled
            ? services.AddSingleton<ICheckpointStore>(
                sp => new MeasuredCheckpointStore(sp.GetRequiredService<T>())
            )
            : services.AddSingleton<ICheckpointStore>(sp => sp.GetRequiredService<T>());
    }

    public static IServiceCollection AddCheckpointStore<T>(
        this IServiceCollection   services,
        Func<IServiceProvider, T> getStore
    ) where T : class, ICheckpointStore {
        services.AddSingleton(getStore);

        return ThriveEventFlowDiagnostics.Enabled
            ? services.AddSingleton<ICheckpointStore>(
                sp => new MeasuredCheckpointStore(sp.GetRequiredService<T>())
            )
            : services.AddSingleton<ICheckpointStore>(sp => sp.GetRequiredService<T>());
    }
}
