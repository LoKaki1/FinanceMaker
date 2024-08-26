using FinanceMaker.Common;
using FinanceMaker.Common.Models.Finance;
using FinanceMaker.Common.Models.Pullers.Enums;
using FinanceMaker.Pullers.PricesPullers.Interfaces;

namespace FinanceMaker.Algorithms;

public abstract class TickerRangeAlgorithmRunnerBase<T> : IAlgorithmRunner<PricesPullerParameters, T>
{
    private readonly IPricesPuller m_PricesPuller;
    
    public AlgorithmType Algorithm  => AlgorithmType.Prices;
    
    public TickerRangeAlgorithmRunnerBase(IPricesPuller pricesPuller)
    {
        m_PricesPuller = pricesPuller;
    }

    public async Task<IEnumerable<T>> Run(PricesPullerParameters input, CancellationToken cancellationToken)
    {
        IEnumerable<FinanceCandleStick> prices = await m_PricesPuller.GetTickerPrices(input, cancellationToken);

        var result = await Run(prices, cancellationToken);

        return result;
    }

    public abstract Task<IEnumerable<T>> Run(IEnumerable<FinanceCandleStick> input, CancellationToken cancellationToken);
}
