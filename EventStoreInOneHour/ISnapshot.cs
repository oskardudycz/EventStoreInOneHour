namespace EventStoreInOneHour;

public interface ISnapshot
{
    void Handle(IAggregate aggregate);
}
