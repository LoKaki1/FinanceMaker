namespace FinanceMaker.Broker.Models;

public enum OrderType
{
    Market,
    Limit,
    Stop
}

public enum OrderSide
{
    Buy,
    Sell
}

public enum OrderStatus
{
    Pending,
    Filled,
    Cancelled,
    Rejected
}
