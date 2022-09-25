using EventStoreInOneHour.Tests.BankAccounts;
using EventStoreInOneHour.Tests.Tools;
using FluentAssertions;
using Npgsql;
using Xunit;

namespace EventStoreInOneHour.Tests;

public class Exercise06TimeTravelling
{
    private readonly NpgsqlConnection dbConnection;
    private readonly EventStore eventStore;

    /// <summary>
    /// Inits Event Store
    /// </summary>
    public Exercise06TimeTravelling()
    {
        dbConnection = PostgresDbConnectionProvider.GetFreshDbConnection();

        // Create Event Store
        eventStore = new EventStore(dbConnection);

        // Initialize Event Store
        eventStore.Init();
    }

    [Fact]
    public async Task AggregateStream_ShouldReturnSpecifiedVersionOfTheStream()
    {
        var bankAccountId = Guid.NewGuid();
        var accountNumber = "PL61 1090 1014 0000 0712 1981 2874";
        var clientId = Guid.NewGuid();
        var currencyISOCOde = "PLN";
        var createdAt = DateTime.UtcNow;
        var version = 1;

        var bankAccountCreated = new BankAccountOpened(
            bankAccountId,
            accountNumber,
            clientId,
            currencyISOCOde,
            createdAt,
            version
        );

        var cashierId = Guid.NewGuid();
        var depositRecorded = new DepositRecorded(bankAccountId, 100, cashierId, DateTime.UtcNow, ++version);

        var atmId = Guid.NewGuid();
        var cashWithdrawn = new CashWithdrawnFromATM(bankAccountId, 50, atmId, DateTime.UtcNow, ++version);

        await eventStore.AppendEventsAsync<BankAccount>(
            bankAccountId,
            new object[] { bankAccountCreated, depositRecorded, cashWithdrawn }
        );

        var aggregateAtVersion1 = await eventStore.GetBankAccount(bankAccountId, 1);

        aggregateAtVersion1.Should().NotBeNull();
        aggregateAtVersion1!.Id.Should().Be(bankAccountId);
        aggregateAtVersion1.Balance.Should().Be(0);
        aggregateAtVersion1.Version.Should().Be(1);


        var aggregateAtVersion2 = await eventStore.GetBankAccount(bankAccountId, 2);

        aggregateAtVersion2.Should().NotBeNull();
        aggregateAtVersion2!.Id.Should().Be(bankAccountId);
        aggregateAtVersion2.Balance.Should().Be(depositRecorded.Amount);
        aggregateAtVersion2.Version.Should().Be(2);


        var aggregateAtVersion3 = await eventStore.GetBankAccount(bankAccountId, 3);

        aggregateAtVersion3.Should().NotBeNull();
        aggregateAtVersion3!.Id.Should().Be(bankAccountId);
        aggregateAtVersion3.Balance.Should().Be(depositRecorded.Amount - cashWithdrawn.Amount);
        aggregateAtVersion3.Version.Should().Be(3);
    }
}
