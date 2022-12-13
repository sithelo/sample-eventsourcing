using ThriveEventFlow.Diagnostics.Logging;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ThriveEventFlow.AspNetCore;

[PublicAPI]
public static class LoggingAppBuilderExtensions {
    public static IHost AddThriveEventFlowLogs(this IHost host) {
        AddThriveEventFlowLogs(host.Services);
        return host;
    }

    public static void AddThriveEventFlowLogs(this IServiceProvider provider)
        => listener ??= new LoggingEventListener(provider.GetRequiredService<ILoggerFactory>());

    static LoggingEventListener? listener;
}
