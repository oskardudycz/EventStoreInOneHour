namespace EventStoreInOneHour.Tests.Cashiers;

public record CashierCreated(
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
            CashierCreated cashierCreated =>
                Create(cashierCreated),
            _ => cashier
        };
    }

    public static Cashier Create(CashierCreated @event) =>
        new Cashier(
            @event.CashierId,
            @event.Name
        );
}
