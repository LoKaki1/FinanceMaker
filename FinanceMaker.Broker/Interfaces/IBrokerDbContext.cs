using System.Threading;
using System.Threading.Tasks;
using FinanceMaker.Broker.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceMaker.Broker.Interfaces;

public interface IBrokerDbContext
{
    DbSet<User> Users { get; }
    DbSet<Order> Orders { get; }
    DbSet<Position> Positions { get; }
    DbSet<Trade> Trades { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
