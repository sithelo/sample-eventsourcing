
using JetBrains.Annotations;

namespace ThriveEventFlow.Subscriptions.Checkpoints;

[PublicAPI]
public record Checkpoint(string Id, ulong? Position);

[PublicAPI]
public interface ICheckpointStore {
    ValueTask<Checkpoint> GetLastCheckpoint(string checkpointId, CancellationToken cancellationToken);

    ValueTask<Checkpoint> StoreCheckpoint(Checkpoint checkpoint, bool force, CancellationToken cancellationToken);
}