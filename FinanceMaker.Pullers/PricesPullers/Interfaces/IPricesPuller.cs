using FinanceMaker.Common.Models;

namespace FinanceMaker.Pullers.PricesPullers.Interfaces
{
    public interface IPricesPuller
	{
		Task<TickerChart> GetTickerPrices(string ticker);
	}
}

