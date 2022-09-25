using Dapper.Contrib.Extensions;
using Npgsql;

namespace EventStoreInOneHour.Tests.BankAccounts;

[Table("BankAccountDetails")]
public class BankAccountDetails
{
    [ExplicitKey]
    public Guid id { get; set; }
    public string Status { get; set; } = default!;
    public string AccountNumber { get; set; } = default!;
    public Guid ClientId { get; set; }
    public string CurrencyISOCode { get; set; } = default!;
    public decimal Balance { get; set; }
    public DateTime CreatedAt { get; set; }
    public long Version { get; set; }
    public DateTime? ClosedAt { get; set; }
    public string? ClosureReason { get; set; }

    public static BankAccountDetails Create(BankAccountOpened @event) =>
        new BankAccountDetails()
        {
            id = @event.BankAccountId,
            Status = BankAccountStatus.Opened.ToString(),
            AccountNumber = @event.AccountNumber,
            ClientId = @event.ClientId,
            CurrencyISOCode = @event.CurrencyISOCode,
            Balance = 0,
            CreatedAt = @event.CreatedAt,
            Version = @event.Version
        };

    public BankAccountDetails Apply(DepositRecorded @event)
    {
        Balance += @event.Amount;
        Version = @event.Version;
        return this;
    }

    public BankAccountDetails Apply(CashWithdrawnFromATM @event)
    {
        Balance -= @event.Amount;
        Version = @event.Version;
        return this;
    }

    public BankAccountDetails Apply(BankAccountClosed @event)
    {
        Status = BankAccountStatus.Closed.ToString();
        ClosedAt = @event.ClosedAt;
        Version = @event.Version;
        return this;
    }
}

public class BankAccountDetailsProjection: FlatTableProjection<BankAccountDetails>
{
    protected override string CreateTableStatement =>
        @"CREATE TABLE IF NOT EXISTS BankAccountDetails (
          ""id""                UUID        NOT NULL    PRIMARY KEY,
          ""Status""            TEXT        NOT NULL,
          ""AccountNumber""     TEXT        NOT NULL,
          ""ClientId""          UUID        NOT NULL,
          ""CurrencyISOCode""   TEXT        NOT NULL,
          ""Balance""           decimal     NOT NULL,
          ""CreatedAt""         timestamp   NOT NULL,
          ""Version""           BIGINT      NOT NULL,
          ""ClosedAt""          timestamp   NULL,
          ""ClosureReason""     TEXT        NULL
      );";

    public BankAccountDetailsProjection(NpgsqlConnection connection): base(connection)
    {
        Projects<BankAccountOpened>(e => e.BankAccountId, (_, @event) => BankAccountDetails.Create(@event));
        Projects<DepositRecorded>(e => e.BankAccountId, (view, @event) => view.Apply(@event));
        Projects<CashWithdrawnFromATM>(e => e.BankAccountId, (view, @event) => view.Apply(@event));
        Projects<BankAccountClosed>(e => e.BankAccountId, (view, @event) => view.Apply(@event));
    }
}
