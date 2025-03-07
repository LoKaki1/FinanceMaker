using System;
using System.Threading;
using System.Threading.Tasks;
using FinanceMaker.Publisher.Traders.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NCrontab;

public class Worker : BackgroundService
{
    private NCrontab.CrontabSchedule m_CrontabSchedule;
    private readonly ILogger<Worker> m_Logger;
    private readonly ITrader m_Trader;

    public Worker(ILogger<Worker> logger, ITrader workerTrader)
    {
        m_Logger = logger;
        m_Trader = workerTrader;
        m_CrontabSchedule = CrontabSchedule.Parse("59 1 * * *");

    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.Now;
            DateTime nextExecutionTime = m_CrontabSchedule.GetNextOccurrence(now);
            await m_Trader.Trade(stoppingToken);
            await Task.Delay(60 * 5 * 1000);

        }
    }
}
