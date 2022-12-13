
using ThriveEventFlow.Subscriptions.Consumers;

namespace ThriveEventFlow.Subscriptions.Filters; 

public static class ConsumePipeExtensions {
    public static ConsumePipe AddDefaultConsumer(this ConsumePipe consumePipe, params IEventHandler[] handlers)
        => consumePipe.AddFilterLast(new ConsumerFilter(new DefaultConsumer(handlers)));
}