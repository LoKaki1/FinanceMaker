using FinanceMaker.Common.Models;

namespace FinanceMaker.Pullers.TickerPullers.Interfaces;

public interface ITickerPuller
{
    Task<IEnumerable<Ticker>> ScanTickers();
}


