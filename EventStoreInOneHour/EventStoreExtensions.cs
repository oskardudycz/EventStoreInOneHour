namespace EventStoreInOneHour;

public static class EventStoreExtensions
{
    public static async Task<T> AggregateStreamAsync<T>(
        this IEventStore eventStore,
        Func<T> getDefault,
        Func<T, object, T> evolve,
        Guid streamId,
        long? atStreamVersion = null,
        DateTime? atTimestamp = null,
        CancellationToken ct = default
    ) where T : notnull
    {
        var events = await eventStore.GetEventsAsync(streamId, atStreamVersion, atTimestamp, ct);

        var aggregate = getDefault();

        foreach (var @event in events)
        {
            aggregate = evolve(aggregate, @event);
        }

        return aggregate;
    }

    public static async Task Handle<TEntity>(
        this IEventStore eventStore,
        Func<TEntity> getDefault,
        Func<TEntity, object, TEntity> evolve,
        Func<object, TEntity, object[]> decide,
        Guid streamId,
        object command,
        long? expectedVersion = null,
        CancellationToken ct = default
    ) where TEntity : notnull
    {
        var entity = await eventStore.AggregateStreamAsync(
            getDefault,
            evolve,
            streamId,
            ct: ct
        );

        var newEvents = decide(command, entity);

        await eventStore.AppendEventsAsync<TEntity>(streamId, newEvents, expectedVersion, ct);
    }
}
