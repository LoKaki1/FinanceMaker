using FinanceMaker.Broker.Data;
using FinanceMaker.Broker.Models;
using FinanceMaker.Broker.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinanceMaker.Broker.Services;

public interface IAccountManagementService
{
    Task<Account?> GetAccountAsync(Guid accountId, CancellationToken cancellationToken);
    Task UpdateAccountBalanceAsync(Order order, decimal executionPrice, CancellationToken cancellationToken);
}

public class AccountManagementService : IAccountManagementService
{
    private readonly BrokerDbContext m_DbContext;
    private readonly ILogger<AccountManagementService> m_Logger;

    public AccountManagementService(
        BrokerDbContext dbContext,
        ILogger<AccountManagementService> logger)
    {
        m_DbContext = dbContext;
        m_Logger = logger;
    }

    public async Task<Account?> GetAccountAsync(Guid accountId, CancellationToken cancellationToken)
    {
        return await m_DbContext.Accounts
            .FirstOrDefaultAsync(a => a.Id == accountId, cancellationToken);
    }

    public async Task UpdateAccountBalanceAsync(Order order, decimal executionPrice, CancellationToken cancellationToken)
    {
        var account = await GetAccountAsync(order.AccountId, cancellationToken);
        if (account == null)
        {
            throw new InvalidOperationException($"Account {order.AccountId} not found");
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
        m_Logger.LogInformation("Updated account balance for account {AccountId}", order.AccountId);
    }
}
