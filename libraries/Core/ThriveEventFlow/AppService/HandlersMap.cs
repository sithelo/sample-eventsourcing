
using ThriveEventFlow.Diagnostics;

namespace ThriveEventFlow;

public delegate Task ActOnAggregateAsync<in TAggregate, in TCommand>(
    TAggregate        aggregate,
    TCommand          command,
    CancellationToken cancellationToken
) where TAggregate : Aggregate;

public delegate void ActOnAggregate<in TAggregate, in TCommand>(TAggregate aggregate, TCommand command)
    where TAggregate : Aggregate;

record RegisteredHandler<T>(ExpectedState ExpectedState, Func<T, object, CancellationToken, ValueTask<T>> Handler);

class HandlersMap<TAggregate> : Dictionary<Type, RegisteredHandler<TAggregate>>
    where TAggregate : Aggregate {
    public void AddHandler<TCommand>(RegisteredHandler<TAggregate> handler) {
        if (ContainsKey(typeof(TCommand))) {
            ThriveEventFlowEventSource.Log.CommandHandlerAlreadyRegistered<TCommand>();
            throw new Exceptions.CommandHandlerAlreadyRegistered<TCommand>();
        }

        Add(typeof(TCommand), handler);
    }

    public void AddHandler<TCommand>(ExpectedState expectedState, ActOnAggregateAsync<TAggregate, TCommand> action) {
        AddHandler<TCommand>(
            new RegisteredHandler<TAggregate>(
                expectedState,
                async (aggregate, cmd, ct) => {
                    await action(aggregate, (TCommand)cmd, ct).NoContext();
                    return aggregate;
                }
            )
        );
       
    }

    public void AddHandler<TCommand>(ExpectedState expectedState, ActOnAggregate<TAggregate, TCommand> action)
        => AddHandler<TCommand>(
            new RegisteredHandler<TAggregate>(
                expectedState,
                (aggregate, cmd, _) => {
                    action(aggregate, (TCommand)cmd);
                    return new ValueTask<TAggregate>(aggregate);
                }
            )
        );

   
}
