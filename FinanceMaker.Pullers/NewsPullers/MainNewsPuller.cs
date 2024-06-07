using System;
using FinanceMaker.Common.Models.Tickers;
using FinanceMaker.Pullers.NewsPullers.Interfaces;

namespace FinanceMaker.Pullers.NewsPullers
{
    public sealed class MainNewsPuller : INewsPuller
	{
        private readonly INewsPuller[] m_NewsPuller;

        public MainNewsPuller(INewsPuller[] newsPuller)
        {
            m_NewsPuller = newsPuller;
        }

        public async Task<IEnumerable<string>> PullNews(string ticker, CancellationToken cancellationToken)
        {
            var pullersTasks = m_NewsPuller.Select(puller => puller.PullNews(ticker, cancellationToken))
                                           .ToArray();

            var newsResult = await Task.WhenAll(pullersTasks);
            var news = newsResult.SelectMany(tickerNews => tickerNews)
                                  .ToArray();

            return news;
        }
    }
}

