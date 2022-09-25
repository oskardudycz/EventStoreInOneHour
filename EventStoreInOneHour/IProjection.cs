using Dapper;
using Dapper.Contrib.Extensions;
using EventStoreInOneHour.Tools;
using Npgsql;

namespace EventStoreInOneHour;

public interface IProjection
{
    void Init();
    Type[] Handles { get; }
    Task Handle(object @event, CancellationToken ct);
}

public abstract class Projection: IProjection
{
    private readonly Dictionary<Type, Func<object, CancellationToken, Task>> handlers = new();

    public virtual void Init() { }

    public Type[] Handles => handlers.Keys.ToArray();

    protected void Projects<TEvent>(Func<TEvent, CancellationToken, Task> action) =>
        handlers.Add(
            typeof(TEvent),
            (@event, ct) => action((TEvent)@event, ct)
        );

    public Task Handle(object @event, CancellationToken ct) =>
        handlers[@event.GetType()](@event, ct);
}

public abstract class FlatTableProjection<TEntity>: Projection where TEntity : class
{
    private readonly NpgsqlConnection dbConnection;
    protected abstract string CreateTableStatement { get; }

    protected FlatTableProjection(
        NpgsqlConnection dbConnection
    )
    {
        this.dbConnection = dbConnection;
    }

    public override void Init() =>
        dbConnection.Execute(CreateTableStatement);

    protected void Projects<TEvent>(
        Func<TEvent, Guid> getId,
        Func<TEntity, TEvent, TEntity> handle
    )
    {
        Projects<TEvent>(async (@event, _) =>
        {
            var entity = await dbConnection.GetAsync<TEntity?>(getId(@event));
            var updatedEntity = handle(entity ?? ObjectFactory<TEntity>.GetEmpty(), @event);

            if (entity == null)
                await dbConnection.InsertAsync(updatedEntity);
            else
                await dbConnection.UpdateAsync(updatedEntity);
        });
    }
}
