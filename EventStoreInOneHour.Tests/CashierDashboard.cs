using System;

namespace EventStoreInOneHour.Tests
{
    public class CashierDashboard
    {
        public Guid Id { get; set; }
        public string CashierName { get; set; }
        public int RecordedDepositsCount { get; set; }
        public decimal TotalBalance { get; set; }
    }
}
