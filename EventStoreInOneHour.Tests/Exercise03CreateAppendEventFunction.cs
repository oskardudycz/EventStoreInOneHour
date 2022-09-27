// using Dapper;
// using EventStoreInOneHour.Tests.BankAccounts;
// using EventStoreInOneHour.Tests.Tools;
// using FluentAssertions;
// using Npgsql;
// using Xunit;
//
// namespace EventStoreInOneHour.Tests;
//
// public class Exercise03CreateAppendEventFunction
// {
//     private readonly NpgsqlConnection dbConnection;
//     private readonly PostgresSchemaProvider schemaProvider;
//     private readonly EventStore eventStore;
//
//
//     private const string AppendEventFunctionName = "append_event";
//
//     /// <summary>
//     /// Inits Event Store
//     /// </summary>
//     public Exercise03CreateAppendEventFunction()
//     {
//         dbConnection = PostgresDbConnectionProvider.GetFreshDbConnection();
//         schemaProvider = new PostgresSchemaProvider(dbConnection);
//
//         // Create Event Store
//         eventStore = new EventStore(dbConnection);
//
//         // Initialize Event Store
//         eventStore.Init();
//     }
//
//     [Fact]
//     public void AppendEventFunction_ShouldBeCreated()
//     {
//         var appendFunctionExists = schemaProvider
//             .FunctionExists(AppendEventFunctionName);
//
//         appendFunctionExists.Should().BeTrue();
//     }
//
//     [Fact]
//     public async Task AppendEventFunction_WhenStreamDoesNotExist_CreateNewStream_And_AppendNewEvent()
//     {
//         var bankAccountId = Guid.NewGuid();
//         var accountNumber = "PL61 1090 1014 0000 0712 1981 2874";
//         var clientId = Guid.NewGuid();
//         var currencyISOCOde = "PLN";
//
//         var @event = new BankAccountOpened(
//             bankAccountId,
//             accountNumber,
//             clientId,
//             currencyISOCOde,
//             DateTime.Now,
//             1
//         );
//
//         await eventStore.AppendEventsAsync<BankAccount>(bankAccountId, new object[] { @event });
//
//         var wasStreamCreated = dbConnection.QuerySingle<bool>(
//             "select exists (select 1 from streams where id = @streamId)", new { streamId = bankAccountId }
//         );
//         wasStreamCreated.Should().BeTrue();
//
//         var wasEventAppended = dbConnection.QuerySingle<bool>(
//             "select exists (select 1 from events where stream_id = @streamId)", new { streamId = bankAccountId }
//         );
//         wasEventAppended.Should().BeTrue();
//     }
// }
