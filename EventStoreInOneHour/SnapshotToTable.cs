using Dapper;
using Npgsql;

namespace EventStoreInOneHour;

public class SnapshotToTable: IProjection
{
    private readonly NpgsqlConnection databaseConnection;
    private readonly string upsertSql;

    public SnapshotToTable(NpgsqlConnection databaseConnection, Type[] handles, string upsertSql)
    {
        this.databaseConnection = databaseConnection;
        Handles = handles;
        this.upsertSql = upsertSql;
    }

    public Type[] Handles { get; }
    public void Handle(object @event)
    {
        databaseConnection.Execute(upsertSql,  @event);
    }
}
