using EventStoreInOneHour.Tools;

namespace EventStoreInOneHour;

public static class EventStoreExtensions
{
    private const string Apply = "Apply";

    public static Task AppendEventsAsync<TStream>(
        this IEventStore eventStore,
        Guid streamId,
        params object[] events
    ) where TStream : notnull =>
        eventStore.AppendEventsAsync<TStream>(streamId, events);

    public static async Task<T?> AggregateStreamAsync<T>(
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
        var version = 0;

        T? aggregate = default;
        foreach (var @event in events)
        {
            aggregate = evolve(aggregate ?? getDefault(), @event);
            aggregate.SetIfExists(nameof(IAggregate.Version), ++version);
        }

        return aggregate;
    }

    public static Task<T?> AggregateStreamAsync<T>(
        this IEventStore eventStore,
        Guid streamId,
        long? atStreamVersion = null,
        DateTime? atTimestamp = null,
        CancellationToken ct = default
    ) where T : notnull =>
        eventStore.AggregateStreamAsync<T>(
            (aggregate, @event) =>
            {
                aggregate.InvokeIfExists(Apply, @event);
                return aggregate;
            },
            streamId,
            atStreamVersion,
            atTimestamp,
            ct
        );

    public static Task<T?> AggregateStreamAsync<T>(
        this IEventStore eventStore,
        Func<T, object, T> evolve,
        Guid streamId,
        long? atStreamVersion = null,
        DateTime? atTimestamp = null,
        CancellationToken ct = default
    ) where T : notnull =>
        eventStore.AggregateStreamAsync(
            () => (T)Activator.CreateInstance(typeof(T), true)!,
            evolve,
            streamId,
            atStreamVersion,
            atTimestamp,
            ct
        );
}
