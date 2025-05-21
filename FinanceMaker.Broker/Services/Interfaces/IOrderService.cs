using FinanceMaker.Broker.Models;
using FinanceMaker.Broker.Models.Enums;

namespace FinanceMaker.Broker.Services.Interfaces;

public interface IOrderService
{
    Task<Order> CreateOrderAsync(
        Guid userId,
        string symbol,
        OrderType type,
        OrderSide side,
        decimal quantity,
        decimal? price,
        decimal? takeProfitPrice,
        decimal? stopLossPrice,
        CancellationToken cancellationToken);

    Task<IEnumerable<Order>> GetOrdersAsync(Guid userId, CancellationToken cancellationToken);
    Task<Order?> GetOrderAsync(Guid orderId, Guid userId, CancellationToken cancellationToken);
    Task<bool> CancelOrderAsync(Guid orderId, Guid userId, CancellationToken cancellationToken);
}
