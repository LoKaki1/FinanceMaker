using FinanceMaker.Common.Models;
using FinanceMaker.Common.Models.Tickers;

namespace FinanceMaker.Pullers.NewsPullers.Interfaces
{
    public interface INewPuller
	{
		Task<TickerNews> PullNews(Ticker ticker);
	}
}

