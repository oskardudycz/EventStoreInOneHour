// namespace EventStoreInOneHour;
//
// public interface IRepository<T> where T : IAggregate
// {
//     Task<T?> FindAsync(Guid id, CancellationToken ct = default);
//
//     Task AddAsync(T aggregate, CancellationToken ct = default);
//
//     Task UpdateAsync(T aggregate, CancellationToken ct = default);
//
//     Task DeleteAsync(T aggregate, CancellationToken ct = default);
//
//     void RegisterSnapshot(ISnapshot snapshot);
// }
