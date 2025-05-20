using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceMaker.Broker.Models;

public enum OrderType
{
    Market,
    Limit,
    Stop,
    StopLimit
}

public enum OrderStatus
{
    Open,
    Filled,
    Cancelled,
    Rejected
}

public enum OrderSide
{
    Buy,
    Sell
}

public class Order
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid AccountId { get; set; }

    [ForeignKey(nameof(AccountId))]
    public Account Account { get; set; } = null!;

    [Required]
    [MaxLength(10)]
    public string Symbol { get; set; } = string.Empty;

    [Required]
    public OrderType Type { get; set; }

    [Required]
    public OrderSide Side { get; set; }

    [Required]
    public int Quantity { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? LimitPrice { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? StopPrice { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? TakeProfitPrice { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? StopLossPrice { get; set; }

    [Required]
    public OrderStatus Status { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? FilledPrice { get; set; }

    public int? FilledQuantity { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? FilledAt { get; set; }

    public DateTime? CancelledAt { get; set; }

    public string? RejectionReason { get; set; }
}
