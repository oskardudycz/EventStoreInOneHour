using EventStoreInOneHour.Tests.BankAccounts;
using EventStoreInOneHour.Tests.Tools;
using FluentAssertions;
using Npgsql;
using Xunit;

namespace EventStoreInOneHour.Tests;

public class Exercise07AggregateAndRepository
{
    /// <summary>
    ///     Inits Event Store
    /// </summary>
    public Exercise07AggregateAndRepository()
    {
        databaseConnection = PostgresDbConnectionProvider.GetFreshDbConnection();

        // Create Event Store
        eventStore = new EventStore(databaseConnection);

        // Initialize Event Store
        eventStore.Init();
    }

    private readonly NpgsqlConnection databaseConnection;
    private readonly EventStore eventStore;

    [Fact]
    public async Task Repository_FullFlow_ShouldSucceed()
    {
        var timeBeforeCreate = DateTime.UtcNow;
        var bankAccountId = Guid.NewGuid();
        var accountNumber = "PL61 1090 1014 0000 0712 1981 2874";
        var clientId = Guid.NewGuid();
        var currencyISOCOde = "PLN";

        await eventStore.Handle(
            bankAccountId,
            new OpenBankAccount(
                bankAccountId,
                accountNumber,
                clientId,
                currencyISOCOde
            )
        );

        var bankAccountFromRepository = await eventStore.GetBankAccount(bankAccountId);

        bankAccountFromRepository.Should().NotBeNull();
        bankAccountFromRepository!.Id.Should().Be(bankAccountId);
        bankAccountFromRepository.Version.Should().Be(1);
        bankAccountFromRepository.AccountNumber.Should().Be(accountNumber);
        bankAccountFromRepository.ClientId.Should().Be(clientId);
        bankAccountFromRepository.CurrencyISOCode.Should().Be(currencyISOCOde);
        bankAccountFromRepository.CreatedAt.Should().BeAfter(timeBeforeCreate);
        bankAccountFromRepository.Balance.Should().Be(0);

        var cashierId = Guid.NewGuid();
        var depositAmount = 100;

        await eventStore.Handle(
            bankAccountId,
            new RecordDeposit(
                depositAmount,
                cashierId
            )
        );

        var bankAccountAfterDeposit = await eventStore.GetBankAccount(bankAccountId);

        bankAccountAfterDeposit.Should().NotBeNull();
        bankAccountAfterDeposit!.Id.Should().Be(bankAccountId);
        bankAccountAfterDeposit.Balance.Should().Be(depositAmount);
        bankAccountAfterDeposit.Version.Should().Be(2);
    }
}
