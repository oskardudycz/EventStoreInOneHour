using System;
using System.Linq;
using EventStoreInOneHour.Tests.Tools;
using FluentAssertions;
using Npgsql;
using Xunit;

namespace EventStoreInOneHour.Tests
{
    public class Exercise04EventStoreMethods
    {
        /// <summary>
        ///     Inits Event Store
        /// </summary>
        public Exercise04EventStoreMethods()
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
        public void GetEvents_ShouldReturnAppendedEvents()
        {
            var now = DateTime.UtcNow;

            var bankAccountId = Guid.NewGuid();
            var accountNumber = "PL61 1090 1014 0000 0712 1981 2874";
            var clientId = Guid.NewGuid();
            var currencyISOCOde = "PLN";

            var bankAccountCreated = new BankAccountCreated(
                bankAccountId,
                accountNumber,
                clientId,
                currencyISOCOde,
                now
            );

            var cashierId = Guid.NewGuid();
            var depositRecorded = new DepositRecorded(bankAccountId, 100, cashierId, now);

            var atmId = Guid.NewGuid();
            var cashWithdrawn = new CashWithdrawnFromATM(bankAccountId, 50, atmId, now);

            eventStore.AppendEvent<BankAccount>(bankAccountId, bankAccountCreated);
            eventStore.AppendEvent<BankAccount>(bankAccountId, depositRecorded);
            eventStore.AppendEvent<BankAccount>(bankAccountId, cashWithdrawn);

            var events = eventStore.GetEvents(bankAccountId);

            events.Should().HaveCount(3);

            events.OfType<BankAccountCreated>().Should().Contain(
                e => e.BankAccountId == bankAccountId && e.AccountNumber == accountNumber
                                                      && e.ClientId == clientId && e.CurrencyISOCode == currencyISOCOde
                                                      && e.CreatedAt == now);

            events.OfType<BankAccountCreated>().Should().Contain(
                e => e.BankAccountId == bankAccountId && e.AccountNumber == accountNumber
                                                      && e.ClientId == clientId && e.CurrencyISOCode == currencyISOCOde
                                                      && e.CreatedAt == now);

            events.OfType<BankAccountCreated>().Should().Contain(
                e => e.BankAccountId == bankAccountId && e.AccountNumber == accountNumber
                                                      && e.ClientId == clientId && e.CurrencyISOCode == currencyISOCOde
                                                      && e.CreatedAt == now);
        }

        [Fact]
        public void GetStreamState_ShouldReturnProperStreamInfo()
        {
            var bankAccountId = Guid.NewGuid();
            var accountNumber = "PL61 1090 1014 0000 0712 1981 2874";
            var clientId = Guid.NewGuid();
            var currencyISOCOde = "PLN";

            var bankAccountCreated = new BankAccountCreated(
                bankAccountId,
                accountNumber,
                clientId,
                currencyISOCOde,
                DateTime.Now
            );
            eventStore.AppendEvent<BankAccount>(bankAccountId, bankAccountCreated);

            var streamState = eventStore.GetStreamState(bankAccountId);

            streamState.Id.Should().Be(bankAccountId);
            streamState.Type.Should().Be(typeof(BankAccount));
            streamState.Version.Should().Be(1);
        }
    }
}
