using System;
using System.Collections.Generic;

namespace EventStoreInOneHour
{
    public interface IAggregate
    {
        Guid Id { get; }
        int Version { get; }
        IEnumerable<object> DequeueUncommittedEvents();
    }
}
