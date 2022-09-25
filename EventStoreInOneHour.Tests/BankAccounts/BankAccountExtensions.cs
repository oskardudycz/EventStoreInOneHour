namespace EventStoreInOneHour.Tests.BankAccounts;

public static class BankAccountExtensions
{
    public static Task<BankAccount?> GetBankAccount(
        this IEventStore eventStore,
        Guid streamId,
        long? atStreamVersion = null,
        DateTime? atTimestamp = null,
        CancellationToken ct = default
    ) =>
        eventStore.AggregateStreamAsync<BankAccount>(
            BankAccount.Evolve,
            streamId,
            atStreamVersion,
            atTimestamp,
            ct
        );
}
