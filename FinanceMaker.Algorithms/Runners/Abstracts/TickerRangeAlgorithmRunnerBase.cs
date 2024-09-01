using System.Reflection;
using FinanceMaker.Common;
using FinanceMaker.Common.Models.Finance;
using FinanceMaker.Common.Models.Pullers.Enums;
using FinanceMaker.Pullers.PricesPullers.Interfaces;

namespace FinanceMaker.Algorithms;

public abstract class TickerRangeAlgorithmRunnerBase<T> : IAlgorithmRunner<RangeAlgorithmInput, object> 
{
    private readonly IPricesPuller m_PricesPuller;
    
    public AlgorithmType AlgorithmType  => AlgorithmType.Prices;
    public abstract Algorithm Algorithm { get; }

    public TickerRangeAlgorithmRunnerBase(IPricesPuller pricesPuller)
    {
        m_PricesPuller = pricesPuller;
    }

    public async Task<IEnumerable<T>> Run(RangeAlgorithmInput input, CancellationToken cancellationToken)
    {
        IEnumerable<FinanceCandleStick> prices = await m_PricesPuller.GetTickerPrices(input, cancellationToken);

        var result = await Run(prices, cancellationToken);

        return result;
    }

    public abstract Task<IEnumerable<T>> Run(IEnumerable<FinanceCandleStick> input, CancellationToken cancellationToken);

    public virtual bool IsRelevant(RangeAlgorithmInput args)
    {
        return args.Algorithm == Algorithm;
    }

    async Task<IEnumerable<object>> IAlgorithmRunner<RangeAlgorithmInput, object>.Run(RangeAlgorithmInput input, CancellationToken cancellationToken)
    {
        var result = await Run(input, cancellationToken);

        if (result is IEnumerable<T> actualResult)
        {
            return (IEnumerable<object>) actualResult;
        }

        throw new Exception($"Bad result {result}");
    }
}
