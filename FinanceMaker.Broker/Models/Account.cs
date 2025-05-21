using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceMaker.Broker.Models;

public class Account
{
    [Key]
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    [Required]
    public Guid UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal CashBalance { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal ClosedPnL { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal MarginUsed { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal AvailableFunds { get; set; }

    public DateTime LastUpdated { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Position> Positions { get; set; } = new List<Position>();

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
