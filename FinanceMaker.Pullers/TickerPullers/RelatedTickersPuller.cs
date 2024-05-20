using FinanceMaker.Common.Models;
using FinanceMaker.Pullers.TickerPullers.Interfaces;

namespace FinanceMaker.Pullers.TickerPullers
{
    public sealed class RelatedTickersPuller : IRelatedTickersPuller
    {
        public Task<IEnumerable<Ticker>> GetRelatedTickers(Ticker ticker)
        {
            throw new NotImplementedException();
        }
    }
}
