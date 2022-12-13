
namespace ThriveEventFlow;

public class MetadataDeserializationException : Exception {
    public MetadataDeserializationException(Exception inner) : base("Failed to deserialize metadata", inner) { }
}
