namespace FinanceMaker.Broker.Models;

public class Transaction
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public TransactionType Type { get; set; }
    public DateTime Timestamp { get; set; }
}

public enum TransactionType
{
    Buy,
    Sell
}
