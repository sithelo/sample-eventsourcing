using System.Diagnostics;
using JetBrains.Annotations;
using ThriveEventFlow.Diagnostics.Metrics;
using ThriveEventFlow.Subscriptions.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;

namespace ThriveEventFlow.Diagnostics.OpenTelemetry;

[PublicAPI]
public static class MeterProviderBuilderExtensions {
    
    public static MeterProviderBuilder AddThriveEventFlowSubscriptions(this MeterProviderBuilder builder, TagList? customTags = null)
        => Ensure.NotNull(builder)
            .AddMeter(SubscriptionMetrics.MeterName)
            .AddMetrics<SubscriptionMetrics>(customTags);

   
    public static MeterProviderBuilder AddThriveEventFlow(this MeterProviderBuilder builder, TagList? customTags = null)
        => Ensure.NotNull(builder)
            .AddMeter(ThriveEventFlowMetrics.MeterName)
            .AddMetrics<ThriveEventFlowMetrics>(customTags);

    static MeterProviderBuilder AddMetrics<T>(this MeterProviderBuilder builder, TagList? customTags = null)
        where T : class, IWithCustomTags {
        builder.GetServices().AddSingleton<T>();

        return builder is IDeferredMeterProviderBuilder deferredMeterProviderBuilder
            ? deferredMeterProviderBuilder.Configure(
                (sp, b) => {
                    b.AddInstrumentation(
                        () => {
                            var instrument = sp.GetRequiredService<T>();
                            if (customTags != null) instrument.SetCustomTags(customTags.Value);
                            return instrument;
                        }
                    );
                }
            ) : builder;
    }
}
