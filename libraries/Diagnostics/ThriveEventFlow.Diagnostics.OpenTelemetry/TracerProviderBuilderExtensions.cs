using JetBrains.Annotations;
using OpenTelemetry.Trace;

namespace ThriveEventFlow.Diagnostics.OpenTelemetry;

[PublicAPI]
public static class TracerProviderBuilderExtensions {
  
    public static TracerProviderBuilder AddThriveEventFlowTracing(this TracerProviderBuilder builder) {
        // The DummyListener is added by default so the remote context is propagated regardless.
        // After adding the activity source to OpenTelemetry we don't need the dummy listener.
        ThriveEventFlowDiagnostics.RemoveDummyListener();

        return Ensure.NotNull(builder)
            .AddSource(ThriveEventFlowDiagnostics.InstrumentationName);
    }
}
