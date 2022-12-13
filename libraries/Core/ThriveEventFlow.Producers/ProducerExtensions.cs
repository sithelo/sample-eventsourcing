using JetBrains.Annotations;

namespace ThriveEventFlow.Producers;

[PublicAPI]
public static class ProducerExtensions {
   
    public static Task Produce<TMessage>(
        this IEventProducer producer,
        StreamName          stream,
        TMessage            message,
        Metadata?           metadata,
        Metadata?           additionalHeaders = null,
        AcknowledgeProduce? onAck             = null,
        CancellationToken   cancellationToken = default
    ) where TMessage : class {
        var producedMessages =
            message is IEnumerable<object> collection
                ? ConvertMany(collection, metadata, additionalHeaders, onAck)
                : ConvertOne(message, metadata, additionalHeaders, onAck);

        return producer.Produce(stream, producedMessages, cancellationToken);
    }

  
    public static Task Produce<TProduceOptions, TMessage>(
        this IEventProducer<TProduceOptions> producer,
        StreamName                           stream,
        TMessage                             message,
        Metadata?                            metadata,
        TProduceOptions                      options,
        Metadata?                            additionalHeaders = null,
        AcknowledgeProduce?                  onAck             = null,
        CancellationToken                    cancellationToken = default
    ) where TMessage : class where TProduceOptions : class {
        var producedMessages =
            Ensure.NotNull(message) is IEnumerable<object> collection
                ? ConvertMany(collection, metadata, additionalHeaders, onAck)
                : ConvertOne(message, metadata, additionalHeaders, onAck);

        return producer.Produce(stream, producedMessages, options, cancellationToken);
    }

    static IEnumerable<ProducedMessage> ConvertMany(
        IEnumerable<object> messages,
        Metadata?           metadata,
        Metadata?           additionalHeaders,
        AcknowledgeProduce? onAck
    )
        => messages.Select(x => new ProducedMessage(x, metadata, additionalHeaders) { OnAck = onAck });

    static IEnumerable<ProducedMessage> ConvertOne(
        object              message,
        Metadata?           metadata,
        Metadata?           additionalHeaders,
        AcknowledgeProduce? onAck
    )
        => new[] { new ProducedMessage(message, metadata, additionalHeaders) { OnAck = onAck } };
}
