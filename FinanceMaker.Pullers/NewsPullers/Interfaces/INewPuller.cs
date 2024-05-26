using FinanceMaker.Common.Models;
using FinanceMaker.Common.Models.Tickers;

namespace FinanceMaker.Pullers.NewsPullers.Interfaces
{
    public interface INewsPuller
	{
		Task<TickerNews> PullNews(string ticker, CancellationToken cancellationToken);
	}
}

