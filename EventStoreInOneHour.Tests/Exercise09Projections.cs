using System;
using Dapper;
using Dapper.Contrib.Extensions;
using EventStoreInOneHour.Tests.Tools;
using FluentAssertions;
using Npgsql;
using SimpleMigrations;
using SimpleMigrations.DatabaseProvider;
using Xunit;

namespace EventStoreInOneHour.Tests
{
    public class Exercise09Projections
    {

        public class CashierDashboardProjection : Projection
        {
            private readonly NpgsqlConnection databaseConnection;

            public CashierDashboardProjection(NpgsqlConnection databaseConnection)
            {
                this.databaseConnection = databaseConnection;

                Projects<CashierCreated>(Apply);
                Projects<DepositRecorded>(Apply);
            }

            void Apply(CashierCreated @event)
            {
                databaseConnection.Execute(
                    @"INSERT INTO CashierDashboards (Id, CashierName, RecordedDepositsCount, TotalBalance)
                    VALUES (@CashierId, @Name, 0, 0)",
                    @event
                 );
            }

            void Apply(DepositRecorded @event)
            {
                databaseConnection.Execute(
                    @"UPDATE CashierDashboards
                    SET TotalBalance = TotalBalance + @Amount, RecordedDepositsCount = RecordedDepositsCount + 1
                    WHERE Id = @CashierId",
                    @event
                );
            }
        }

        [Migration(2, "Create Users dashboard table")]
        public class CreateUsersDashboard : Migration
        {
            protected override void Up()
            {
                Execute(@"CREATE TABLE CashierDashboards (
                      Id             UUID                      NOT NULL    PRIMARY KEY,
                      CashierName     TEXT                      NOT NULL,
                      RecordedDepositsCount  integer                   NOT NULL,
                      TotalBalance   decimal                   NOT NULL
                  );");
            }

            protected override void Down()
            {
                Execute("DROP TABLE CashierDashboards");
            }
        }

        private readonly NpgsqlConnection databaseConnection;
        private readonly EventStore eventStore;
        private readonly IRepository<Cashier> clientRepository;
        private readonly IRepository<BankAccount> bankAccountRepository;

        /// <summary>
        /// Inits Event Store
        /// </summary>
        public Exercise09Projections()
        {
            databaseConnection = PostgresDbConnectionProvider.GetFreshDbConnection();

            var databaseProvider =
                new PostgresqlDatabaseProvider(databaseConnection) {SchemaName = typeof(Exercise09Projections).Name};

            var migrationsAssembly = typeof(Exercise09Projections).Assembly;
            var migrator = new SimpleMigrator(migrationsAssembly, databaseProvider);
            migrator.Load();
            migrator.MigrateToLatest();

            // Create Event Store
            eventStore = new EventStore(databaseConnection);

            eventStore.AddProjection(new CashierDashboardProjection(databaseConnection));

            // Initialize Event Store
            eventStore.Init();

            clientRepository = new Repository<Cashier>(eventStore);
            bankAccountRepository = new Repository<BankAccount>(eventStore);
        }

        [Fact]
        public void AddingAndUpdatingAggregate_ShouldCreateAndUpdateSnapshotAccordingly()
        {
            var cashier1 = Cashier.Create(Guid.NewGuid(), "John Doe");
            var cashier2 = Cashier.Create(Guid.NewGuid(), "Emily Rose");
            clientRepository.Add(cashier1);
            clientRepository.Add(cashier2);

            var bankAccountId = Guid.NewGuid();
            var accountNumber = "PL61 1090 1014 0000 0712 1981 2874";
            var currencyISOCOde = "PLN";

            var bankAccount = BankAccount.Open(
                bankAccountId,
                accountNumber,
                Guid.NewGuid(),
                currencyISOCOde
            );
            bankAccountRepository.Add(bankAccount);

            bankAccount.RecordDeposit(100, cashier1.Id);
            bankAccountRepository.Update(bankAccount);

            bankAccount.RecordDeposit(10, cashier2.Id);
            bankAccountRepository.Update(bankAccount);

            var otherBankAccountId = Guid.NewGuid();
            var otherAccountNumber = "PL61 1090 1014 0000 0712 1981 3000";

            var otherAccount = BankAccount.Open(
                otherBankAccountId,
                otherAccountNumber,
                Guid.NewGuid(),
                "PLN"
            );
            bankAccountRepository.Add(bankAccount);

            otherAccount.RecordDeposit(13, cashier1.Id);
            bankAccountRepository.Update(otherAccount);

            var cashier1Dashboard = databaseConnection.Get<CashierDashboard>(cashier1.Id);

            cashier1Dashboard.Should().NotBeNull();
            cashier1Dashboard.Id.Should().Be(cashier1.Id);
            cashier1Dashboard.CashierName.Should().Be(cashier1.Name);
            cashier1Dashboard.RecordedDepositsCount.Should().Be(2);
            cashier1Dashboard.TotalBalance.Should().Be(113);


            var cashier2Dashboard = databaseConnection.Get<CashierDashboard>(cashier2.Id);

            cashier2Dashboard.Should().NotBeNull();
            cashier2Dashboard.Id.Should().Be(cashier2.Id);
            cashier2Dashboard.CashierName.Should().Be(cashier2.Name);
            cashier2Dashboard.RecordedDepositsCount.Should().Be(1);
            cashier2Dashboard.TotalBalance.Should().Be(10);
        }
    }
}
