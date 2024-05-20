using FinanceMaker.Common.Models;
using FinanceMaker.Common.Models.Tickers;

namespace FinanceMaker.Pullers.PricesPullers.Interfaces
{
    public interface IPricesPuller
	{
		Task<TickerChart> GetTickerPrices(string ticker);
	}
}

