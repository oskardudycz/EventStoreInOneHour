using EventStoreInOneHour.Tools;

namespace EventStoreInOneHour;

public static class EventStoreExtensions
{
    private const string Apply = "Apply";

    public static T AggregateStream<T>(
        this IEventStore eventStore,
        Guid streamId,
        long? atStreamVersion = null,
        DateTime? atTimestamp = null
    ) where T : notnull =>
        eventStore.AggregateStream<T>(
            (aggregate, @event) =>
            {
                aggregate.InvokeIfExists(Apply, @event);
                return aggregate;
            },
            streamId,
            atStreamVersion,
            atTimestamp
        );

    public static T AggregateStream<T>(
        this IEventStore eventStore,
        Func<T, object, T> evolve,
        Guid streamId,
        long? atStreamVersion = null,
        DateTime? atTimestamp = null
    ) where T : notnull =>
        eventStore.AggregateStream(
            () => (T)Activator.CreateInstance(typeof(T), true)!,
            evolve,
            streamId,
            atStreamVersion,
            atTimestamp
        );

    public static T AggregateStream<T>(
        this IEventStore eventStore,
        Func<T> getDefault,
        Func<T, object, T> evolve,
        Guid streamId,
        long? atStreamVersion = null,
        DateTime? atTimestamp = null
    ) where T : notnull
    {
        var aggregate = getDefault();

        var events = eventStore.GetEvents(streamId, atStreamVersion, atTimestamp);
        var version = 0;

        foreach (var @event in events)
        {
            aggregate = evolve(aggregate, @event);
            aggregate.SetIfExists(nameof(IAggregate.Version), ++version);
        }

        return aggregate;
    }
}
