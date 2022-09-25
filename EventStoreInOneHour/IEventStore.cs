using System.Collections;

namespace EventStoreInOneHour;

public interface IEventStore
{
    void Init();
    void AddProjection(IProjection projection);
    Task AppendEventsAsync<TStream>(Guid streamId, IEnumerable<object> @event, long? expectedVersion = null, CancellationToken ct = default) where TStream : notnull;
    StreamState? GetStreamState(Guid streamId);
    Task<IEnumerable> GetEventsAsync(Guid streamId, long? atStreamVersion = null, DateTime? atTimestamp = null, CancellationToken ct = default);
}
