
using ThriveEventFlow.Subscriptions.Context;

namespace ThriveEventFlow.Subscriptions; 

public interface IEventHandler {
    string DiagnosticName { get; }
    
    ValueTask<EventHandlingStatus> HandleEvent(IMessageConsumeContext context);
}