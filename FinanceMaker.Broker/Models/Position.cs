using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceMaker.Broker.Models;

public class Position
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
    public int Quantity { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal AveragePrice { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal LastPrice { get; set; }

    public decimal MarketValue => Quantity * LastPrice;
    public decimal UnrealizedPnL => MarketValue - (AveragePrice * Quantity);
    public decimal RealizedPnL { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
