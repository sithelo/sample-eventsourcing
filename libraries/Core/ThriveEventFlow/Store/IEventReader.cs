namespace ThriveEventFlow; 

public interface IEventReader {
   
    Task<StreamEvent[]> ReadEvents(
        StreamName         stream,
        StreamReadPosition start,
        int                count,
        CancellationToken  cancellationToken
    );
}
