namespace EventStoreInOneHour.Tests;

public class BankAccountCreated
{
    public Guid BankAccountId { get; }
    public string AccountNumber { get; }
    public Guid ClientId { get; }
    public DateTime CreatedAt { get; }
    public string CurrencyISOCode { get; }

    public BankAccountCreated(
        Guid bankAccountId,
        string accountNumber,
        Guid clientId,
        string currencyISOCode,
        DateTime createdAt
    )
    {
        BankAccountId = bankAccountId;
        AccountNumber = accountNumber;
        ClientId = clientId;
        CurrencyISOCode = currencyISOCode;
        CreatedAt = createdAt;
    }
}


public class DepositRecorded
{
    public Guid BankAccountId { get; }
    public decimal Amount { get; }
    public Guid CashierId { get; }
    public DateTime RecordedAt { get; }

    public DepositRecorded(Guid bankAccountId, decimal amount, Guid cashierId, DateTime recordedAt)
    {
        BankAccountId = bankAccountId;
        Amount = amount;
        CashierId = cashierId;
        RecordedAt = recordedAt;
    }
}

public class CashWithdrawnFromATM
{
    public Guid BankAccountId { get; }
    public decimal Amount { get; }
    public Guid ATMId { get; }
    public DateTime RecordedAt { get; }

    public CashWithdrawnFromATM(Guid bankAccountId, decimal amount, Guid atmId, DateTime recordedAt)
    {
        BankAccountId = bankAccountId;
        Amount = amount;
        ATMId = atmId;
        RecordedAt = recordedAt;
    }
}

public class BankAccount : Aggregate
{
    public string AccountNumber { get; private set; } = default!;
    public Guid ClientId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string CurrencyISOCode { get; private set; } = default!;
    public decimal Balance { get; private set; }

    // for dapper
    public BankAccount()
    {
    }

    private BankAccount(
        Guid bankAccountId,
        string accountNumber,
        Guid clientId,
        string currencyISOCode,
        DateTime createdAt
    )
    {
        var @event = new BankAccountCreated(
            bankAccountId,
            accountNumber,
            clientId,
            currencyISOCode,
            createdAt
        );

        Enqueue(@event);
        Apply(@event);
    }

    public static BankAccount Open(
        Guid bankAccountId,
        string accountNumber,
        Guid clientId,
        string currencyISOCode
    )
    {
        return new BankAccount(bankAccountId, accountNumber, clientId, currencyISOCode, DateTime.UtcNow);
    }

    public void RecordDeposit(
        decimal amount,
        Guid cashierId
    )
    {
        var @event = new DepositRecorded(Id, amount, cashierId, DateTime.UtcNow);

        Enqueue(@event);
        Apply(@event);
    }

    public void WithdrawnCash(
        decimal amount,
        Guid atmId
    )
    {
        var @event = new CashWithdrawnFromATM(Id, amount, atmId, DateTime.UtcNow);

        Enqueue(@event);
        Apply(@event);
    }

    public void Apply(BankAccountCreated @event)
    {
        Id = @event.BankAccountId;
        AccountNumber = @event.AccountNumber;
        ClientId = @event.ClientId;
        CurrencyISOCode = @event.CurrencyISOCode;
        CreatedAt = @event.CreatedAt;
        Balance = 0;
    }

    public void Apply(DepositRecorded @event)
    {
        Balance += @event.Amount;
    }

    public void Apply(CashWithdrawnFromATM @event)
    {
        Balance -= @event.Amount;
    }
}
