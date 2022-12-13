
using ThriveEventFlow.Subscriptions.Context;

namespace ThriveEventFlow.Subscriptions.Filters.Partitioning; 

public static class Partitioner {
    public delegate uint GetPartitionHash(string partitionKey);
    
    public delegate string GetPartitionKey(IMessageConsumeContext context);
}