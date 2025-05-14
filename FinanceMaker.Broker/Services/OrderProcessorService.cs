using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FinanceMaker.Broker.Interfaces;
using FinanceMaker.Broker.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinanceMaker.Broker.Services;

public class OrderProcessorService
{
    private readonly IBrokerDbContext m_DbContext;
    private readonly IMarketDataService m_MarketDataService;
    private readonly ILogger<OrderProcessorService> m_Logger;
    private const decimal PriceEpsilon = 0.01m;

    public OrderProcessorService(
        IBrokerDbContext dbContext,
        IMarketDataService marketDataService,
        ILogger<OrderProcessorService> logger)
    {
        m_DbContext = dbContext;
        m_MarketDataService = marketDataService;
        m_Logger = logger;
    }

    public async Task ProcessOrdersAsync(CancellationToken cancellationToken = default)
    {
        var openOrders = await m_DbContext.Orders
            .Where(o => o.Status == OrderStatus.Pending)
            .ToListAsync(cancellationToken);

        foreach (var order in openOrders)
        {
            try
            {
                var currentPrice = await m_MarketDataService.GetLastPriceAsync(order.Symbol, cancellationToken);
                await ProcessOrderAsync(order, currentPrice, cancellationToken);
            }
            catch (Exception ex)
            {
                m_Logger.LogError(ex, "Error processing order {OrderId}", order.Id);
            }
        }
    }

    private async Task ProcessOrderAsync(Order order, decimal currentPrice, CancellationToken cancellationToken)
    {
        if (order.Status != OrderStatus.Pending)
        {
            return;
        }

        var shouldExecute = order.Type switch
        {
            OrderType.Market => true,
            OrderType.Limit => order.Side == OrderSide.Buy
                ? currentPrice <= order.Price + PriceEpsilon
                : currentPrice >= order.Price - PriceEpsilon,
            OrderType.Stop => order.Side == OrderSide.Buy
                ? currentPrice >= order.Price - PriceEpsilon
                : currentPrice <= order.Price + PriceEpsilon,
            _ => false
        };

        if (!shouldExecute)
        {
            return;
        }

        // Execute the order
        order.Status = OrderStatus.Filled;
        order.FilledAt = DateTime.UtcNow;
        order.FilledPrice = currentPrice;
        order.FilledQuantity = order.Quantity;

        // Create trade record
        var trade = new Trade
        {
            Username = order.Username,
            Symbol = order.Symbol,
            Side = order.Side,
            Price = currentPrice,
            Quantity = order.Quantity,
            OrderId = order.Id
        };

        await m_DbContext.Trades.AddAsync(trade, cancellationToken);

        // Update user's position
        var position = await m_DbContext.Positions
            .FirstOrDefaultAsync(p => p.Username == order.Username && p.Symbol == order.Symbol, cancellationToken);

        if (position is null)
        {
            position = new Position
            {
                Username = order.Username,
                Symbol = order.Symbol,
                Quantity = 0,
                AveragePrice = 0,
                CurrentPrice = currentPrice,
                UnrealizedPnL = 0,
                RealizedPnL = 0
            };

            await m_DbContext.Positions.AddAsync(position, cancellationToken);
        }

        // Update position
        if (order.Side == OrderSide.Buy)
        {
            var totalCost = position.Quantity * position.AveragePrice + order.Quantity * currentPrice;
            var totalQuantity = position.Quantity + order.Quantity;

            position.AveragePrice = totalCost / totalQuantity;
            position.Quantity = totalQuantity;
        }
        else
        {
            var realizedPnL = (currentPrice - position.AveragePrice) * order.Quantity;

            position.RealizedPnL += realizedPnL;
            position.Quantity -= order.Quantity;
        }

        position.CurrentPrice = currentPrice;
        position.UnrealizedPnL = (currentPrice - position.AveragePrice) * position.Quantity;
        position.LastUpdated = DateTime.UtcNow;

        // Update user's balance
        var user = await m_DbContext.Users.FindAsync(new object[] { order.Username }, cancellationToken);

        if (user is not null)
        {
            var cost = order.Side == OrderSide.Buy ? currentPrice * order.Quantity : -(currentPrice * order.Quantity);
            user.Balance += cost;
        }

        await m_DbContext.SaveChangesAsync(cancellationToken);

        // Process child orders if any
        if (order.ChildOrders.Any())
        {
            foreach (var childOrder in order.ChildOrders.Where(o => o.Status == OrderStatus.Pending))
            {
                childOrder.Status = OrderStatus.Cancelled;
            }

            await m_DbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
