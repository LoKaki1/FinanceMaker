using FinanceMaker.Common;
using FinanceMaker.Common.Extensions;
using FinanceMaker.Common.Models.Finance;
using FinanceMaker.Common.Models.Finance.Enums;
using FinanceMaker.Pullers.PricesPullers.Interfaces;

namespace FinanceMaker.Algorithms;

public sealed class KeyLevelsRunner :
    TickerRangeAlgorithmRunnerBase<KeyLevelCandleSticks>
{
    private int m_Neighbors;
    private float m_Epsilon;

    public KeyLevelsRunner(IPricesPuller pricesPuller) : base(pricesPuller)
    {
        m_Neighbors = 10;
        m_Epsilon = 0.005f;
    }

    public override Algorithm Algorithm => Algorithm.KeyLevels;

    public override Task<KeyLevelCandleSticks> Run(IEnumerable<FinanceCandleStick> input,
                                                   CancellationToken cancellationToken)
    {
        var count = input.GetNonEnumeratedCount();
        var candlesArr = input.ToArray();
        var pivots = new Pivot[count];
        var levels = new List<float>();
        var emaCandles = new EMACandleStick[count];

        for (var i = 0; i < count; i++)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                throw new OperationCanceledException("Cancelled on key level detection");
            }
            var pivot = GetPivot(candlesArr, i, m_Neighbors, m_Neighbors);

            pivots[i] = pivot;

            if (pivot == Pivot.High)
            {
                levels.Add((float)candlesArr[i].High);
            }

            if (pivot == Pivot.Low)
            {
                levels.Add((float)candlesArr[i].Low);
            }

            emaCandles[i] = new(candlesArr[i], 0)
            {
                Pivot = pivot
            };
        }
        // We need to return it some how I don't know why
        List<float> distinctedLevels = [];
        var epsilon = m_Epsilon;

        for (int i = 0; i < levels.Count; i++)
        {
            if (!distinctedLevels.Any(level => level + level * epsilon >= levels[i] && levels[i] >= level - level * epsilon))
            {
                distinctedLevels.Add(levels[i]);
            }
        }
        var reuslt = new KeyLevelCandleSticks(emaCandles, distinctedLevels);
        return Task.FromResult(reuslt);
        // return Task.FromResult((IEnumerable<EMACandleStick>)emaCandles);
    }

    /// <summary>
    /// Calculate the pivot of the current candle by the NIO algorithm 
    /// it is just a banch of "ifs"
    /// </summary>
    /// <param name="candles"></param>
    /// <param name="index"></param>
    /// <param name="neighborsRight"></param>
    /// <param name="neighborsLeft"></param>
    /// <returns></returns> <summary>
    private static Pivot GetPivot(FinanceCandleStick[] candles,
                                  int index,
                                  int neighborsRight,
                                  int neighborsLeft)
    {
        if (index - neighborsLeft < 0 || (index + neighborsRight) >= candles.Length)
        {
            return Pivot.Unchanged;
        }

        Pivot pivotLow = Pivot.Low;
        Pivot pivotHigh = Pivot.High;

        for (int i = index - neighborsLeft; i < index + neighborsRight; i++)
        {
            if (candles[index].Low > candles[i].Low)
            {
                pivotLow = Pivot.Unchanged;
            }
            if (candles[index].High < candles[i].High)
            {
                pivotHigh = Pivot.Unchanged;
            }
        }

        if (pivotLow == pivotHigh)
        {
            return Pivot.Unchanged;
        }

        else if (pivotLow == Pivot.Low)
        {
            return pivotLow;
        }
        else if (pivotHigh == Pivot.High)
        {
            return pivotHigh;
        }

        return Pivot.Unchanged;
    }
}
