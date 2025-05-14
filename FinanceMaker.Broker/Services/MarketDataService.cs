using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FinanceMaker.Broker.Interfaces;
using FinanceMaker.Common;
using FinanceMaker.Common.Models.Pullers.Enums;
using FinanceMaker.Pullers.PricesPullers;
using FinanceMaker.Pullers.PricesPullers.Interfaces;
using Microsoft.Extensions.Logging;

namespace FinanceMaker.Broker.Services;

public class MarketDataService : IMarketDataService
{
    private readonly MainPricesPuller m_PricesPuller;
    private readonly ILogger<MarketDataService> m_Logger;
    private readonly ConcurrentDictionary<string, decimal> m_LastPrices = new();
    private readonly SemaphoreSlim m_UpdateLock = new(1, 1);
    private Task? m_UpdateTask;
    private CancellationTokenSource? m_UpdateCts;

    public MarketDataService(ILogger<MarketDataService> logger)
    {
        m_PricesPuller = new MainPricesPuller(new IPricesPuller[0]); // Initialize with empty array for now
        m_Logger = logger;
    }

    public async Task<decimal> GetLastPriceAsync(string symbol, CancellationToken cancellationToken = default)
    {
        if (m_LastPrices.TryGetValue(symbol, out var price))
        {
            return price;
        }

        var prices = await GetLastPricesAsync(new[] { symbol }, cancellationToken);

        return prices[symbol];
    }

    public async Task<Dictionary<string, decimal>> GetLastPricesAsync(IEnumerable<string> symbols, CancellationToken cancellationToken = default)
    {
        var result = new Dictionary<string, decimal>();
        var missingSymbols = symbols.Where(s => !m_LastPrices.ContainsKey(s)).ToList();

        if (missingSymbols.Any())
        {
            var parameters = new PricesPullerParameters(
                missingSymbols.First(),
                DateTime.Now.AddDays(-1),
                DateTime.Now,
                Period.Daily
            );

            try
            {
                var priceData = await m_PricesPuller.GetTickerPrices(parameters, cancellationToken);
                var latestPrice = priceData.LastOrDefault();

                if (latestPrice is not null)
                {
                    m_LastPrices[missingSymbols.First()] = (decimal)latestPrice.Close;
                }
            }
            catch (Exception ex)
            {
                m_Logger.LogError(ex, "Error getting price data for {Symbol}", missingSymbols.First());
            }
        }

        foreach (var symbol in symbols)
        {
            if (m_LastPrices.TryGetValue(symbol, out var price))
            {
                result[symbol] = price;
            }
        }

        return result;
    }

    public Task StartMarketDataUpdatesAsync(CancellationToken cancellationToken = default)
    {
        if (m_UpdateTask is not null)
        {
            return m_UpdateTask;
        }

        m_UpdateCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        m_UpdateTask = Task.Run(async () =>
        {
            while (!m_UpdateCts.Token.IsCancellationRequested)
            {
                try
                {
                    await UpdatePricesAsync(m_UpdateCts.Token);
                }
                catch (Exception ex)
                {
                    m_Logger.LogError(ex, "Error updating prices");
                }

                await Task.Delay(5000, m_UpdateCts.Token);
            }
        }, m_UpdateCts.Token);

        return m_UpdateTask;
    }

    private async Task UpdatePricesAsync(CancellationToken cancellationToken)
    {
        if (!await m_UpdateLock.WaitAsync(0, cancellationToken))
        {
            return;
        }

        try
        {
            var symbols = m_LastPrices.Keys.ToList();

            if (!symbols.Any())
            {
                return;
            }

            var parameters = new PricesPullerParameters(
                symbols.First(),
                DateTime.Now.AddDays(-1),
                DateTime.Now,
                Period.Daily
            );

            var priceData = await m_PricesPuller.GetTickerPrices(parameters, cancellationToken);
            var latestPrice = priceData.LastOrDefault();

            if (latestPrice is not null)
            {
                m_LastPrices[symbols.First()] = (decimal)latestPrice.Close;
            }
        }
        finally
        {
            m_UpdateLock.Release();
        }
    }
}
