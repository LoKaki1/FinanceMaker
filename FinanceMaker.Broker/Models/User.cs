using System;

namespace FinanceMaker.Broker.Models;

public class User
{
    public string Username { get; set; } = null!;
    public decimal Balance { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
