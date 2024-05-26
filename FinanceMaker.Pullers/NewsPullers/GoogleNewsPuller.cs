using System;
using FinanceMaker.Common.Models.Tickers;
using FinanceMaker.Pullers.NewsPullers.Interfaces;

namespace FinanceMaker.Pullers.NewsPullers
{
    public sealed class GoogleNewsPuller : INewsPuller
    {
        private readonly IHttpClientFactory m_RequestService;
        private readonly string m_NewsUrl;

        public GoogleNewsPuller(IHttpClientFactory requestService)
        {
            m_RequestService = requestService;
            m_NewsUrl = "https://www.google.com/search?q={0}&tbm=nws&hl=en";
        }

        public async Task<TickerNews> PullNews(string ticker, CancellationToken cancellationToken)
        {
            var client = m_RequestService.CreateClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36");
            
            var url = string.Format(m_NewsUrl, ticker);
            var googleResponse = await client.GetAsync(url, cancellationToken);
            var htmlContent = await googleResponse.Content.ReadAsStringAsync(cancellationToken);
            var htmlDocument = new HtmlAgilityPack.HtmlDocument();
       
            htmlDocument.LoadHtml(htmlContent);
            var nodes = htmlDocument.DocumentNode.SelectNodes("//a[@class='WlydOe']");
            var hrefs = nodes.Select(node => node.GetAttributeValue("href", string.Empty))
                             .Where(href => !string.IsNullOrEmpty(href))
                             .ToArray();

            var tickerNews = new TickerNews(ticker, hrefs);

            return tickerNews;
        }
    }

}

