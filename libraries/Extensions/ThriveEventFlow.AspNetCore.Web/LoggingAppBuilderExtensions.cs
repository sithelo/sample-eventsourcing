using JetBrains.Annotations;
using ThriveEventFlow.Diagnostics.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;

namespace ThriveEventFlow.AspNetCore.Web;

[PublicAPI]
public static class LoggingAppBuilderExtensions {
    public static IApplicationBuilder UseThriveEventFlowLogs(this IApplicationBuilder host) {
        listener ??= new LoggingEventListener(host.ApplicationServices.GetRequiredService<ILoggerFactory>());
        return host;
    }

    static LoggingEventListener? listener;
}
