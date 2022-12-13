using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ThriveEventFlow.AspNetCore;

[PublicAPI]
public static class AggregateFactoryBuilderExtensions {
    public static IHost UseAggregateFactory(this IHost host) {
        UseAggregateFactory(host.Services);
        return host;
    }
    
    static void UseAggregateFactory(IServiceProvider sp) {
        var resolvers = sp.GetServices<ResolveAggregateFactory>();
        var registry  = sp.GetService<AggregateFactoryRegistry>() ?? AggregateFactoryRegistry.Instance;

        foreach (var resolver in resolvers) {
            registry.UnsafeCreateAggregateUsing(
                resolver.Type,
                () => resolver.CreateInstance(sp)
            );
        }
    }
}