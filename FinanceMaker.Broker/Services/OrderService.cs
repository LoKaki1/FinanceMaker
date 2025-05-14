using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FinanceMaker.Broker.Interfaces;
using FinanceMaker.Broker.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinanceMaker.Broker.Services;

public class OrderService : IOrderService
{
    private readonly IBrokerDbContext m_DbContext;
    private readonly ILogger<OrderService> m_Logger;

    public OrderService(IBrokerDbContext dbContext, ILogger<OrderService> logger)
    {
        m_DbContext = dbContext;
        m_Logger = logger;
    }

    public async Task<Order> PlaceOrderAsync(Order order, CancellationToken cancellationToken = default)
    {
        var user = await m_DbContext.Users.FindAsync(new object[] { order.Username }, cancellationToken);

        if (user is null)
        {
            throw new ArgumentException($"User {order.Username} not found");
        }

        // Validate order
        if (order.Type == OrderType.Market)
        {
            order.Price = 0; // Market orders don't have a price
        }

        // Check if user has enough balance for the order
        var requiredBalance = order.Price * order.Quantity;

        if (user.Balance < requiredBalance)
        {
            throw new InvalidOperationException($"Insufficient balance. Required: {requiredBalance}, Available: {user.Balance}");
        }

        await m_DbContext.Orders.AddAsync(order, cancellationToken);
        await m_DbContext.SaveChangesAsync(cancellationToken);

        return order;
    }

    public async Task<Order?> GetOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await m_DbContext.Orders
            .Include(o => o.ParentOrder)
            .Include(o => o.ChildOrders)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
    }

    public async Task<IEnumerable<Order>> GetUserOrdersAsync(string username, CancellationToken cancellationToken = default)
    {
        return await m_DbContext.Orders
            .Include(o => o.ParentOrder)
            .Include(o => o.ChildOrders)
            .Where(o => o.Username == username)
            .ToListAsync(cancellationToken);
    }

    public async Task CancelOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = await GetOrderAsync(orderId, cancellationToken) ?? throw new ArgumentException($"Order {orderId} not found");

        if (order.Status != OrderStatus.Pending)
        {
            throw new InvalidOperationException($"Cannot cancel order {orderId} with status {order.Status}");
        }

        order.Status = OrderStatus.Cancelled;
        await m_DbContext.SaveChangesAsync(cancellationToken);

        // Cancel child orders if any
        foreach (var childOrder in order.ChildOrders.Where(o => o.Status == OrderStatus.Pending))
        {
            childOrder.Status = OrderStatus.Cancelled;
        }

        await m_DbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<Order>> GetOpenOrdersAsync(CancellationToken cancellationToken = default)
    {
        return await m_DbContext.Orders
            .Include(o => o.ParentOrder)
            .Include(o => o.ChildOrders)
            .Where(o => o.Status == OrderStatus.Pending)
            .ToListAsync(cancellationToken);
    }
}
