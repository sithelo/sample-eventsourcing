
using System.Threading.Channels;

namespace ThriveEventFlow.Subscriptions.Channels;

public sealed class ConcurrentChannelWorker<T> : IAsyncDisposable {
    readonly Channel<T>              _channel;
    readonly CancellationTokenSource _cts;
    readonly Task[]                  _readerTasks;

   
    public ConcurrentChannelWorker(
        Channel<T>        channel,
        ProcessElement<T> process,
        int               concurrencyLevel
    ) {
        _channel = channel;
        _cts     = new CancellationTokenSource();

        _readerTasks = Enumerable.Range(0, concurrencyLevel)
            .Select(_ => Task.Run(() => _channel.Read(process, _cts.Token))).ToArray();
    }

    public ValueTask Write(T element, CancellationToken cancellationToken)
        => _disposing ? default : _channel.Write(element, false, cancellationToken);

    public ValueTask DisposeAsync() {
        if (_disposing) return default;

        _disposing = true;
        return _channel.Stop(_cts, _readerTasks);
    }

    bool _disposing;
}