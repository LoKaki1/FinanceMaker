using FinanceMaker.Broker.Data;
using FinanceMaker.Broker.Interfaces;
using FinanceMaker.Broker.Models;
using FinanceMaker.Broker.Models.Enums;
using FinanceMaker.Broker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinanceMaker.Broker.Services;

public class OrderExecutionService : IOrderExecutionService
{
    private readonly BrokerDbContext m_DbContext;
    private readonly IMarketDataService m_MarketDataService;
    private readonly ILogger<OrderExecutionService> m_Logger;

    public OrderExecutionService(
        BrokerDbContext dbContext,
        IMarketDataService marketDataService,
        ILogger<OrderExecutionService> logger)
    {
        m_DbContext = dbContext;
        m_MarketDataService = marketDataService;
        m_Logger = logger;
    }

    public async Task<Order> ExecuteOrderAsync(Order order, CancellationToken cancellationToken)
    {
        try
        {
            var currentPrice = await m_MarketDataService.GetCurrentPriceAsync(order.Symbol, cancellationToken);

            if (!CanExecuteOrder(order, currentPrice))
            {
                return order;
            }

            using var transaction = await m_DbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                await UpdateOrderStatusAsync(order, currentPrice, cancellationToken);
                await UpdatePositionAsync(order, currentPrice, cancellationToken);
                await UpdateAccountBalanceAsync(order, currentPrice, cancellationToken);

                await transaction.CommitAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }

            return order;
        }
        catch (Exception ex)
        {
            m_Logger.LogError(ex, "Error executing order {OrderId}", order.Id);
            throw;
        }
    }

    private bool CanExecuteOrder(Order order, decimal currentPrice)
    {
        return order.Type switch
        {
            OrderType.Market => true,
            OrderType.Limit => order.Side == OrderSide.Buy
                ? currentPrice <= order.Price!.Value
                : currentPrice >= order.Price!.Value,
            OrderType.Stop => order.Side == OrderSide.Buy
                ? currentPrice >= order.StopPrice!.Value
                : currentPrice <= order.StopPrice!.Value,
            OrderType.StopLimit => order.Side == OrderSide.Buy
                ? currentPrice >= order.StopPrice!.Value && currentPrice <= order.Price!.Value
                : currentPrice <= order.StopPrice!.Value && currentPrice >= order.Price!.Value,
            _ => false
        };
    }

    private async Task UpdateOrderStatusAsync(Order order, decimal executionPrice, CancellationToken cancellationToken)
    {
        order.Status = OrderStatus.Filled;
        order.FilledPrice = executionPrice;
        order.FilledQuantity = (int)order.Quantity;
        order.FilledAt = DateTime.UtcNow;

        await m_DbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task UpdatePositionAsync(Order order, decimal executionPrice, CancellationToken cancellationToken)
    {
        var position = await m_DbContext.Positions
            .FirstOrDefaultAsync(p => p.AccountId == order.AccountId && p.Symbol == order.Symbol, cancellationToken);

        if (position == null)
        {
            position = new Position
            {
                AccountId = order.AccountId,
                Symbol = order.Symbol,
                Quantity = 0,
                AveragePrice = 0,
                LastPrice = executionPrice,
                RealizedPnL = 0
            };
            m_DbContext.Positions.Add(position);
        }

        var oldQuantity = position.Quantity;
        var oldAveragePrice = position.AveragePrice;

        if (order.Side == OrderSide.Buy)
        {
            position.Quantity += (int)order.Quantity;
            position.AveragePrice = ((oldQuantity * oldAveragePrice) + ((int)order.Quantity * executionPrice)) / position.Quantity;
        }
        else
        {
            position.Quantity -= (int)order.Quantity;
            if (position.Quantity == 0)
            {
                position.AveragePrice = 0;
            }
        }

        position.LastPrice = executionPrice;
        position.LastUpdated = DateTime.UtcNow;

        // Calculate PnL
        if (order.Side == OrderSide.Sell)
        {
            var realizedPnL = (executionPrice - oldAveragePrice) * (int)order.Quantity;
            position.RealizedPnL += realizedPnL;
        }

        await m_DbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task UpdateAccountBalanceAsync(Order order, decimal executionPrice, CancellationToken cancellationToken)
    {
        var account = await m_DbContext.Accounts
            .FirstOrDefaultAsync(a => a.Id == order.AccountId, cancellationToken);

        if (account == null)
        {
            throw new InvalidOperationException("Account not found");
        }

        var orderValue = order.Quantity * executionPrice;

        if (order.Side == OrderSide.Buy)
        {
            account.CashBalance -= orderValue;
            account.MarginUsed += orderValue;
        }
        else
        {
            account.CashBalance += orderValue;
            account.MarginUsed -= orderValue;
        }

        account.AvailableFunds = account.CashBalance - account.MarginUsed;
        account.LastUpdated = DateTime.UtcNow;

        await m_DbContext.SaveChangesAsync(cancellationToken);
    }
}
