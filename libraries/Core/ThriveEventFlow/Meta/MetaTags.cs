namespace ThriveEventFlow;

public static class MetaTags {
    private const string Prefix = "thrive";

    public const string MessageId     = $"{Prefix}.message-id";
    public const string CorrelationId = $"{Prefix}.correlation-id";
    public const string CausationId   = $"{Prefix}.causation-id";
}
