using FinanceMaker.Common;
using FinanceMaker.Common.Models.Finance;
using FinanceMaker.Common.Models.Pullers.Enums;
using FinanceMaker.Common.Models.Pullers.YahooFinance;
using FinanceMaker.Pullers.PricesPullers.Interfaces;
using System.Net.Http.Json;

namespace FinanceMaker.Pullers;

public sealed class YahooInterdayPricesPuller : IPricesPuller
{
    private readonly IHttpClientFactory m_RequestsService;
    private readonly Dictionary<Period, string> m_RelevantPeriods;
    private readonly string m_FinanceUrl;

    public YahooInterdayPricesPuller(IHttpClientFactory requestsService)
    {
        m_RequestsService = requestsService;
        m_RelevantPeriods = new Dictionary<Period, string> 
        {
            { Period.OneMinute, "1m" },
            { Period.ThreeMinutes, "3m" },
            { Period.OneHour, "1h"}
        };
        m_FinanceUrl = "https://query1.finance.yahoo.com/v8/finance/chart/{0}?period1={1}&period2={2}&interval={3}&includePrePost=true&lang=en-US&region=US";
    }

    public async Task<IEnumerable<FinanceCandleStick>> GetTickerPrices(PricesPullerParameters pricesPullerParameters,
                                                                       CancellationToken cancellationToken)
    {
        var period = pricesPullerParameters.Period;
        
        if (!m_RelevantPeriods.TryGetValue(period, out string? yahooPeriod))
        {
            throw new NotImplementedException($"Yahoo interday api doesn't support {Enum.GetName(period)} as a period");
        }

        var client  = m_RequestsService.CreateClient();
        client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36");
        var url = string.Format(m_FinanceUrl, pricesPullerParameters.Ticker, pricesPullerParameters.StartTime.Ticks, pricesPullerParameters.EndTime.Ticks, yahooPeriod);

        var yahooResponse = await client.GetFromJsonAsync<InterdayModel>(url, cancellationToken);

        var result = yahooResponse?.chart?.result?.FirstOrDefault();
        var indicators = result?.indicators?.quote?.FirstOrDefault();

        if (result is null || indicators is null)
        {
            throw new ArgumentException($"Yahoo api returned null for those params:\n{pricesPullerParameters}");
        }

        var timestamps = result.timestamp;
        var candles = new FinanceCandleStick[timestamps.Length];

        for(int i = 0; i < timestamps.Length; i++)
        {
            var candleDate = new DateTime(timestamps[i]);
            var open = indicators.open[i];
            var close = indicators.close[i];
            var low = indicators.low[i];
            var high= indicators.high[i];
            var volume = indicators.volume[i];

            candles[i] = new FinanceCandleStick(candleDate, open, close, high, low, volume);
        }

        return candles;
    }

    public bool IsRelevant(PricesPullerParameters args)
    {
        return m_RelevantPeriods.ContainsKey(args.Period) && args.EndTime - args.StartTime < TimeSpan.FromDays(7);
    }
}
