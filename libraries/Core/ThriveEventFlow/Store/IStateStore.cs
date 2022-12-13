using JetBrains.Annotations;

namespace ThriveEventFlow; 


[PublicAPI]
public interface IStateStore {
  
    Task<T> LoadState<T>(StreamName stream, CancellationToken cancellationToken) where T : State<T>, new();
}