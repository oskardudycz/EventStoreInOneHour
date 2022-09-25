using EventStoreInOneHour.Tools;

namespace EventStoreInOneHour.Tests.BankAccounts;

public static class BankAccountExtensions
{
    public static Task<BankAccount> GetBankAccount(
        this IEventStore eventStore,
        Guid streamId,
        long? atStreamVersion = null,
        DateTime? atTimestamp = null,
        CancellationToken ct = default
    ) =>
        eventStore.AggregateStreamAsync(
            ObjectFactory<BankAccount>.GetEmpty,
            BankAccount.Evolve,
            streamId,
            atStreamVersion,
            atTimestamp,
            ct
        );


    public static Task Handle(
        this IEventStore eventStore,
        Guid streamId,
        object command,
        long? expectedVersion = null,
        CancellationToken ct = default
    ) =>
        eventStore.Handle(
            ObjectFactory<BankAccount>.GetEmpty,
            BankAccount.Evolve,
            (_, account) => new[] { BankAccountDecider.Handle(command, account) },
            streamId,
            command,
            expectedVersion,
            ct
        );
}
