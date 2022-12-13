// Copyright (C) 2021-2022 Ubiquitous AS. All rights reserved
// Licensed under the Apache License, Version 2.0.

using System.Threading.Channels;

namespace ThriveEventFlow.Subscriptions.Channels;

public class ChannelWorker<T> {
    readonly Channel<T>              _channel;
    readonly bool                    _throwOnFull;
    readonly CancellationTokenSource _cts;
    readonly Task                    _readerTask;


    public ChannelWorker(
        Channel<T>        channel,
        ProcessElement<T> process,
        bool              throwOnFull = false
    ) {
        _channel     = channel;
        _throwOnFull = throwOnFull;
        _cts         = new CancellationTokenSource();
        _readerTask  = Task.Run(() => _channel.Read(process, _cts.Token));
    }

    public ValueTask Write(T element, CancellationToken cancellationToken)
        => _stopping ? default : _channel.Write(element, _throwOnFull, cancellationToken);

    public ValueTask Stop(Func<CancellationToken, ValueTask>? finalize = null) {
        if (_stopping) return default;

        _stopping = true;
        return _channel.Stop(_cts, new[] { _readerTask }, finalize);
    }

    bool _stopping;
}
