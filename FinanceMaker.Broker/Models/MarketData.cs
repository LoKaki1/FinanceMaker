namespace FinanceMaker.Broker.Models;

public class MarketData
{
    public string Symbol { get; set; } = string.Empty;
    public decimal LastPrice { get; set; }
    public decimal Bid { get; set; }
    public decimal Ask { get; set; }
    public int BidSize { get; set; }
    public int AskSize { get; set; }
    public decimal Open { get; set; }
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public decimal Close { get; set; }
    public long Volume { get; set; }
    public DateTime Timestamp { get; set; }
}
