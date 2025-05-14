using System;
using FinanceMaker.Common.Models.Trades.Enums;

namespace FinanceMaker.Broker.Models;

public class Trade
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Username { get; set; } = null!;
    public string Symbol { get; set; } = null!;
    public OrderSide Side { get; set; }
    public decimal Price { get; set; }
    public decimal Quantity { get; set; }
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;
}
