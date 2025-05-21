using FinanceMaker.Broker.Data;
using FinanceMaker.Broker.Models.Enums;
using FinanceMaker.Broker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FinanceMaker.Broker.Services;

public class OrderProcessorBackgroundService : BackgroundService
{
    private readonly IServiceProvider m_ServiceProvider;
    private readonly ILogger<OrderProcessorBackgroundService> m_Logger;
    private readonly PeriodicTimer m_Timer;

    public OrderProcessorBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<OrderProcessorBackgroundService> logger)
    {
        m_ServiceProvider = serviceProvider;
        m_Logger = logger;
        m_Timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (await m_Timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var scope = m_ServiceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<BrokerDbContext>();
                var orderExecutionService = scope.ServiceProvider.GetRequiredService<IOrderExecutionService>();

                var pendingOrders = await dbContext.Orders
                    .Where(o => o.Status == OrderStatus.Pending)
                    .ToListAsync(stoppingToken);

                foreach (var order in pendingOrders)
                {
                    try
                    {
                        await orderExecutionService.ExecuteOrderAsync(order, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        m_Logger.LogError(ex, "Error processing order {OrderId}", order.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                m_Logger.LogError(ex, "Error in order processor background service");
            }
        }
    }
}
