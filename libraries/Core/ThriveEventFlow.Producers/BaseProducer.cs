using System.Diagnostics;
using ThriveEventFlow.Diagnostics;
using ThriveEventFlow.Producers.Diagnostics;
using static ThriveEventFlow.Diagnostics.TelemetryTags;

namespace ThriveEventFlow.Producers;

public abstract class BaseProducer<TProduceOptions> : BaseProducer, IEventProducer<TProduceOptions>
    where TProduceOptions : class {
    protected BaseProducer(ProducerTracingOptions? tracingOptions = null) 
        : base(tracingOptions) { }
    
    protected abstract Task ProduceMessages(
        StreamName                   stream,
        IEnumerable<ProducedMessage> messages,
        TProduceOptions?             options,
        CancellationToken            cancellationToken = default
    );

    /// <inheritdoc />
    public async Task Produce(
        StreamName                   stream,
        IEnumerable<ProducedMessage> messages,
        TProduceOptions?             options,
        CancellationToken            cancellationToken = default
    ) {
        var (activity, msgs) = ProducerActivity.Start(messages, DefaultTags);

        if (activity is { IsAllDataRequested: true }) {
            activity.SetTag(Messaging.Destination, stream);
        }

        await ProduceMessages(stream, msgs, options, cancellationToken).NoContext();

        activity?.Dispose();
    }

    /// <inheritdoc />
    protected override Task ProduceMessages(
        StreamName                   stream,
        IEnumerable<ProducedMessage> messages,
        CancellationToken            cancellationToken = default
    )
        => ProduceMessages(stream, messages, null, cancellationToken);
}

public abstract class BaseProducer : IEventProducer {
    protected KeyValuePair<string, object?>[] DefaultTags { get; }

    protected BaseProducer(ProducerTracingOptions? tracingOptions = null) {
        var options = tracingOptions ?? new ProducerTracingOptions();
        DefaultTags = options.AllTags.Concat(ThriveEventFlowDiagnostics.Tags).ToArray();
    }

    protected abstract Task ProduceMessages(
        StreamName                   stream,
        IEnumerable<ProducedMessage> messages,
        CancellationToken            cancellationToken = default
    );

    public async Task Produce(
        StreamName                   stream,
        IEnumerable<ProducedMessage> messages,
        CancellationToken            cancellationToken = default
    ) {
        var messagesArray = messages.ToArray();
        if (messagesArray.Length == 0) return;

        var traced = messagesArray.Length == 1
            ? ForOne()
            : ProducerActivity.Start(messagesArray, DefaultTags);

        using var activity = traced.act;

        if (activity is { IsAllDataRequested: true }) {
            activity.SetTag(Messaging.Destination, stream);
            activity.SetTag(TelemetryTags.ThriveEventFlow.Stream, stream);
        }

        await ProduceMessages(stream, traced.msgs, cancellationToken).NoContext();

        (Activity? act, ProducedMessage[] msgs) ForOne() {
            var (act, producedMessage) = ProducerActivity.Start(messagesArray[0], DefaultTags);
            return (act?.Start(), new[] { producedMessage });
        }
    }
}
