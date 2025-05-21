namespace FinanceMaker.Broker.Services.Interfaces;

public interface IMarketDataService
{
    Task<decimal> GetCurrentPriceAsync(string symbol, CancellationToken cancellationToken);
}
