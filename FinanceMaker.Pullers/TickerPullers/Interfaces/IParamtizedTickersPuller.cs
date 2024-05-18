using FinanceMaker.Common.Models;
using FinanceMaker.Common.Models.Pullers;

namespace FinanceMaker.Pullers.TickerPullers.Interfaces
{
    public interface IParamtizedTickersPuller
	{
        Task<IEnumerable<Ticker>> ScanTickers(TickersPullerParameters scannerParams);
    }
}

