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
    ) where T : notnull
    {
        var aggregate = (T)Activator.CreateInstance(typeof(T), true)!;

        var events = eventStore.GetEvents(streamId, atStreamVersion, atTimestamp);
        var version = 0;

        foreach (var @event in events)
        {
            aggregate.InvokeIfExists(Apply, @event);
            aggregate.SetIfExists(nameof(IAggregate.Version), ++version);
        }

        return aggregate;
    }
}
