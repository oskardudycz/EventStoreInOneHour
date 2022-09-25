using System.Collections;

namespace EventStoreInOneHour;

public interface IEventStore
{
    void Init();
    void AddProjection(IProjection projection);
    bool AppendEvent<TStream>(Guid streamId, object @event, long? expectedVersion = null) where TStream : notnull;
    StreamState? GetStreamState(Guid streamId);
    IEnumerable GetEvents(Guid streamId, long? atStreamVersion = null, DateTime? atTimestamp = null);
}
