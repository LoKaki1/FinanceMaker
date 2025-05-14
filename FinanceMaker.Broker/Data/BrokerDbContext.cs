using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FinanceMaker.Broker.Interfaces;
using FinanceMaker.Broker.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceMaker.Broker.Data;

public class BrokerDbContext : DbContext, IBrokerDbContext
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<Position> Positions { get; set; } = null!;
    public DbSet<Trade> Trades { get; set; } = null!;

    public BrokerDbContext(DbContextOptions<BrokerDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Username);
            entity.Property(e => e.Balance).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Price).HasPrecision(18, 2);
            entity.Property(e => e.Quantity).HasPrecision(18, 2);
            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(e => e.Username);
        });

        modelBuilder.Entity<Position>(entity =>
        {
            entity.HasKey(e => new { e.Username, e.Symbol });
            entity.Property(e => e.Quantity).HasPrecision(18, 2);
            entity.Property(e => e.AveragePrice).HasPrecision(18, 2);
            entity.Property(e => e.CurrentPrice).HasPrecision(18, 2);
            entity.Property(e => e.UnrealizedPnL).HasPrecision(18, 2);
            entity.Property(e => e.RealizedPnL).HasPrecision(18, 2);
            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(e => e.Username);
        });

        modelBuilder.Entity<Trade>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Price).HasPrecision(18, 2);
            entity.Property(e => e.Quantity).HasPrecision(18, 2);
            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(e => e.Username);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is User && e.State == EntityState.Added);

        foreach (var entry in entries)
        {
            if (entry.Entity is User user)
            {
                user.Balance = 25000m; // Default starting balance
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
