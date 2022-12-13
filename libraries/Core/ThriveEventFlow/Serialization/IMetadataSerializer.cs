namespace ThriveEventFlow; 

public interface IMetadataSerializer {
    byte[] Serialize(Metadata evt);

    
    Metadata? Deserialize(ReadOnlySpan<byte> bytes);
}