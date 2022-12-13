
using JetBrains.Annotations;
using Microsoft.Extensions.Hosting;

namespace ThriveEventFlow.Producers;

[PublicAPI]
public interface IEventProducer {
   
    Task Produce(
        StreamName                   stream,
        IEnumerable<ProducedMessage> messages,
        CancellationToken            cancellationToken = default
    );
}

[PublicAPI]
public interface IEventProducer<in TProduceOptions> : IEventProducer where TProduceOptions : class {
    
    Task Produce(
        StreamName                   stream,
        IEnumerable<ProducedMessage> messages,
        TProduceOptions?             options,
        CancellationToken            cancellationToken = default
    );
}

public interface IHostedProducer : IHostedService {
    bool Ready { get; }
}