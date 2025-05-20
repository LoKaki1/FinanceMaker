using FinanceMaker.Broker.Models;

namespace FinanceMaker.Broker.Interfaces;

public interface IBrokerService
{
    Task<OrderResult> PlaceOrderAsync(Order order, CancellationToken cancellationToken);
    Task<OrderResult> CancelOrderAsync(string orderId, CancellationToken cancellationToken);
    Task<Position> GetPositionAsync(string symbol, CancellationToken cancellationToken);
    Task<IEnumerable<Position>> GetAllPositionsAsync(CancellationToken cancellationToken);
    Task<AccountSummary> GetAccountSummaryAsync(CancellationToken cancellationToken);
    Task<MarketData> GetMarketDataAsync(string symbol, CancellationToken cancellationToken);
    Task<IEnumerable<Order>> GetOpenOrdersAsync(CancellationToken cancellationToken);
    Task<Order> GetOrderAsync(string orderId, CancellationToken cancellationToken);
}
