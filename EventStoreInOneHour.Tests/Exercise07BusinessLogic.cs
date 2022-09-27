using EventStoreInOneHour.Tests.BankAccounts;
using EventStoreInOneHour.Tests.Tools;
using FluentAssertions;
using Npgsql;
using Xunit;

namespace EventStoreInOneHour.Tests;

public class Exercise07BusinessLogic
{
    /// <summary>
    ///     Inits Event Store
    /// </summary>
    public Exercise07BusinessLogic()
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
    public async Task Repository_FullFlow_ShouldSucceed()
    {
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

        var bankAccount = await eventStore.GetBankAccount(bankAccountId);

        bankAccount.Should().NotBeNull();
        bankAccount.Id.Should().Be(bankAccountId);
        bankAccount.Version.Should().Be(1);
        bankAccount.Balance.Should().Be(0);

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
