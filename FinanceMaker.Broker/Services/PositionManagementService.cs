using FinanceMaker.Broker.Data;
using FinanceMaker.Broker.Models;
using FinanceMaker.Broker.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinanceMaker.Broker.Services;

public interface IPositionManagementService
{
    Task UpdatePositionAsync(Order order, decimal executionPrice, CancellationToken cancellationToken);
}

public class PositionManagementService : IPositionManagementService
{
    private readonly BrokerDbContext m_DbContext;
    private readonly ILogger<PositionManagementService> m_Logger;

    public PositionManagementService(
        BrokerDbContext dbContext,
        ILogger<PositionManagementService> logger)
    {
        m_DbContext = dbContext;
        m_Logger = logger;
    }

    public async Task UpdatePositionAsync(Order order, decimal executionPrice, CancellationToken cancellationToken)
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

        if (order.Side == OrderSide.Sell)
        {
            var realizedPnL = (executionPrice - oldAveragePrice) * (int)order.Quantity;
            position.RealizedPnL += realizedPnL;
        }

        await m_DbContext.SaveChangesAsync(cancellationToken);
        m_Logger.LogInformation("Updated position for {Symbol} in account {AccountId}", order.Symbol, order.AccountId);
    }
}
