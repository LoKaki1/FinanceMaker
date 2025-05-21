using FinanceMaker.Broker.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace FinanceMaker.Broker.Services;

public class MarketDataService : IMarketDataService
{
    private readonly ILogger<MarketDataService> m_Logger;
    private readonly HttpClient m_HttpClient;

    public MarketDataService(
        ILogger<MarketDataService> logger,
        HttpClient httpClient)
    {
        m_Logger = logger;
        m_HttpClient = httpClient;
    }

    public async Task<decimal> GetCurrentPriceAsync(string symbol, CancellationToken cancellationToken)
    {
        try
        {
            // TODO: Implement actual market data provider integration
            // For now, return a mock price
            return 100.00m;
        }
        catch (Exception ex)
        {
            m_Logger.LogError(ex, "Error getting current price for {Symbol}", symbol);
            throw;
        }
    }
}
