using System;
using System.Threading;
using System.Threading.Tasks;

namespace FinanceMaker.Broker.Interfaces
{
    public interface IRetryPolicyService
    {
        Task<T> ExecuteWithRetryAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken = default);
        Task ExecuteWithRetryAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default);
    }
}
