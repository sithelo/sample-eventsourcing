
using ThriveEventFlow.Subscriptions.Logging;

namespace ThriveEventFlow.Subscriptions.Context;


public delegate ValueTask Acknowledge(IMessageConsumeContext ctx);


public delegate ValueTask Fail(IMessageConsumeContext ctx, Exception exception);


public class AsyncConsumeContext : WrappedConsumeContext {
    readonly Acknowledge _acknowledge;
    readonly Fail        _fail;


    public AsyncConsumeContext(IMessageConsumeContext inner, Acknowledge acknowledge, Fail fail) : base(inner) {
        inner.LogContext ??= Logger.Current;
        _acknowledge     =   acknowledge;
        _fail            =   fail;
    }


    public async ValueTask Acknowledge() => await _acknowledge(this);


    public ValueTask Fail(Exception exception) => _fail(this, exception);

    public string? PartitionKey { get; internal set; }
    public long    PartitionId  { get; internal set; }
}
