
using ThriveEventFlow.Subscriptions.Consumers;
using ThriveEventFlow.Subscriptions.Context;

namespace ThriveEventFlow.Subscriptions.Filters;

public class ConsumerFilter : ConsumeFilter<IMessageConsumeContext> {
    readonly IMessageConsumer<IMessageConsumeContext> _consumer;

    public ConsumerFilter(IMessageConsumer<IMessageConsumeContext> consumer) => _consumer = consumer;

    protected override ValueTask Send(IMessageConsumeContext context, LinkedListNode<IConsumeFilter>? next)
        => _consumer.Consume(context);
}
