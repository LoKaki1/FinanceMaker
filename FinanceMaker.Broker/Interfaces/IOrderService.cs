using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FinanceMaker.Broker.Models;

namespace FinanceMaker.Broker.Interfaces;

public interface IOrderService
{
    Task<Order> PlaceOrderAsync(Order order, CancellationToken cancellationToken = default);
    Task<Order?> GetOrderAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Order>> GetUserOrdersAsync(string username, CancellationToken cancellationToken = default);
    Task CancelOrderAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Order>> GetOpenOrdersAsync(CancellationToken cancellationToken = default);
}
