
using System.Diagnostics;


using ThriveEventFlow.Diagnostics;
using ThriveEventFlow.Diagnostics.Tracing;
using ThriveEventFlow.Subscriptions.Context;
using ThriveEventFlow.Subscriptions.Diagnostics;
using ActivityStatus = ThriveEventFlow.Diagnostics.ActivityStatus;

namespace ThriveEventFlow.Subscriptions.Filters;

public class TracingFilter : ConsumeFilter<IMessageConsumeContext> {
    readonly KeyValuePair<string, object?>[] _defaultTags;

    public TracingFilter(string consumerName) {
        var tags = new KeyValuePair<string, object?>[] { new(TelemetryTags.ThriveEventFlow.Consumer, consumerName) };

        _defaultTags = tags.Concat(ThriveEventFlowDiagnostics.Tags).ToArray();
    }

    protected override async ValueTask Send(
        IMessageConsumeContext          context,
        LinkedListNode<IConsumeFilter>? next
    ) {
        if (context.Message == null || next == null) return;

        using var activity = Activity.Current?.Context != context.ParentContext
            ? SubscriptionActivity.Start(
                $"{Constants.Components.Consumer}.{context.SubscriptionId}/{context.MessageType}",
                ActivityKind.Consumer,
                context,
                _defaultTags
            )
            : Activity.Current;

        if (activity?.IsAllDataRequested == true && context is AsyncConsumeContext delayedAckContext) {
            activity.SetContextTags(context)?.SetTag(TelemetryTags.ThriveEventFlow.Partition, delayedAckContext.PartitionId);
        }

        try {
            await next.Value.Send(context, next.Next).NoContext();

            if (activity != null) {
                if (context.WasIgnored()) {
                    activity.ActivityTraceFlags = ActivityTraceFlags.None;
                }

                activity.SetActivityStatus(ActivityStatus.Ok());
            }
        }
        catch (Exception e) {
            activity?.SetActivityStatus(ActivityStatus.Error(e, $"Error handling {context.MessageType}"));
            throw;
        }
    }
}
