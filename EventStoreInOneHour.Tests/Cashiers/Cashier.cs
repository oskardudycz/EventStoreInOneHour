namespace EventStoreInOneHour.Tests.Cashiers;

public record CashierEmployed(
    Guid CashierId,
    string Name
);

public record Cashier(
    Guid Id,
    string Name
)
{
    public static Cashier Evolve(Cashier cashier, object @event)
    {
        return @event switch
        {
            CashierEmployed cashierCreated =>
                Create(cashierCreated),
            _ => cashier
        };
    }

    public static Cashier Create(CashierEmployed @event) =>
        new Cashier(
            @event.CashierId,
            @event.Name
        );
}
