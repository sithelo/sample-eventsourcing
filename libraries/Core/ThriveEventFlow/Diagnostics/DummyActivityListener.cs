using System.Diagnostics;

namespace ThriveEventFlow.Diagnostics;

public static class DummyActivityListener {
    public static ActivityListener Create()
        => new() { ShouldListenTo  = x => x.Name == ThriveEventFlowDiagnostics.InstrumentationName };
}
