namespace FinanceMaker.Broker.Models.Requests;
using FinanceMaker.Broker.Models.Enums;

public class PlaceOrderRequest
{
    public string Symbol { get; set; } = string.Empty;
    public OrderType Type { get; set; }
    public OrderSide Side { get; set; }
    public int Quantity { get; set; }
    public decimal? LimitPrice { get; set; }
    public decimal? StopPrice { get; set; }
    public decimal? TakeProfitPrice { get; set; }
    public decimal? StopLossPrice { get; set; }
}
