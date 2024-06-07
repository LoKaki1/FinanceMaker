using FinanceMaker.Common.Models.Finance;
using FinanceMaker.Common.Models.Pullers.Enums;
using FinanceMaker.Common.Models.Tickers;
using FinanceMaker.Common.Resolvers.Interfaces;

namespace FinanceMaker.Pullers.PricesPullers.Interfaces
{
    public interface IPricesPuller: IResolveable<Period>
	{
		Task<IEnumerable<FinanceCandleStick>> GetTickerPrices(string ticker,
                                                        Period period,
                                                        DateTime endDate,
                                                        DateTime startDate,
                                                        CancellationToken cancellationToken);
	}
}

