// namespace EventStoreInOneHour;
//
// public class Repository<T>: IRepository<T> where T : IAggregate
// {
//     private readonly IEventStore eventStore;
//     private readonly IList<ISnapshot> snapshots = new List<ISnapshot>();
//
//     public Repository(IEventStore eventStore) =>
//         this.eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
//
//     public Task<T?> FindAsync(Guid id, CancellationToken ct = default) =>
//         eventStore.AggregateStreamAsync<T>(id, ct: ct);
//
//     public Task AddAsync(T aggregate, CancellationToken ct = default) =>
//         Store(aggregate, ct);
//
//     public Task UpdateAsync(T aggregate, CancellationToken ct = default) =>
//         Store(aggregate, ct);
//
//     public Task DeleteAsync(T aggregate, CancellationToken ct = default) =>
//         Store(aggregate, ct);
//
//     public void RegisterSnapshot(ISnapshot snapshot) =>
//         snapshots.Add(snapshot);
//
//     private async Task Store<TStream>(TStream aggregate, CancellationToken ct = default) where TStream : IAggregate
//     {
//         var events = aggregate.DequeueUncommittedEvents().ToArray();
//         var initialVersion = aggregate.Version - events.Length;
//
//         await eventStore.AppendEventsAsync<TStream>(aggregate.Id, events, initialVersion, ct);
//
//         foreach (var snapshot in snapshots)
//         {
//             snapshot.Handle(aggregate);
//         }
//     }
// }
