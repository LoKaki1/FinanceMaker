using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinanceMaker.Broker.Data;
using FinanceMaker.Broker.Models;
using FinanceMaker.Broker.Models.Enums;
using FinanceMaker.Broker.Models.Responses;
using FinanceMaker.Broker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinanceMaker.Broker.Services;

public interface IOrderProcessorService
{
    Task<OrderResponse> ProcessOrderAsync(Order order, CancellationToken cancellationToken);
}

public class OrderProcessorService : IOrderProcessorService
{
    private readonly IOrderValidationService m_OrderValidationService;
    private readonly IPriceManagementService m_PriceManagementService;
    private readonly IAccountManagementService m_AccountManagementService;
    private readonly IPositionManagementService m_PositionManagementService;
    private readonly ILogger<OrderProcessorService> m_Logger;
    private readonly Dictionary<OrderType, Func<Order, decimal, Task>> m_OrderProcessors;

    public OrderProcessorService(
        IOrderValidationService orderValidationService,
        IPriceManagementService priceManagementService,
        IAccountManagementService accountManagementService,
        IPositionManagementService positionManagementService,
        ILogger<OrderProcessorService> logger)
    {
        m_OrderValidationService = orderValidationService;
        m_PriceManagementService = priceManagementService;
        m_AccountManagementService = accountManagementService;
        m_PositionManagementService = positionManagementService;
        m_Logger = logger;
        m_OrderProcessors = new Dictionary<OrderType, Func<Order, decimal, Task>>
        {
            { OrderType.Market, ProcessMarketOrder },
            { OrderType.Limit, ProcessLimitOrder },
            { OrderType.Stop, ProcessStopOrder }
        };
    }

    public async Task<OrderResponse> ProcessOrderAsync(Order order, CancellationToken cancellationToken)
    {
        try
        {
            if (!await m_OrderValidationService.ValidateOrderAsync(order, cancellationToken))
            {
                return OrderResponse.CreateError("Order validation failed");
            }

            decimal currentPrice = m_PriceManagementService.GetCurrentPrice(order.Symbol);
            if (currentPrice == 0)
            {
                return OrderResponse.CreateError("No current price available for symbol");
            }

            var account = await m_AccountManagementService.GetAccountAsync(order.AccountId, cancellationToken);
            if (account is null)
            {
                return OrderResponse.CreateError("Account not found");
            }

            if (order.Side == OrderSide.Buy && account.AvailableFunds < order.Quantity * currentPrice)
            {
                return OrderResponse.CreateError("Insufficient funds");
            }

            if (!m_OrderProcessors.TryGetValue(order.Type, out var processor))
            {
                return OrderResponse.CreateError($"Unsupported order type: {order.Type}");
            }

            await processor(order, currentPrice);
            await m_AccountManagementService.UpdateAccountBalanceAsync(order, currentPrice, cancellationToken);
            await m_PositionManagementService.UpdatePositionAsync(order, currentPrice, cancellationToken);

            LogOrderSuccess(order, currentPrice);
            return OrderResponse.CreateSuccess(order);
        }
        catch (Exception ex)
        {
            await HandleOrderError(order, ex);
            return OrderResponse.CreateError("Internal server error");
        }
    }

    private async Task ProcessMarketOrder(Order order, decimal currentPrice)
    {
        order.Status = OrderStatus.Filled;
        order.FilledPrice = currentPrice;
        order.FilledQuantity = (int)order.Quantity;
        order.FilledAt = DateTime.UtcNow;
    }

    private async Task ProcessLimitOrder(Order order, decimal currentPrice)
    {
        if (order.Price is null)
        {
            throw new ArgumentException("Limit price is required for limit orders");
        }

        if (IsLimitOrderExecutable(order, currentPrice))
        {
            await ProcessMarketOrder(order, currentPrice);
        }
    }

    private bool IsLimitOrderExecutable(Order order, decimal currentPrice)
    {
        return (order.Side == OrderSide.Buy && currentPrice <= order.Price) ||
               (order.Side == OrderSide.Sell && currentPrice >= order.Price);
    }

    private async Task ProcessStopOrder(Order order, decimal currentPrice)
    {
        if (order.StopPrice is null)
        {
            throw new ArgumentException("Stop price is required for stop orders");
        }

        if (IsStopOrderExecutable(order, currentPrice))
        {
            await ProcessMarketOrder(order, currentPrice);
        }
    }

    private bool IsStopOrderExecutable(Order order, decimal currentPrice)
    {
        return (order.Side == OrderSide.Buy && currentPrice >= order.StopPrice) ||
               (order.Side == OrderSide.Sell && currentPrice <= order.StopPrice);
    }

    private void LogOrderSuccess(Order order, decimal currentPrice)
    {
        m_Logger.LogInformation(
            "Order {OrderId} processed successfully. Symbol: {Symbol}, Quantity: {Quantity}, Price: {Price}",
            order.Id, order.Symbol, order.Quantity, currentPrice);
    }

    private async Task HandleOrderError(Order order, Exception ex)
    {
        m_Logger.LogError(ex, "Error processing order {OrderId}", order.Id);
        order.Status = OrderStatus.Rejected;
        order.RejectionReason = "Internal server error";
    }
}

