using FinanceMaker.Common.Extensions;
using FinanceMaker.Common.Models.Pullers;
using FinanceMaker.Pullers.TickerPullers.Interfaces;
using HtmlAgilityPack;

namespace FinanceMaker.Pullers.TickerPullers
{
    public sealed class MostVolatiltyTickers : IParamtizedTickersPuller
    {
        private readonly IHttpClientFactory m_RequestService;
        private readonly string m_TradingViewUrl;
        public MostVolatiltyTickers(IHttpClientFactory requestService)
        {
            m_RequestService = requestService;
            m_TradingViewUrl = "https://www.tradingview.com/markets/stocks-usa/market-movers-most-volatile/?utm_source=chatgpt.com";

        }
        public async Task<IEnumerable<string>> ScanTickers(TickersPullerParameters scannerParams, CancellationToken cancellationToken)
        {
            var client = m_RequestService.CreateClient();
            client.AddBrowserUserAgent();
            var result = await client.GetAsync(m_TradingViewUrl, cancellationToken);
            var content = await result.Content.ReadAsStringAsync(cancellationToken);
            var doc = new HtmlDocument();
            doc.LoadHtml(content);
            var tickers = doc.DocumentNode.SelectNodes("//tr[@class=\"row-RdUXZpkv listRow\"]").Select(_ => _.Attributes["data-rowkey"].Value.Split(':')[1]).ToArray();

            if (tickers is null)
            {
                return [];
            }

            return tickers.Where(_ => !string.IsNullOrWhiteSpace(_));
        }
    }
}
