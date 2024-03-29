using EventStoreInOneHour.Tests.Tools;
using FluentAssertions;
using Npgsql;
using Xunit;

namespace EventStoreInOneHour.Tests;

public class Exercise01CreateStreamsTable: IDisposable
{
    private readonly NpgsqlConnection dbConnection;
    private readonly PostgresSchemaProvider schemaProvider;

    private const string StreamsTableName = "streams";

    private const string IdColumnName = "id";
    private const string TypeColumnName = "type";
    private const string VersionColumnName = "version";

    /// <summary>
    /// Inits Event Store
    /// </summary>
    public Exercise01CreateStreamsTable()
    {
        dbConnection = PostgresDbConnectionProvider.GetFreshDbConnection();
        schemaProvider = new PostgresSchemaProvider(dbConnection);

        // Create Event Store
        var eventStore = new EventStore(dbConnection);

        // Initialize Event Store
        eventStore.Init();
    }

    /// <summary>
    /// Verifies if Stream table was created
    /// </summary>
    [Fact]
    public void StreamsTable_ShouldBeCreated()
    {
        var streamsTable = schemaProvider.GetTable(StreamsTableName);

        streamsTable.Should().NotBeNull();
        streamsTable!.Name.Should().Be(StreamsTableName);
    }

    /// <summary>
    /// Verifies if Stream table has Id column of type Guid
    /// </summary>
    [Fact]
    public void StreamsTable_ShouldHave_IdColumn()
    {
        var idColumn = schemaProvider
            .GetTable(StreamsTableName)?
            .GetColumn(IdColumnName);

        idColumn.Should().NotBeNull();
        idColumn!.Name.Should().Be(IdColumnName);
        idColumn.Type.Should().Be(Column.GuidType);
    }

    /// <summary>
    /// Verifies if Stream table has Type column of type String
    /// </summary>
    [Fact]
    public void StreamsTable_ShouldHave_TypeColumn_WithStringType()
    {
        var typeColumn = schemaProvider
            .GetTable(StreamsTableName)?
            .GetColumn(TypeColumnName);

        typeColumn.Should().NotBeNull();
        typeColumn!.Name.Should().Be(TypeColumnName);
        typeColumn.Type.Should().Be(Column.StringType);
    }

    /// <summary>
    /// Verifies if Stream table has Version column of type Long
    /// </summary>
    [Fact]
    public void StreamsTable_ShouldHave_VersionColumn_WithLongType()
    {
        var versionColumn = schemaProvider
            .GetTable(StreamsTableName)?
            .GetColumn(VersionColumnName);

        versionColumn.Should().NotBeNull();
        versionColumn!.Name.Should().Be(VersionColumnName);
        versionColumn.Type.Should().Be(Column.LongType);
    }

    /// <summary>
    /// Disposes connection to database
    /// </summary>
    public void Dispose()
    {
        dbConnection.Dispose();
    }
}
