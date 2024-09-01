using FinanceMaker.Algorithms.Chart;
using FinanceMaker.Common;
using FinanceMaker.Common.Models.Finance;
using FinanceMaker.Pullers.PricesPullers.Interfaces;

namespace FinanceMaker.Algorithms;

public sealed class KeyLevelsRunner : TickerRangeAlgorithmRunnerBase<double>
{
    public KeyLevelsRunner(IPricesPuller pricesPuller) : base(pricesPuller)
    {
    }

    public override Algorithm Algorithm => Algorithm.KeyLevels;

    public override Task<IEnumerable<double>> Run(IEnumerable<FinanceCandleStick> input, CancellationToken cancellationToken)
    {
        var keyLevels = KeyLevels.GetKeyLevels(input);

        return Task.FromResult(keyLevels);
    }
}
