using FinanceMaker.Common.Models.Finance;
using FinanceMaker.Common.Models.Tickers;
using FinanceMaker.Common.Resolvers.Interfaces;
using FinanceMaker.Pullers.PricesPullers.Interfaces;
using YahooFinanceApi;
using Period = FinanceMaker.Common.Models.Pullers.Enums.Period;
using YahooPeriod = YahooFinanceApi.Period;

namespace FinanceMaker.Pullers.PricesPullers
{
    public class YahooPricesPuller: IPricesPuller
	{
        private readonly IDictionary<Period, YahooPeriod> m_PeriodToPeriod;

        public YahooPricesPuller()
        {
            m_PeriodToPeriod = new Dictionary<Period, YahooPeriod>()
            {
                {  Period.Daily, YahooPeriod.Daily },
                {  Period.Weekly, YahooPeriod.Weekly },
                { Period.Monthly, YahooPeriod.Monthly }
            };
        }


        public async Task<TickerChart> GetTickerPrices(string ticker,
                                                       Period period,
                                                       DateTime startDate,
                                                       DateTime endDate,
                                                       CancellationToken cancellationToken)
        {
            if (!m_PeriodToPeriod.TryGetValue(period, out YahooPeriod yahooPeriod))
            {
                throw new NotImplementedException($"Yahoo api doesn't support {Enum.GetName(period)} as a period");
            }

            var historicalData = await Yahoo.GetHistoricalAsync(ticker, startDate, endDate, yahooPeriod, cancellationToken);

            var tickerCandles = historicalData.Select(data => new FinanceCandleStick(data.DateTime, data.Open, data.Close, data.High, data.Low, data.Volume))
                                              .ToArray();

            var chart = new TickerChart(ticker, tickerCandles);

            return chart;
        }

        public bool IsRelevant(Period args)
        {
            return m_PeriodToPeriod.ContainsKey(args);
        }
    }
}

