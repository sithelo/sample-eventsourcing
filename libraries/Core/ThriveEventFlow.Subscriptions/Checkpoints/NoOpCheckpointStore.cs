
using ThriveEventFlow.Subscriptions.Logging;

namespace ThriveEventFlow.Subscriptions.Checkpoints;

public class NoOpCheckpointStore : ICheckpointStore {
    Checkpoint _start;

    public NoOpCheckpointStore(ulong? start = null) => _start = new Checkpoint("", start);

    public ValueTask<Checkpoint> GetLastCheckpoint(string checkpointId, CancellationToken cancellationToken) {
        Logger.Current.CheckpointLoaded(this, _start);
        return new ValueTask<Checkpoint>(_start);
    }

    public ValueTask<Checkpoint> StoreCheckpoint(
        Checkpoint        checkpoint,
        bool              force,
        CancellationToken cancellationToken
    ) {
        _start = checkpoint;
        CheckpointStored?.Invoke(this, checkpoint);
        Logger.Current.CheckpointStored(this, checkpoint, force);
        return new ValueTask<Checkpoint>(checkpoint);
    }

    public event EventHandler<Checkpoint>? CheckpointStored;
}
