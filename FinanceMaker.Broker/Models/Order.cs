using System;
using FinanceMaker.Common.Models.Trades.Enums;

namespace FinanceMaker.Broker.Models;

public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Username { get; set; } = null!;
    public string Symbol { get; set; } = null!;
    public OrderType Type { get; set; }
    public OrderSide Side { get; set; }
    public decimal Price { get; set; }
    public decimal Quantity { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? FilledAt { get; set; }
    public decimal? FilledPrice { get; set; }
    public decimal? FilledQuantity { get; set; }
    public Guid? ParentOrderId { get; set; }
    public Order? ParentOrder { get; set; }
    public List<Order> ChildOrders { get; set; } = new();
}
