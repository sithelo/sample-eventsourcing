using JetBrains.Annotations;

namespace ThriveEventFlow; 

[PublicAPI]
public record StreamEvent(Guid Id, object? Payload, Metadata Metadata, string ContentType, long Position);