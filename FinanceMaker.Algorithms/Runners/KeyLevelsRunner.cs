using FinanceMaker.Algorithms.Chart;
using FinanceMaker.Common;
using FinanceMaker.Common.Models.Finance;
using FinanceMaker.Pullers.PricesPullers.Interfaces;

namespace FinanceMaker.Algorithms;

public sealed class KeyLevelsRunner : TickerRangeAlgorithmRunnerBase<KeyLevelCandleStick>
{
    public KeyLevelsRunner(IPricesPuller pricesPuller) : base(pricesPuller)
    {
    }

    public override Algorithm Algorithm => Algorithm.KeyLevels;

    public override Task<IEnumerable<KeyLevelCandleStick>> Run(IEnumerable<FinanceCandleStick> input, CancellationToken cancellationToken)
    {
        var keyLevels = KeyLevels.GetKeyLevels(input);
        var keyLevelsCandleSticks = new List<KeyLevelCandleStick>();
        
        // I don't like it because it reduces the number of candles
        foreach (var candle in input)
        {
            var relevantKeyLevel = keyLevels.FirstOrDefault(keyLevel
                     => Math.Abs((double)candle.High - keyLevel) < 0.03 ||
                        Math.Abs((double)candle.Low - keyLevel) < 0.03);

            // How ??

            var keylevelCandleStick = new KeyLevelCandleStick(candle, (decimal)relevantKeyLevel);
            keyLevelsCandleSticks.Add(keylevelCandleStick);
        }

        return Task.FromResult((IEnumerable<KeyLevelCandleStick>) keyLevelsCandleSticks);
    }
}
