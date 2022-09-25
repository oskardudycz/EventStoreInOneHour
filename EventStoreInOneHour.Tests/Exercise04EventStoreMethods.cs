using EventStoreInOneHour.Tests.BankAccounts;
using EventStoreInOneHour.Tests.Tools;
using FluentAssertions;
using Npgsql;
using Xunit;

namespace EventStoreInOneHour.Tests;

public class Exercise04EventStoreMethods
{
    /// <summary>
    ///     Inits Event Store
    /// </summary>
    public Exercise04EventStoreMethods()
    {
        dbConnection = PostgresDbConnectionProvider.GetFreshDbConnection();

        // Create Event Store
        eventStore = new EventStore(dbConnection);

        // Initialize Event Store
        eventStore.Init();
    }

    private readonly NpgsqlConnection dbConnection;
    private readonly EventStore eventStore;

    [Fact]
    public async Task GetEvents_ShouldReturnAppendedEvents()
    {
        var now = DateTime.UtcNow;

        var bankAccountId = Guid.NewGuid();
        var accountNumber = "PL61 1090 1014 0000 0712 1981 2874";
        var clientId = Guid.NewGuid();
        var currencyISOCOde = "PLN";
        var version = 1;

        var bankAccountCreated = new BankAccountOpened(
            bankAccountId,
            accountNumber,
            clientId,
            currencyISOCOde,
            now,
            version
        );

        var cashierId = Guid.NewGuid();
        var depositRecorded = new DepositRecorded(bankAccountId, 100, cashierId, now, ++version);

        var atmId = Guid.NewGuid();
        var cashWithdrawn = new CashWithdrawnFromATM(bankAccountId, 50, atmId, now, ++version);

        await eventStore.AppendEventsAsync<BankAccount>(
            bankAccountId,
            new object[] { bankAccountCreated, depositRecorded, cashWithdrawn }
        );

        var events = await eventStore.GetEventsAsync(bankAccountId);

        events.Should().HaveCount(3);

        events.OfType<BankAccountOpened>().Should().Contain(
            e => e.BankAccountId == bankAccountId && e.AccountNumber == accountNumber
                                                  && e.ClientId == clientId && e.CurrencyISOCode == currencyISOCOde
                                                  && e.CreatedAt == now);

        events.OfType<BankAccountOpened>().Should().Contain(
            e => e.BankAccountId == bankAccountId && e.AccountNumber == accountNumber
                                                  && e.ClientId == clientId && e.CurrencyISOCode == currencyISOCOde
                                                  && e.CreatedAt == now);

        events.OfType<BankAccountOpened>().Should().Contain(
            e => e.BankAccountId == bankAccountId && e.AccountNumber == accountNumber
                                                  && e.ClientId == clientId && e.CurrencyISOCode == currencyISOCOde
                                                  && e.CreatedAt == now);
    }
}
