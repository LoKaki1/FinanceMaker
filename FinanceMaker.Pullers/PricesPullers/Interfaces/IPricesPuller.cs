using FinanceMaker.Common;
using FinanceMaker.Common.Models.Finance;
using FinanceMaker.Common.Models.Pullers.Enums;
using FinanceMaker.Common.Models.Tickers;
using FinanceMaker.Common.Resolvers.Interfaces;

namespace FinanceMaker.Pullers.PricesPullers.Interfaces
{
    public interface IPricesPuller: IResolveable<PricesPullerParameters>
	{
		Task<IEnumerable<FinanceCandleStick>> GetTickerPrices(PricesPullerParameters pricesPullerParameters,
                                                              CancellationToken cancellationToken);
	}
}

