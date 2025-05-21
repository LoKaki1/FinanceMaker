using System.Threading;
using System.Threading.Tasks;
using FinanceMaker.Broker.Models;

namespace FinanceMaker.Broker.Interfaces
{
    public interface IOrderExecutionService
    {
        Task<Order> ExecuteOrderAsync(Order order, CancellationToken cancellationToken = default);
    }
}
