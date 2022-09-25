namespace EventStoreInOneHour.Tests.BankAccounts;

public record BankAccountOpened(
    Guid BankAccountId,
    string AccountNumber,
    Guid ClientId,
    string CurrencyISOCode,
    DateTime CreatedAt,
    long Version
);

public record DepositRecorded(
    Guid BankAccountId,
    decimal Amount,
    Guid CashierId,
    DateTime RecordedAt,
    long Version
);

public record CashWithdrawnFromATM(
    Guid BankAccountId,
    decimal Amount,
    Guid ATMId,
    DateTime RecordedAt,
    long Version
);

public record BankAccountClosed(Guid BankAccountId,
    string commandReason,
    DateTime ClosedAt,
    long Version);

public enum BankAccountStatus
{
    Opened,
    Closed
}

public record BankAccount(
    Guid Id,
    BankAccountStatus Status,
    string AccountNumber,
    Guid ClientId,
    string CurrencyISOCode,
    decimal Balance,
    DateTime CreatedAt,
    long Version = 0,
    DateTime? ClosedAt = null
)
{
    public static BankAccount Evolve(BankAccount bankAccount, object @event)
    {
        return @event switch
        {
            BankAccountOpened bankAccountCreated =>
                Create(bankAccountCreated),
            DepositRecorded depositRecorded =>
                bankAccount.Apply(depositRecorded),
            CashWithdrawnFromATM cashWithdrawnFromATM =>
                bankAccount.Apply(cashWithdrawnFromATM),
            BankAccountClosed bankAccountClosed =>
                bankAccount.Apply(bankAccountClosed),
            _ => bankAccount
        };
    }

    private static BankAccount Create(BankAccountOpened @event) =>
        new BankAccount(
            @event.BankAccountId,
            BankAccountStatus.Opened,
            @event.AccountNumber,
            @event.ClientId,
            @event.CurrencyISOCode,
            0,
            @event.CreatedAt,
            @event.Version
        );

    private BankAccount Apply(DepositRecorded @event) =>
        this with
        {
            Balance = Balance + @event.Amount,
            Version = @event.Version
        };

    private BankAccount Apply(CashWithdrawnFromATM @event) =>
        this with
        {
            Balance = Balance - @event.Amount,
            Version = @event.Version,
        };

    private BankAccount Apply(BankAccountClosed @event) =>
        this with
        {
            Status = BankAccountStatus.Closed,
            ClosedAt = @event.ClosedAt,
            Version = @event.Version
        };
}
