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

public class OrderProcessorService : IOrderProcessorService
{
    private readonly BrokerDbContext m_DbContext;
    private readonly ILogger<OrderProcessorService> m_Logger;
    private readonly Dictionary<string, decimal> m_LastPrices = new();
    private readonly object m_Lock = new();
    private readonly Dictionary<OrderType, Func<Order, decimal, Account, Task>> m_OrderProcessors;

    public OrderProcessorService(
        BrokerDbContext dbContext,
        ILogger<OrderProcessorService> logger)
    {
        m_DbContext = dbContext;
        m_Logger = logger;
        m_OrderProcessors = new Dictionary<OrderType, Func<Order, decimal, Account, Task>>
        {
            { OrderType.Market, ExecuteMarketOrder },
            { OrderType.Limit, ProcessLimitOrder },
            { OrderType.Stop, ProcessStopOrder }
        };
    }

    public async Task<OrderResponse> ProcessOrderAsync(Order order)
    {
        try
        {
            if (!await ValidateOrderAsync(order))
            {
                return OrderResponse.CreateError("Order validation failed");
            }

            decimal currentPrice = GetCurrentPrice(order.Symbol);
            if (currentPrice == 0)
            {
                return OrderResponse.CreateError("No current price available for symbol");
            }

            var account = await GetAccount(order.AccountId);
            if (account is null)
            {
                return OrderResponse.CreateError("Account not found");
            }

            if (order.Side == OrderSide.Buy && account.AvailableFunds < order.Quantity * currentPrice)
            {
                return OrderResponse.CreateError("Insufficient funds");
            }

            await ProcessOrder(order, currentPrice, account);
            await CreateTransaction(order, currentPrice, account);
            LogOrderSuccess(order, currentPrice);
            return OrderResponse.CreateSuccess(order);
        }
        catch (Exception ex)
        {
            await HandleOrderError(order, ex);
            return OrderResponse.CreateError("Internal server error");
        }
    }

    public void UpdatePrice(string symbol, decimal price)
    {
        lock (m_Lock)
        {
            m_LastPrices[symbol] = price;
        }
    }

    private async Task<bool> ValidateOrderAsync(Order order)
    {
        if (order.Quantity <= 0 || string.IsNullOrWhiteSpace(order.Symbol))
        {
            return false;
        }
        return await m_DbContext.Accounts.AnyAsync(a => a.Id == order.AccountId);
    }

    private decimal GetCurrentPrice(string symbol)
    {
        lock (m_Lock)
        {
            return m_LastPrices.TryGetValue(symbol, out var price) ? price : 0;
        }
    }

    private async Task<Account?> GetAccount(Guid accountId)
    {
        return await m_DbContext.Accounts.FirstOrDefaultAsync(a => a.Id == accountId);
    }

    private async Task ProcessOrder(Order order, decimal currentPrice, Account account)
    {
        if (!m_OrderProcessors.TryGetValue(order.Type, out var processor))
        {
            throw new ArgumentException($"Unsupported order type: {order.Type}");
        }
        await processor(order, currentPrice, account);
    }

    private async Task ProcessLimitOrder(Order order, decimal currentPrice, Account account)
    {
        if (order.Price is null)
        {
            throw new ArgumentException("Limit price is required for limit orders");
        }
        if (IsLimitOrderExecutable(order, currentPrice))
        {
            await ExecuteMarketOrder(order, currentPrice, account);
        }
    }

    private bool IsLimitOrderExecutable(Order order, decimal currentPrice)
    {
        return (order.Side == OrderSide.Buy && currentPrice <= order.Price) ||
               (order.Side == OrderSide.Sell && currentPrice >= order.Price);
    }

    private async Task ProcessStopOrder(Order order, decimal currentPrice, Account account)
    {
        if (order.StopPrice is null)
        {
            throw new ArgumentException("Stop price is required for stop orders");
        }
        if (IsStopOrderExecutable(order, currentPrice))
        {
            await ExecuteMarketOrder(order, currentPrice, account);
        }
    }

    private bool IsStopOrderExecutable(Order order, decimal currentPrice)
    {
        return (order.Side == OrderSide.Buy && currentPrice >= order.StopPrice) ||
               (order.Side == OrderSide.Sell && currentPrice <= order.StopPrice);
    }

    private async Task CreateTransaction(Order order, decimal currentPrice, Account account)
    {
        var transaction = new Transaction
        {
            UserId = account.UserId,
            Symbol = order.Symbol,
            Quantity = (int)order.Quantity,
            Price = currentPrice,
            Type = order.Side == OrderSide.Buy ? TransactionType.Buy : TransactionType.Sell,
            Timestamp = DateTime.UtcNow
        };
        m_DbContext.Transactions.Add(transaction);
        await m_DbContext.SaveChangesAsync();
    }

    private async Task ExecuteMarketOrder(Order order, decimal currentPrice, Account account)
    {
        var totalCost = order.Quantity * currentPrice;
        account.AvailableFunds += order.Side == OrderSide.Buy ? -totalCost : totalCost;
        order.Status = OrderStatus.Filled;
        order.FilledPrice = currentPrice;
        order.FilledAt = DateTime.UtcNow;
        await m_DbContext.SaveChangesAsync();
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
        await m_DbContext.SaveChangesAsync();
    }
}

