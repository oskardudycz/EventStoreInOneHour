using System.Linq.Expressions;
using Dapper;
using Dapper.Contrib.Extensions;
using EventStoreInOneHour.Tests.BankAccounts;
using EventStoreInOneHour.Tests.Tools;
using FluentAssertions;
using Npgsql;
using SimpleMigrations;
using SimpleMigrations.DatabaseProvider;
using Xunit;

namespace EventStoreInOneHour.Tests;

public class Exercise08Snapshots
{
    [Migration(1, "Create Users table")]
    public class CreateUsers : Migration
    {
        protected override void Up()
        {
            Execute(@"CREATE TABLE bankaccounts (
                      id              UUID                      NOT NULL    PRIMARY KEY,
                      accountNumber   TEXT                      NOT NULL,
                      clientId        UUID                      NOT NULL,
                      currencyISOCode TEXT                      NOT NULL,
                      balance         DECIMAL                   NOT NULL,
                      createdAt       timestamp                 NOT NULL,
                      version         BIGINT                    NOT NULL
                  );");
        }

        protected override void Down()
        {
            Execute("DROP TABLE bankaccounts");
        }
    }

    private readonly NpgsqlConnection databaseConnection;
    private readonly EventStore eventStore;
    private readonly IRepository<BankAccount> repository;

    /// <summary>
    /// Inits Event Store
    /// </summary>
    public Exercise08Snapshots()
    {
        databaseConnection = PostgresDbConnectionProvider.GetFreshDbConnection();

        var databaseProvider =
            new PostgresqlDatabaseProvider(databaseConnection) {SchemaName = typeof(Exercise08Snapshots).Name};

        var migrationsAssembly = typeof(Exercise08Snapshots).Assembly;
        var migrator = new SimpleMigrator(migrationsAssembly, databaseProvider);
        migrator.Load();
        migrator.MigrateToLatest();

        // Create Event Store
        eventStore = new EventStore(databaseConnection);

        // Initialize Event Store
        eventStore.Init();

        var userSnapshot = new SnapshotToTable<BankAccount>(
            databaseConnection,
            @"INSERT INTO bankaccounts (id, accountNumber, clientId, currencyISOCode, balance, createdAt, version) VALUES (@Id, @AccountNumber, @ClientId, @CurrencyISOCode, @Balance, @CreatedAt, @Version)
                 ON CONFLICT (id)
                 DO UPDATE SET Balance = @Balance, version = @Version");

        repository = new Repository<BankAccount>(eventStore);
        repository.RegisterSnapshot(userSnapshot);
    }

    [Fact]
    public async Task AddingAndUpdatingAggregate_ShouldCreateAndUpdateSnapshotAccordingly()
    {
        var timeBeforeCreate = DateTime.UtcNow;
        var bankAccountId = Guid.NewGuid();
        var accountNumber = "PL61 1090 1014 0000 0712 1981 2874";
        var clientId = Guid.NewGuid();
        var currencyISOCOde = "PLN";

        var bankAccount = BankAccount.Open(
            bankAccountId,
            accountNumber,
            clientId,
            currencyISOCOde
        );

        await repository.AddAsync(bankAccount);

        var snapshot = databaseConnection.Get<BankAccount>(bankAccountId);

        snapshot.Id.Should().Be(bankAccountId);
        snapshot.Version.Should().Be(1);
        snapshot.AccountNumber.Should().Be(accountNumber);
        snapshot.ClientId.Should().Be(clientId);
        snapshot.CurrencyISOCode.Should().Be(currencyISOCOde);
        snapshot.CreatedAt.Should().BeAfter(timeBeforeCreate);
        snapshot.Balance.Should().Be(0);

        var cashierId = Guid.NewGuid();
        var depositAmount = 100;

        snapshot.RecordDeposit(depositAmount, cashierId);

        await repository.UpdateAsync(snapshot);

        var snapshotAfterUpdate = databaseConnection.Get<BankAccount>(bankAccountId);

        snapshotAfterUpdate.Id.Should().Be(bankAccountId);
        snapshotAfterUpdate.Balance.Should().Be(depositAmount);
        snapshotAfterUpdate.Version.Should().Be(2);
    }

    [Fact]
    public async Task Snapshots_ShouldBeQueryable()
    {
        var firstMatchingBankAccount = BankAccount.Open(
            Guid.NewGuid(),
            Guid.NewGuid().ToString(),
            Guid.NewGuid(),
            "PLN"
        );
        var secondMatchingAccount = BankAccount.Open(
            Guid.NewGuid(),
            Guid.NewGuid().ToString(),
            Guid.NewGuid(),
            "USD"
        );
        var thirdMatchingAccount = BankAccount.Open(
            Guid.NewGuid(),
            Guid.NewGuid().ToString(),
            Guid.NewGuid(),
            "EUR"
        );

        await repository.AddAsync(firstMatchingBankAccount);
        await repository.AddAsync(secondMatchingAccount);
        await repository.AddAsync(thirdMatchingAccount);


        var bankAccounts = databaseConnection.Query<BankAccount>(
            @"SELECT id, accountNumber, clientId, currencyISOCode, balance, createdAt, version
                    FROM bankaccounts");

        bankAccounts.Count().Should().Be(3);

        Expression<Func<BankAccount, bool>> bankAccountEqualsTo(BankAccount bankAccountToCompare)
        {
            return  e => e.Id == bankAccountToCompare.Id
                         && e.AccountNumber == bankAccountToCompare.AccountNumber
                         && e.ClientId == bankAccountToCompare.ClientId
                         && e.CurrencyISOCode == bankAccountToCompare.CurrencyISOCode
                         && e.Balance == bankAccountToCompare.Balance;
        }

        bankAccounts.Should().Contain(bankAccountEqualsTo(firstMatchingBankAccount));
        bankAccounts.Should().Contain(bankAccountEqualsTo(secondMatchingAccount));
        bankAccounts.Should().Contain(bankAccountEqualsTo(thirdMatchingAccount));
    }
}
