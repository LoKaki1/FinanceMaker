namespace FinanceMaker.Broker.Models;

public class AccountSummary
{
    public decimal CashBalance { get; set; }
    public decimal BuyingPower { get; set; }
    public decimal TotalEquity { get; set; }
    public decimal OpenPnL { get; set; }
    public decimal ClosedPnL { get; set; }
    public decimal MarginUsed { get; set; }
    public decimal AvailableFunds { get; set; }
    public DateTime LastUpdated { get; set; }
}
