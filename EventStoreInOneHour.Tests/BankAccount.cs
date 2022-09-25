namespace EventStoreInOneHour.Tests;

public record BankAccountCreated(
    Guid BankAccountId,
    string AccountNumber,
    Guid ClientId,
    string CurrencyISOCode,
    DateTime CreatedAt
);

public record DepositRecorded(
    Guid BankAccountId,
    decimal Amount,
    Guid CashierId,
    DateTime RecordedAt
);

public record CashWithdrawnFromATM(
    Guid BankAccountId,
    decimal Amount,
    Guid ATMId,
    DateTime RecordedAt
);

public class BankAccount: Aggregate
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

    public static BankAccount Evolve(BankAccount bankAccount, object @event)
    {
        switch (@event)
        {
            case BankAccountCreated bankAccountCreated:
                bankAccount.Apply(bankAccountCreated);
                break;
            case DepositRecorded depositRecorded:
                bankAccount.Apply(depositRecorded);
                break;
            case CashWithdrawnFromATM cashWithdrawnFromATM:
                bankAccount.Apply(cashWithdrawnFromATM);
                break;
        }

        return bankAccount;
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
