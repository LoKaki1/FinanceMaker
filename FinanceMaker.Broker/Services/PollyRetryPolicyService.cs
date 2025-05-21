using System;
using System.Threading;
using System.Threading.Tasks;
using FinanceMaker.Broker.Interfaces;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace FinanceMaker.Broker.Services
{
    public class PollyRetryPolicyService : IRetryPolicyService
    {
        private readonly ILogger<PollyRetryPolicyService> m_Logger;
        private readonly AsyncRetryPolicy m_RetryPolicy;

        public PollyRetryPolicyService(ILogger<PollyRetryPolicyService> logger)
        {
            m_Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            m_RetryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    3,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (exception, timeSpan, retryCount, context) =>
                    {
                        m_Logger.LogWarning(
                            exception,
                            "Retry {RetryCount} after {Delay}ms due to {Exception}",
                            retryCount,
                            timeSpan.TotalMilliseconds,
                            exception.Message);
                    });
        }

        public async Task<T> ExecuteWithRetryAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken = default)
        {
            return await m_RetryPolicy.ExecuteAsync(async () =>
            {
                try
                {
                    return await action(cancellationToken);
                }
                catch (Exception ex)
                {
                    m_Logger.LogError(ex, "Error executing action with retry");
                    throw;
                }
            });
        }

        public async Task ExecuteWithRetryAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default)
        {
            await m_RetryPolicy.ExecuteAsync(async () =>
            {
                try
                {
                    await action(cancellationToken);
                }
                catch (Exception ex)
                {
                    m_Logger.LogError(ex, "Error executing action with retry");
                    throw;
                }
            });
        }
    }
}
