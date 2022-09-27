// using EventStoreInOneHour.Tests.BankAccounts;
// using EventStoreInOneHour.Tests.Tools;
// using FluentAssertions;
// using Npgsql;
// using Xunit;
//
// namespace EventStoreInOneHour.Tests;
//
// public class Exercise05StreamAggregation
// {
//     private readonly NpgsqlConnection dbConnection;
//     private readonly EventStore eventStore;
//
//     /// <summary>
//     /// Inits Event Store
//     /// </summary>
//     public Exercise05StreamAggregation()
//     {
//         dbConnection = PostgresDbConnectionProvider.GetFreshDbConnection();
//
//         // Create Event Store
//         eventStore = new EventStore(dbConnection);
//
//         // Initialize Event Store
//         eventStore.Init();
//     }
//
//     [Fact]
//     public async Task AggregateStream_ShouldReturnObjectWithStateBasedOnEvents()
//     {
//         var bankAccountId = Guid.NewGuid();
//         var accountNumber = "PL61 1090 1014 0000 0712 1981 2874";
//         var clientId = Guid.NewGuid();
//         var currencyISOCOde = "PLN";
//         var createdAt = DateTime.UtcNow;
//         var version = 1;
//
//         var bankAccountCreated = new BankAccountOpened(
//             bankAccountId,
//             accountNumber,
//             clientId,
//             currencyISOCOde,
//             createdAt,
//             version
//         );
//
//         var cashierId = Guid.NewGuid();
//         var depositRecorded = new DepositRecorded(bankAccountId, 100, cashierId, DateTime.UtcNow, ++version);
//
//         var atmId = Guid.NewGuid();
//         var cashWithdrawn = new CashWithdrawnFromATM(bankAccountId, 50, atmId, DateTime.UtcNow, ++version);
//
//         await eventStore.AppendEventsAsync<BankAccount>(
//             bankAccountId,
//             new object[] { bankAccountCreated, depositRecorded, cashWithdrawn }
//         );
//
//         var bankAccount = await eventStore.GetBankAccount(bankAccountId);
//
//         bankAccount.Id.Should().Be(bankAccountId);
//         bankAccount.Version.Should().Be(3);
//         bankAccount.Balance.Should().Be(50);
//     }
// }
