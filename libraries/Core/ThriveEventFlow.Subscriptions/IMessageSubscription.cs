
using ThriveEventFlow.Subscriptions.Diagnostics;

namespace ThriveEventFlow.Subscriptions;

public delegate void OnSubscribed(string subscriptionId);

public delegate void OnDropped(string subscriptionId, DropReason dropReason, Exception? exception);

public delegate void OnUnsubscribed(string subscriptionId);

public interface IMessageSubscription {
    string SubscriptionId { get; }
    
    ValueTask Subscribe(
        OnSubscribed      onSubscribed,
        OnDropped         onDropped,
        CancellationToken cancellationToken
    );

    ValueTask Unsubscribe(OnUnsubscribed onUnsubscribed, CancellationToken cancellationToken);
}

public interface IMeasuredSubscription {
    GetSubscriptionGap GetMeasure();
}