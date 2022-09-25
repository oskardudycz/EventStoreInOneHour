namespace EventStoreInOneHour;

public class Repository<T>: IRepository<T> where T : IAggregate
{
    private readonly IEventStore eventStore;
    private readonly IList<ISnapshot> snapshots = new List<ISnapshot>();

    public Repository(IEventStore eventStore)
    {
        this.eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
    }

    public T? Find(Guid id)
    {
        return eventStore.AggregateStream<T>(id);
    }

    public void Add(T aggregate)
    {
        Store(aggregate);
    }

    public void Update(T aggregate)
    {
        Store(aggregate);
    }

    public void Delete(T aggregate)
    {
        Store(aggregate);
    }

    public void AddSnapshot(ISnapshot snapshot)
    {
        snapshots.Add(snapshot);
    }

    public bool Store<TStream>(TStream aggregate) where TStream : IAggregate
    {
        var events = aggregate.DequeueUncommittedEvents();
        var initialVersion = aggregate.Version - events.Count();

        foreach (var @event in events)
        {
            eventStore.AppendEvent<TStream>(aggregate.Id, @event, initialVersion++);
        }

        foreach (var snapshot in snapshots)
        {
            snapshot.Handle(aggregate);
        }

        return true;
    }
}
