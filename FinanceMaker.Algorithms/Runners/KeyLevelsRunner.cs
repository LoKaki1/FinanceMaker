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
        
        foreach (var keyLevel in keyLevels)
        {
            var closestCandle = input.FirstOrDefault(candle
                     => Math.Abs((double)candle.High - keyLevel) < 0.03 ||
                        Math.Abs((double)candle.Low - keyLevel) < 0.03);

            if (closestCandle is null)
            {
                // How ??

                closestCandle = new FinanceCandleStick(DateTime.Now, 0f, 0f, 0, 0, 0);
            }

            var keylevelCandleStick = new KeyLevelCandleStick(closestCandle, (decimal)keyLevel);
            keyLevelsCandleSticks.Add(keylevelCandleStick);
        }

        return Task.FromResult((IEnumerable<KeyLevelCandleStick>) keyLevelsCandleSticks);
    }
}
