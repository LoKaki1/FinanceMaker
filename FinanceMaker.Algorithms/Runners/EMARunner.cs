using FinanceMaker.Algorithms.Chart;
using FinanceMaker.Common;
using FinanceMaker.Common.Models.Finance;
using FinanceMaker.Pullers.PricesPullers.Interfaces;

namespace FinanceMaker.Algorithms;

public sealed class EMARunner : TickerRangeAlgorithmRunnerBase<EMACandleStick>
{
    public override Algorithm Algorithm => Algorithm.EMA;
    
    public EMARunner(IPricesPuller pricesPuller) : base(pricesPuller)
    {
    }


    public override Task<IEnumerable<EMACandleStick>> Run(IEnumerable<FinanceCandleStick> input, CancellationToken cancellationToken)
    {
        var emaResult = EMACaluclator.CalculateEMA(input, cancellationToken);

        return Task.FromResult(emaResult);
    }
}
