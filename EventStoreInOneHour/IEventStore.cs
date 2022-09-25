namespace EventStoreInOneHour;

public interface IEventStore
{
    void Init();
    void RegisterProjection(IProjection projection);

    Task AppendEventsAsync<TStream>(
        Guid streamId,
        IEnumerable<object> @event,
        long? expectedVersion = null,
        CancellationToken ct = default
    ) where TStream : notnull;

    Task<IReadOnlyList<object>> GetEventsAsync(
        Guid streamId,
        long? atStreamVersion = null,
        DateTime? atTimestamp = null,
        CancellationToken ct = default
    );
}
