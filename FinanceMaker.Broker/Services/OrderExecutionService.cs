using FinanceMaker.Broker.Models;
using FinanceMaker.Broker.Models.Enums;
using FinanceMaker.Broker.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace FinanceMaker.Broker.Services;

public interface IOrderExecutionService
{
    Task<Order> ExecuteOrderAsync(Order order, CancellationToken cancellationToken);
}

public class OrderExecutionService : IOrderExecutionService
{
    private readonly IMarketDataService m_MarketDataService;
    private readonly IOrderValidationService m_OrderValidationService;
    private readonly IAccountManagementService m_AccountManagementService;
    private readonly IPositionManagementService m_PositionManagementService;
    private readonly ILogger<OrderExecutionService> m_Logger;

    public OrderExecutionService(
        IMarketDataService marketDataService,
        IOrderValidationService orderValidationService,
        IAccountManagementService accountManagementService,
        IPositionManagementService positionManagementService,
        ILogger<OrderExecutionService> logger)
    {
        m_MarketDataService = marketDataService;
        m_OrderValidationService = orderValidationService;
        m_AccountManagementService = accountManagementService;
        m_PositionManagementService = positionManagementService;
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

            await UpdateOrderStatusAsync(order, currentPrice);
            await m_AccountManagementService.UpdateAccountBalanceAsync(order, currentPrice, cancellationToken);
            await m_PositionManagementService.UpdatePositionAsync(order, currentPrice, cancellationToken);

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

    private async Task UpdateOrderStatusAsync(Order order, decimal executionPrice)
    {
        order.Status = OrderStatus.Filled;
        order.FilledPrice = executionPrice;
        order.FilledQuantity = (int)order.Quantity;
        order.FilledAt = DateTime.UtcNow;
    }
}
