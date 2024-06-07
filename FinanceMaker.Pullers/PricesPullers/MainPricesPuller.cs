using FinanceMaker.Common.Models.Finance;
using FinanceMaker.Common.Models.Pullers.Enums;
using FinanceMaker.Common.Models.Tickers;
using FinanceMaker.Common.Resolvers.Abstracts;
using FinanceMaker.Pullers.PricesPullers.Interfaces;

namespace FinanceMaker.Pullers.PricesPullers
{
    public sealed class MainPricesPuller : ResolverBase<IPricesPuller, Period>, IPricesPuller
	{

        public MainPricesPuller(IPricesPuller[] pricesPuller) : base(pricesPuller)
        { }

        public Task<IEnumerable<FinanceCandleStick>> GetTickerPrices(string ticker,
                                                 Period period,
                                                 DateTime startDate,
                                                 DateTime endDate,
                                                 CancellationToken cancellationToken)
        {
            var resolvedPuller = Resolve(period);

            return resolvedPuller.GetTickerPrices(ticker, period, startDate, endDate, cancellationToken);
        }

        public bool IsRelevant(Period args)
        {
            return Resolve(args) is null;
        }
    }
}

