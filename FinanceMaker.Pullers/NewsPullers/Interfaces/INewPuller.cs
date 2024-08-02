using FinanceMaker.Common.Models.Pullers;

namespace FinanceMaker.Pullers.NewsPullers.Interfaces
{
    public interface INewsPuller
	{
		Task<IEnumerable<string>> PullNews(NewsPullerParameters ticker, CancellationToken cancellationToken);
	}
}

