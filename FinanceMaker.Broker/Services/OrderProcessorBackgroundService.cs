using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FinanceMaker.Broker.Services;

public class OrderProcessorBackgroundService : BackgroundService
{
    private readonly OrderProcessorService m_OrderProcessor;
    private readonly ILogger<OrderProcessorBackgroundService> m_Logger;

    public OrderProcessorBackgroundService(
        OrderProcessorService orderProcessor,
        ILogger<OrderProcessorBackgroundService> logger)
    {
        m_OrderProcessor = orderProcessor;
        m_Logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await m_OrderProcessor.ProcessOrdersAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                m_Logger.LogError(ex, "Error occurred while processing orders");
            }

            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }
    }
}
