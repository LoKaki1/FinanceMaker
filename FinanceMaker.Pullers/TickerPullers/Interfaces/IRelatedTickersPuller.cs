using FinanceMaker.Common.Models;

namespace FinanceMaker.Pullers.TickerPullers.Interfaces
{
    public interface IRelatedTickersPuller
    {
        Task<IEnumerable<string>> GetRelatedTickers(string ticker, CancellationToken cancellationToken);
    }
}
