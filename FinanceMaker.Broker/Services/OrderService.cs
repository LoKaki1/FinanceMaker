using FinanceMaker.Broker.Data;
using FinanceMaker.Broker.Models;
using FinanceMaker.Broker.Models.Enums;
using FinanceMaker.Broker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinanceMaker.Broker.Services;

public class OrderService : IOrderService
{
    private readonly BrokerDbContext m_DbContext;

    public OrderService(BrokerDbContext dbContext)
    {
        m_DbContext = dbContext;
    }

    public async Task<Order> CreateOrderAsync(
        Guid userId,
        string symbol,
        OrderType type,
        OrderSide side,
        decimal quantity,
        decimal? price,
        decimal? takeProfitPrice,
        decimal? stopLossPrice,
        CancellationToken cancellationToken)
    {
        var account = await m_DbContext.Accounts
            .FirstOrDefaultAsync(a => a.UserId == userId, cancellationToken);

        if (account == null)
        {
            throw new InvalidOperationException("Account not found");
        }

        ValidateOrder(type, price, stopPrice: null, takeProfitPrice, stopLossPrice);

        var order = new Order
        {
            AccountId = account.Id,
            Symbol = symbol,
            Type = type,
            Side = side,
            Quantity = quantity,
            Price = price,
            StopPrice = null, // Will be set for Stop orders
            TakeProfitPrice = takeProfitPrice,
            StopLossPrice = stopLossPrice,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        m_DbContext.Orders.Add(order);
        await m_DbContext.SaveChangesAsync(cancellationToken);

        return order;
    }

    public async Task<IEnumerable<Order>> GetOrdersAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await m_DbContext.Orders
            .Include(o => o.Account)
            .Where(o => o.Account.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Order?> GetOrderAsync(Guid orderId, Guid userId, CancellationToken cancellationToken)
    {
        return await m_DbContext.Orders
            .Include(o => o.Account)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.Account.UserId == userId, cancellationToken);
    }

    public async Task<bool> CancelOrderAsync(Guid orderId, Guid userId, CancellationToken cancellationToken)
    {
        var order = await m_DbContext.Orders
            .Include(o => o.Account)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.Account.UserId == userId, cancellationToken);

        if (order == null)
        {
            return false;
        }

        if (order.Status != OrderStatus.Pending)
        {
            throw new InvalidOperationException("Only pending orders can be cancelled");
        }

        order.Status = OrderStatus.Cancelled;
        order.CancelledAt = DateTime.UtcNow;

        await m_DbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private void ValidateOrder(
        OrderType type,
        decimal? price,
        decimal? stopPrice,
        decimal? takeProfitPrice,
        decimal? stopLossPrice)
    {
        switch (type)
        {
            case OrderType.Market:
                if (price.HasValue)
                {
                    throw new ArgumentException("Market orders cannot have a price");
                }
                break;

            case OrderType.Limit:
                if (!price.HasValue)
                {
                    throw new ArgumentException("Limit orders must have a price");
                }
                break;

            case OrderType.Stop:
                if (!stopPrice.HasValue)
                {
                    throw new ArgumentException("Stop orders must have a stop price");
                }
                break;

            case OrderType.StopLimit:
                if (!price.HasValue || !stopPrice.HasValue)
                {
                    throw new ArgumentException("StopLimit orders must have both price and stop price");
                }
                break;
        }

        // Validate take profit and stop loss prices
        if (takeProfitPrice.HasValue && price.HasValue && takeProfitPrice <= price)
        {
            throw new ArgumentException("Take profit price must be higher than the order price");
        }

        if (stopLossPrice.HasValue && price.HasValue && stopLossPrice >= price)
        {
            throw new ArgumentException("Stop loss price must be lower than the order price");
        }
    }
}
