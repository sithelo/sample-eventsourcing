using System.Runtime.CompilerServices;
using ThriveEventFlow;
using JetBrains.Annotations;

namespace ThriveEventFlow;

[PublicAPI]
public abstract class Aggregate {

    public object[] Original { get; protected set; } = Array.Empty<object>();


    public IReadOnlyCollection<object> Changes => _changes.AsReadOnly();


    public IEnumerable<object> Current => Original.Concat(_changes);


    public void ClearChanges() => _changes.Clear();


    public int OriginalVersion => Original.Length - 1;


    public int CurrentVersion => OriginalVersion + Changes.Count;

    readonly List<object> _changes = new();


    public abstract void Load(IEnumerable<object?> events);


    protected void AddChange(object evt) => _changes.Add(evt);

   
    protected void EnsureDoesntExist(Func<Exception>? getException = null) {
        if (CurrentVersion >= 0)
            throw getException?.Invoke()
               ?? new DomainException($"{GetType().Name} already exists");
    }

  
    protected void EnsureExists(Func<Exception>? getException = null) {
        if (CurrentVersion < 0)
            throw getException?.Invoke()
               ?? new DomainException($"{GetType().Name} doesn't exist");
    }
}

public abstract class Aggregate<T> : Aggregate where T : State<T>, new() {
    protected Aggregate() => State = new T();

    
    protected (T PreviousState, T CurrentState) Apply(object evt) {
        AddChange(evt);
        var previous = State;
        State = State.When(evt);
        return (previous, State);
    }

 
    public override void Load(IEnumerable<object?> events) {
        Original = events.Where(x => x != null).ToArray()!;
        State    = Original.Aggregate(new T(), Fold);
    }

    static T Fold(T state, object evt) => state.When(evt);

    
    public T State { get; private set; }
}
