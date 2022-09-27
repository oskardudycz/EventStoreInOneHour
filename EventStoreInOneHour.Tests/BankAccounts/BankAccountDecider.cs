namespace EventStoreInOneHour.Tests.BankAccounts;

public record OpenBankAccount(
    Guid BankAccountId,
    string AccountNumber,
    Guid ClientId,
    string CurrencyISOCode
);

public record RecordDeposit(
    decimal Amount,
    Guid CashierId
);

public record WithdrawnCashFromATM(
    decimal Amount,
    Guid AtmId
);

public record CloseBankAccount(
    string Reason
);

public static class BankAccountDecider
{
    public static object Handle(object command, BankAccount bankAccount) =>
        command switch
        {
            OpenBankAccount openBankAccount =>
                Handle(openBankAccount),
            RecordDeposit recordDeposit =>
                Handle(recordDeposit, bankAccount),
            WithdrawnCashFromATM withdrawnCash =>
                Handle(withdrawnCash, bankAccount),
            CloseBankAccount closeBankAccount =>
                Handle(closeBankAccount, bankAccount),
            _ =>
                throw new InvalidOperationException($"{command.GetType().Name} cannot be handled for Bank Account")
        };

    public static BankAccountOpened Handle(
        OpenBankAccount command
    ) =>
        new BankAccountOpened(
            command.BankAccountId,
            command.AccountNumber,
            command.ClientId,
            command.CurrencyISOCode,
            DateTime.UtcNow,
            1
        );

    public static DepositRecorded Handle(
        RecordDeposit command,
        BankAccount account
    )
    {
        if (account.Status == BankAccountStatus.Closed)
            throw new InvalidOperationException("Account is closed!");

        return new DepositRecorded(account.Id, command.Amount, command.CashierId, DateTime.UtcNow, account.Version + 1);
    }

    public static CashWithdrawnFromATM Handle(
        WithdrawnCashFromATM command,
        BankAccount account
    )
    {
        if (account.Status == BankAccountStatus.Closed)
            throw new InvalidOperationException("Account is closed!");

        if (account.Balance < command.Amount)
            throw new InvalidOperationException("Not enough money!");

        return new CashWithdrawnFromATM(account.Id, command.Amount, command.AtmId, DateTime.UtcNow, account.Version + 1);
    }

    public static  BankAccountClosed Handle(
        CloseBankAccount command,
        BankAccount account
    )
    {
        if (account.Status == BankAccountStatus.Closed)
            throw new InvalidOperationException("Account is already closed!");

        return new BankAccountClosed(account.Id, command.Reason, DateTime.UtcNow, account.Version + 1);
    }
}
