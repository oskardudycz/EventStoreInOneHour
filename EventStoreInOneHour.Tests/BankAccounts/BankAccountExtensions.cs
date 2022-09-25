namespace EventStoreInOneHour.Tests.BankAccounts;

public static class BankAccountExtensions
{
    public static BankAccount? GetBankAccount(
        this IEventStore eventStore,
        Guid streamId,
        long? atStreamVersion = null,
        DateTime? atTimestamp = null
    ) => eventStore.AggregateStream<BankAccount>(
        BankAccount.Evolve,
        streamId,
        atStreamVersion,
        atTimestamp
    );
}
