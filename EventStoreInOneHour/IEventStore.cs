using System.Collections;

namespace EventStoreInOneHour;

public interface IEventStore
{
    void Init();
    void RegisterProjection(IProjection projection);
    Task AppendEventsAsync<TStream>(Guid streamId, IEnumerable<object> @event, long? expectedVersion = null, CancellationToken ct = default) where TStream : notnull;
    StreamState? GetStreamState(Guid streamId);
    Task<IReadOnlyList<object>> GetEventsAsync(Guid streamId, long? atStreamVersion = null, DateTime? atTimestamp = null, CancellationToken ct = default);
}
