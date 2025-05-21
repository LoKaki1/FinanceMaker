using FinanceMaker.Broker.Models.Enums;

namespace FinanceMaker.Broker.Models.Requests;

public class OrderRequest
{
    public string Symbol { get; set; } = string.Empty;
    public OrderType Type { get; set; }
    public OrderSide Side { get; set; }
    public decimal Quantity { get; set; }
    public decimal? Price { get; set; }  // Required for LIMIT orders
    public decimal? TakeProfitPrice { get; set; }  // Optional
    public decimal? StopLossPrice { get; set; }  // Optional
}
