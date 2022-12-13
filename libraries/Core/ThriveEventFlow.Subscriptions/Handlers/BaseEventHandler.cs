
using ThriveEventFlow.Subscriptions.Context;

namespace ThriveEventFlow.Subscriptions; 

public abstract class BaseEventHandler : IEventHandler {
    protected BaseEventHandler() => DiagnosticName = GetType().Name;
    
    public string DiagnosticName { get; }

    public abstract ValueTask<EventHandlingStatus> HandleEvent(IMessageConsumeContext context);
}