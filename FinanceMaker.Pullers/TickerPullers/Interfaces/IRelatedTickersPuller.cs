using FinanceMaker.Common.Models;

namespace FinanceMaker.Pullers.TickerPullers.Interfaces
{
    public interface IRelatedTickersPuller
    {
        Task<IEnumerable<Ticker>> GetRelatedTickers(Ticker ticker);
    }
}
