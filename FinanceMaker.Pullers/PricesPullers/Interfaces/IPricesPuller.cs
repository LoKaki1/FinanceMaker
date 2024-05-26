using FinanceMaker.Common.Models.Pullers.Enums;
using FinanceMaker.Common.Models.Tickers;
using FinanceMaker.Common.Resolvers.Interfaces;

namespace FinanceMaker.Pullers.PricesPullers.Interfaces
{
    public interface IPricesPuller: IResolveable<Period>
	{
		Task<TickerChart> GetTickerPrices(string ticker,
                                          Period period,
                                          DateTime startDate,
                                          DateTime endDate,
                                          CancellationToken cancellationToken);
	}
}

