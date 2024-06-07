using FinanceMaker.Common.Models;
using FinanceMaker.Common.Models.Tickers;

namespace FinanceMaker.Pullers.NewsPullers.Interfaces
{
    public interface INewsPuller
	{
		Task<IEnumerable<string>> PullNews(string ticker, CancellationToken cancellationToken);
	}
}

