namespace EventStoreInOneHour;

public interface ISnapshot
{
    Type Handles { get; }
    void Handle(IAggregate aggregate);
}
