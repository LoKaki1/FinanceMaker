using System;

namespace FinanceMaker.Broker.Models;

public class Position
{
    public string Username { get; set; } = null!;
    public string Symbol { get; set; } = null!;
    public decimal Quantity { get; set; }
    public decimal AveragePrice { get; set; }
    public decimal CurrentPrice { get; set; }
    public decimal UnrealizedPnL { get; set; }
    public decimal RealizedPnL { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
