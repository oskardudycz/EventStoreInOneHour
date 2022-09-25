namespace EventStoreInOneHour.Tests;

public class CashierCreated
{
    public Guid CashierId { get; }
    public string Name { get; }

    public CashierCreated(
        Guid cashierId,
        string name
    )
    {
        CashierId = cashierId;
        Name = name;
    }
}

public class Cashier: Aggregate
{
    public string Name { get; private set; } = default!;

    // for dapper
    public Cashier()
    {
    }

    private Cashier(
        Guid cashierId,
        string name
    )
    {
        var @event = new CashierCreated(cashierId, name);

        Enqueue(@event);
        Apply(@event);
    }

    public static Cashier Create(
        Guid cashierId,
        string name
    )
    {
        return new Cashier(cashierId, name);
    }

    public void Apply(CashierCreated @event)
    {
        Id = @event.CashierId;
        Name = @event.Name;
    }
}
