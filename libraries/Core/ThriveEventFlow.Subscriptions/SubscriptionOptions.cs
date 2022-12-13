
using JetBrains.Annotations;


namespace ThriveEventFlow.Subscriptions;     

[PublicAPI]
public record SubscriptionOptions {
    
    public string SubscriptionId { get; set; } = null!;
        
   
    public bool ThrowOnError { get; set; }
    
    
    public IEventSerializer? EventSerializer { get; set; }
}