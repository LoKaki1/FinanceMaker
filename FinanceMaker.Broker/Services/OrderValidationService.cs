using FinanceMaker.Broker.Data;
using FinanceMaker.Broker.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinanceMaker.Broker.Services;

public interface IOrderValidationService
{
    Task<bool> ValidateOrderAsync(Order order, CancellationToken cancellationToken);
}

public class OrderValidationService : IOrderValidationService
{
    private readonly BrokerDbContext m_DbContext;
    private readonly ILogger<OrderValidationService> m_Logger;

    public OrderValidationService(
        BrokerDbContext dbContext,
        ILogger<OrderValidationService> logger)
    {
        m_DbContext = dbContext;
        m_Logger = logger;
    }

    public async Task<bool> ValidateOrderAsync(Order order, CancellationToken cancellationToken)
    {
        if (order.Quantity <= 0 || string.IsNullOrWhiteSpace(order.Symbol))
        {
            m_Logger.LogWarning("Invalid order: Quantity or Symbol is invalid");
            return false;
        }

        var accountExists = await m_DbContext.Accounts
            .AnyAsync(a => a.Id == order.AccountId, cancellationToken);

        if (!accountExists)
        {
            m_Logger.LogWarning("Invalid order: Account {AccountId} not found", order.AccountId);
            return false;
        }

        return true;
    }
}
