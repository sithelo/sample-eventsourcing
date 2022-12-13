namespace ThriveEventFlow;


public interface IEventStore : IEventReader, IEventWriter {
    
    Task<bool> StreamExists(StreamName stream, CancellationToken cancellationToken);
    
    // /// <summary>
    // /// Read a number of events from a given stream, backwards (from the stream end)
    // /// </summary>
    // /// <param name="stream">Stream name</param>
    // /// <param name="count">How many events to read</param>
    // /// <param name="cancellationToken">Cancellation token</param>
    // /// <returns>An array with events retrieved from the stream</returns>
    // Task<StreamEvent[]> ReadEventsBackwards(
    //     StreamName        stream,
    //     int               count,
    //     CancellationToken cancellationToken
    // );

   
    Task TruncateStream(
        StreamName             stream,
        StreamTruncatePosition truncatePosition,
        ExpectedStreamVersion  expectedVersion,
        CancellationToken      cancellationToken
    );

   
    Task DeleteStream(
        StreamName            stream,
        ExpectedStreamVersion expectedVersion,
        CancellationToken     cancellationToken
    );
}