using FinanceMaker.Common;
using FinanceMaker.Common.Models.Finance;
using FinanceMaker.Common.Models.Pullers.Enums;
using FinanceMaker.Pullers.PricesPullers.Interfaces;

namespace FinanceMaker.Pullers;

public sealed class YahooInterdayPricesPuller : IPricesPuller
{
    private readonly IHttpClientFactory m_RequestsService;
    private readonly string m_FinanceUrl;

    public YahooInterdayPricesPuller(IHttpClientFactory requestsService)
    {
        m_RequestsService = requestsService;
        m_FinanceUrl = "https://query1.finance.yahoo.com/v8/finance/chart/{0}?period1={1}&period2={2}&interval={3}&includePrePost=true&lang=en-US&region=US";
    }

    public Task<IEnumerable<FinanceCandleStick>> GetTickerPrices(PricesPullerParameters pricesPullerParameters,
                                                                 CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public bool IsRelevant(PricesPullerParameters args)
    {
        throw new NotImplementedException();
    }
}
