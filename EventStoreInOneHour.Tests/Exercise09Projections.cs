using Dapper;
using Dapper.Contrib.Extensions;
using EventStoreInOneHour.Tests.BankAccounts;
using EventStoreInOneHour.Tests.Cashiers;
using EventStoreInOneHour.Tests.Tools;
using FluentAssertions;
using Npgsql;
using Xunit;

namespace EventStoreInOneHour.Tests;

public class Exercise09Projections
{
    private readonly NpgsqlConnection dbConnection;
    private readonly EventStore eventStore;

    /// <summary>
    /// Inits Event Store
    /// </summary>
    public Exercise09Projections()
    {
        dbConnection = PostgresDbConnectionProvider.GetFreshDbConnection();

        // Create Event Store
        eventStore = new EventStore(dbConnection);

        eventStore.RegisterProjection(new CashierDashboardProjection(dbConnection));
        eventStore.RegisterProjection(new BankAccountDetailsProjection(dbConnection));

        // Initialize Event Store
        eventStore.Init();
    }

    [Fact]
    public async Task AddingAndUpdatingAggregate_ShouldCreateAndUpdateSnapshotAccordingly()
    {
        var cashier1 = new CashierEmployed(Guid.NewGuid(), "John Doe");
        var cashier2 = new CashierEmployed(Guid.NewGuid(), "Emily Rose");
        await eventStore.AppendEventsAsync<Cashier>(cashier1.CashierId, new[] { cashier1 });
        await eventStore.AppendEventsAsync<Cashier>(cashier2.CashierId, new[] { cashier2 });

        var bankAccountId = Guid.NewGuid();
        var accountNumber = "PL61 1090 1014 0000 0712 1981 2874";
        var currencyISOCOde = "PLN";

        await eventStore.Handle(
            bankAccountId,
            new OpenBankAccount(
                bankAccountId,
                accountNumber,
                Guid.NewGuid(),
                currencyISOCOde
            )
        );

        await eventStore.Handle(
            bankAccountId,
            new RecordDeposit(
                100,
                cashier1.CashierId
            )
        );

        await eventStore.Handle(
            bankAccountId,
            new RecordDeposit(
                10,
                cashier2.CashierId
            )
        );

        var otherBankAccountId = Guid.NewGuid();
        var otherAccountNumber = "PL61 1090 1014 0000 0712 1981 3000";

        await eventStore.Handle(
            bankAccountId,
            new OpenBankAccount(
                otherBankAccountId,
                otherAccountNumber,
                Guid.NewGuid(),
                "PLN"
            )
        );

        await eventStore.Handle(
            bankAccountId,
            new RecordDeposit(
                13,
                cashier1.CashierId
            )
        );

        var cashier1Dashboard = dbConnection.Get<CashierDashboard>(cashier1.CashierId);

        cashier1Dashboard.Should().NotBeNull();
        cashier1Dashboard.Id.Should().Be(cashier1.CashierId);
        cashier1Dashboard.CashierName.Should().Be(cashier1.Name);
        cashier1Dashboard.RecordedDepositsCount.Should().Be(2);
        cashier1Dashboard.TotalBalance.Should().Be(113);

        var cashier2Dashboard = dbConnection.Get<CashierDashboard>(cashier2.CashierId);

        cashier2Dashboard.Should().NotBeNull();
        cashier2Dashboard.Id.Should().Be(cashier2.CashierId);
        cashier2Dashboard.CashierName.Should().Be(cashier2.Name);
        cashier2Dashboard.RecordedDepositsCount.Should().Be(1);
        cashier2Dashboard.TotalBalance.Should().Be(10);

        var bankAccount = dbConnection.Get<BankAccountDetails>(bankAccountId);


        var otherBankAccount = dbConnection.Get<BankAccountDetails>(otherBankAccountId);
    }
}
