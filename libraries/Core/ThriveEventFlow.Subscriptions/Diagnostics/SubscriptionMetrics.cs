
using System.Diagnostics;
using System.Diagnostics.Metrics;
using ThriveEventFlow.Diagnostics;
using static ThriveEventFlow.Subscriptions.Diagnostics.SubscriptionsEventSource;

// ReSharper disable ParameterTypeCanBeEnumerable.Local

namespace ThriveEventFlow.Subscriptions.Diagnostics;

public sealed class SubscriptionMetrics : IWithCustomTags, IDisposable {
    const string MetricPrefix = "thrive";
    const string Category     = "subscription";

    public static readonly string MeterName = ThriveEventFlowDiagnostics.GetMeterName(Category);

    public const string GapCountMetricName    = $"{MetricPrefix}.{Category}.gap.count";
    public const string GapTimeMetricName     = $"{MetricPrefix}.{Category}.gap.seconds";
    public const string ProcessingRateName    = $"{MetricPrefix}.{Category}.duration";
    public const string ErrorCountName        = $"{MetricPrefix}.{Category}.errors.count";
    public const string CheckpointQueueLength = $"{MetricPrefix}.{Category}.pendingCheckpoint.count";

    public const string SubscriptionIdTag = "subscription-id";
    public const string MessageTypeTag    = "message-type";
    public const string PartitionIdTag    = "partition";

    public SubscriptionMetrics(IEnumerable<GetSubscriptionGap> measures) {
        _meter = ThriveEventFlowDiagnostics.GetMeter(MeterName);
        var getGaps = measures.ToArray();
        _checkpointMetrics = new Lazy<CheckpointCommitMetrics>(() => new CheckpointCommitMetrics());
        IEnumerable<SubscriptionGap>? gaps = null;

        _meter.CreateObservableGauge(
            GapCountMetricName,
            () => TryObserving(GapCountMetricName, () => ObserveGapValues(getGaps)),
            "events",
            "Gap between the last processed event and the stream tail"
        );

        _meter.CreateObservableGauge(
            GapTimeMetricName,
            () => TryObserving(GapTimeMetricName, ObserveTimeValues),
            "s",
            "Subscription time lag, seconds"
        );

        _meter.CreateObservableGauge(
            CheckpointQueueLength,
            () => TryObserving(CheckpointQueueLength, _checkpointMetrics.Value.Record),
            "events",
            "Number of pending checkpoints"
        );

        var histogram  = _meter.CreateHistogram<double>(ProcessingRateName, "ms", "Processing duration, milliseconds");
        var errorCount = _meter.CreateCounter<long>(ErrorCountName, "events", "Number of event processing failures");

        _listener = new ActivityListener {
            ShouldListenTo  = x => x.Name == ThriveEventFlowDiagnostics.InstrumentationName,
            ActivityStopped = x => ActivityStopped(histogram, errorCount, x)
        };

        ActivitySource.AddActivityListener(_listener);

        IEnumerable<Measurement<double>> ObserveTimeValues()
            => gaps?
                   .Select(x => Measure(x.TimeGap.TotalSeconds, x.SubscriptionId))
            ?? Array.Empty<Measurement<double>>();

        IEnumerable<Measurement<long>> ObserveGapValues(GetSubscriptionGap[] gapMeasure)
            => gapMeasure
                .Select(GetGap)
                .Where(x => x != SubscriptionGap.Invalid)
                .Select(x => Measure((long)x.PositionGap, x.SubscriptionId));

        Measurement<T> Measure<T>(T value, string subscriptionId) where T : struct {
            if (_customTags.Length == 0) {
                return new Measurement<T>(value, SubTag(subscriptionId));
            }

            var tags = new List<KeyValuePair<string, object?>>(_customTags) { SubTag(subscriptionId) };
            return new Measurement<T>(value, tags);
        }
    }

    static IEnumerable<Measurement<T>> TryObserving<T>(string metric, Func<IEnumerable<Measurement<T>>> observe)
        where T : struct {
        try {
            return observe();
        }
        catch (Exception e) {
            Log.MetricCollectionFailed(metric, e);
            Activity.Current?.SetStatus(ActivityStatusCode.Error, e.Message);
            return Array.Empty<Measurement<T>>();
        }
    }

    static KeyValuePair<string, object?> GetTag(string key, object? id) => new(key, id);

    static KeyValuePair<string, object?> SubTag(object? id) => new(SubscriptionIdTag, id);

    void ActivityStopped(Histogram<double> histogram, Counter<long> errorCount, Activity activity) {
        if (activity.Kind != ActivityKind.Consumer) return;

        var subId = activity.GetTagItem(TelemetryTags.ThriveEventFlow.Subscription);
        if (subId == null) return;

        var subTag       = SubTag(subId);
        var typeTag      = GetTag(MessageTypeTag, activity.GetTagItem(TelemetryTags.Message.Type));
        var partitionTag = GetTag(PartitionIdTag, activity.GetTagItem(TelemetryTags.ThriveEventFlow.Partition));

        var tags = new TagList(_customTags) {
            subTag,
            typeTag,
            partitionTag
        };

        histogram.Record(activity.Duration.TotalMilliseconds, tags);

        if (activity.Status == ActivityStatusCode.Error) {
            errorCount.Add(1, tags);
        }
    }

    static SubscriptionGap GetGap(GetSubscriptionGap gapMeasure) {
        var cts = new CancellationTokenSource(500);

        try {
            var t = gapMeasure(cts.Token);

            return t.IsCompleted ? t.Result : t.NoContext().GetAwaiter().GetResult();
        }
        catch (Exception e) {
            Log.MetricCollectionFailed("Subscription Gap", e);
            return SubscriptionGap.Invalid;
        }
    }

    readonly Meter                         _meter;
    readonly ActivityListener              _listener;
    readonly Lazy<CheckpointCommitMetrics> _checkpointMetrics;
    KeyValuePair<string, object?>[]        _customTags = ThriveEventFlowDiagnostics.Tags;

    public void Dispose() {
        _listener.Dispose();
        _meter.Dispose();
        if (_checkpointMetrics.IsValueCreated) _checkpointMetrics.Value.Dispose();
    }

    public void SetCustomTags(TagList customTags) => _customTags = _customTags.Concat(customTags).ToArray();
}
