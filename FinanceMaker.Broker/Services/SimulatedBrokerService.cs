using FinanceMaker.Broker.Data;
using FinanceMaker.Broker.Interfaces;
using FinanceMaker.Broker.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinanceMaker.Broker.Services;

public class SimulatedBrokerService : IBrokerService
{
    private readonly BrokerDbContext m_DbContext;
    private readonly Random m_Random;
    private readonly ILogger<SimulatedBrokerService> m_Logger;

    public SimulatedBrokerService(
        BrokerDbContext dbContext,
        ILogger<SimulatedBrokerService> logger)
    {
        m_DbContext = dbContext;
        m_Logger = logger;
        m_Random = new Random();
    }

    public async Task<OrderResult> PlaceOrderAsync(Order order, CancellationToken cancellationToken)
    {
        try
        {
            // Simulate market impact and slippage
            var marketData = await GetMarketDataAsync(order.Symbol, cancellationToken);
            var executionPrice = SimulateExecutionPrice(order, marketData);

            // Create order result
            var result = new OrderResult
            {
                Success = true,
                OrderId = Guid.NewGuid().ToString(),
                Status = OrderStatus.Filled,
                FilledPrice = executionPrice,
                FilledQuantity = order.Quantity,
                FilledTime = DateTime.UtcNow
            };

            // Update position
            await UpdatePositionAsync(order, executionPrice, cancellationToken);

            return result;
        }
        catch (Exception ex)
        {
            m_Logger.LogError(ex, "Error placing order for {Symbol}", order.Symbol);
            return new OrderResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                Status = OrderStatus.Rejected
            };
        }
    }

    public async Task<OrderResult> CancelOrderAsync(string orderId, CancellationToken cancellationToken)
    {
        try
        {
            var order = await m_DbContext.Orders.FindAsync(new object[] { orderId }, cancellationToken);
            if (order == null)
            {
                return new OrderResult
                {
                    Success = false,
                    ErrorMessage = "Order not found",
                    Status = OrderStatus.Cancelled
                };
            }

            order.Status = OrderStatus.Cancelled;
            await m_DbContext.SaveChangesAsync(cancellationToken);

            return new OrderResult
            {
                Success = true,
                OrderId = orderId,
                Status = OrderStatus.Cancelled
            };
        }
        catch (Exception ex)
        {
            m_Logger.LogError(ex, "Error cancelling order {OrderId}", orderId);
            return new OrderResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                Status = OrderStatus.Rejected
            };
        }
    }

    public async Task<Position> GetPositionAsync(string symbol, CancellationToken cancellationToken)
    {
        return await m_DbContext.Positions
            .FirstOrDefaultAsync(p => p.Symbol == symbol, cancellationToken)
            ?? new Position { Symbol = symbol, Quantity = 0 };
    }

    public async Task<IEnumerable<Position>> GetAllPositionsAsync(CancellationToken cancellationToken)
    {
        return await m_DbContext.Positions.ToListAsync(cancellationToken);
    }

    public async Task<AccountSummary> GetAccountSummaryAsync(CancellationToken cancellationToken)
    {
        var positions = await m_DbContext.Positions.ToListAsync(cancellationToken);
        var account = await m_DbContext.Accounts.FirstOrDefaultAsync(cancellationToken)
            ?? new Account { CashBalance = 100000 }; // Default starting balance

        var openPnL = positions.Sum(p => p.UnrealizedPnL);
        var totalEquity = account.CashBalance + positions.Sum(p => p.MarketValue);

        return new AccountSummary
        {
            CashBalance = account.CashBalance,
            BuyingPower = account.CashBalance * 2, // Simple 2x leverage
            TotalEquity = totalEquity,
            OpenPnL = openPnL,
            ClosedPnL = account.ClosedPnL,
            MarginUsed = positions.Sum(p => p.MarketValue),
            AvailableFunds = account.CashBalance,
            LastUpdated = DateTime.UtcNow
        };
    }

    public async Task<MarketData> GetMarketDataAsync(string symbol, CancellationToken cancellationToken)
    {
        // Simulate network latency
        await Task.Delay(50, cancellationToken);

        // Simulate market data with some randomness
        var basePrice = 100.0m; // Example base price
        var randomFactor = (decimal)(m_Random.NextDouble() * 0.02 - 0.01); // ±1% random movement
        var price = basePrice * (1 + randomFactor);

        return new MarketData
        {
            Symbol = symbol,
            LastPrice = price,
            Bid = price * 0.999m,
            Ask = price * 1.001m,
            BidSize = m_Random.Next(100, 1000),
            AskSize = m_Random.Next(100, 1000),
            Open = basePrice,
            High = price * 1.02m,
            Low = price * 0.98m,
            Close = price,
            Volume = m_Random.Next(10000, 100000),
            Timestamp = DateTime.UtcNow
        };
    }

    public async Task<IEnumerable<Order>> GetOpenOrdersAsync(CancellationToken cancellationToken)
    {
        return await m_DbContext.Orders
            .Where(o => o.Status == OrderStatus.Open)
            .ToListAsync(cancellationToken);
    }

    public async Task<Order> GetOrderAsync(string orderId, CancellationToken cancellationToken)
    {
        return await m_DbContext.Orders.FindAsync(new object[] { orderId }, cancellationToken)
            ?? throw new KeyNotFoundException($"Order {orderId} not found");
    }

    private decimal SimulateExecutionPrice(Order order, MarketData marketData)
    {
        // Simulate slippage based on order size and market conditions
        var slippageFactor = order.Quantity / 1000.0m; // More slippage for larger orders
        var randomSlippage = (decimal)(m_Random.NextDouble() * 0.002 - 0.001); // ±0.1% random slippage

        return order.Type switch
        {
            OrderType.Market => order.Side == OrderSide.Buy
                ? marketData.Ask * (1 + slippageFactor + randomSlippage)
                : marketData.Bid * (1 - slippageFactor + randomSlippage),
            OrderType.Limit => order.LimitPrice ?? marketData.LastPrice,
            _ => marketData.LastPrice
        };
    }

    private async Task UpdatePositionAsync(Order order, decimal executionPrice, CancellationToken cancellationToken)
    {
        var position = await m_DbContext.Positions
            .FirstOrDefaultAsync(p => p.Symbol == order.Symbol, cancellationToken);

        if (position == null)
        {
            position = new Position
            {
                Symbol = order.Symbol,
                Quantity = 0,
                AveragePrice = 0
            };
            m_DbContext.Positions.Add(position);
        }

        var quantityChange = order.Side == OrderSide.Buy ? order.Quantity : -order.Quantity;
        var newQuantity = position.Quantity + quantityChange;

        if (newQuantity == 0)
        {
            m_DbContext.Positions.Remove(position);
        }
        else
        {
            position.Quantity = newQuantity;
            position.AveragePrice = (position.AveragePrice * position.Quantity + executionPrice * quantityChange) / newQuantity;
            position.LastPrice = executionPrice;
        }

        await m_DbContext.SaveChangesAsync(cancellationToken);
    }
}
