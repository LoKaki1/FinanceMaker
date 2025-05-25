using Microsoft.Extensions.Logging;

namespace FinanceMaker.Broker.Services;

public interface IPriceManagementService
{
    decimal GetCurrentPrice(string symbol);
    void UpdatePrice(string symbol, decimal price);
}

public class PriceManagementService : IPriceManagementService
{
    private readonly Dictionary<string, decimal> m_LastPrices = new();
    private readonly object m_Lock = new();
    private readonly ILogger<PriceManagementService> m_Logger;

    public PriceManagementService(ILogger<PriceManagementService> logger)
    {
        m_Logger = logger;
    }

    public decimal GetCurrentPrice(string symbol)
    {
        lock (m_Lock)
        {
            return m_LastPrices.TryGetValue(symbol, out var price) ? price : 0;
        }
    }

    public void UpdatePrice(string symbol, decimal price)
    {
        lock (m_Lock)
        {
            m_LastPrices[symbol] = price;
            m_Logger.LogInformation("Updated price for {Symbol} to {Price}", symbol, price);
        }
    }
}
