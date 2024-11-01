using System;
using FinanceMaker.Common.Extensions;
using FinanceMaker.Common.Models.Finance;
using FinanceMaker.Common.Models.Finance.Enums;

namespace FinanceMaker.Algorithms.Chart
{
    public static class KeyLevels
    {
        public static IEnumerable<double> GetKeyLevels(IEnumerable<FinanceCandleStick> candles, int neighbors = 10)
        {
            if (candles is not IEnumerable<KeyLevelCandleStick> keyLevelsCandles)
            {
                keyLevelsCandles = candles.Select(c => new KeyLevelCandleStick(c, 0, Pivot.Unchanged));
            }

            var count = keyLevelsCandles.GetNonEnumeratedCount();
            var candlesArr = keyLevelsCandles.ToArray();
            var pivots = new Pivot[count];
            var levels = new List<double>();

            for (int i = 0; i < count; i++)
            {
                var pivot = GetPivot(candlesArr, i, neighbors, neighbors);

                pivots[i] = pivot;

                if (pivot == Pivot.High)
                {
                    levels.Add((double)candlesArr[i].High);
                }

                if (pivot == Pivot.Low)
                {
                    levels.Add((double)candlesArr[i].Low);
                }

                candlesArr[i].Pivot = pivot;
            }
            List<double> distinctedLevels = new List<double>();
            var epsilon = 0.1;

            for (int i = 0; i < levels.Count; i++)
            {
                if (!distinctedLevels.Any(level => level + level * epsilon >= levels[i] && levels[i] >= level - level * epsilon))
                {
                    distinctedLevels.Add(levels[i]);
                }
            }

            return distinctedLevels;
        }


        private static Pivot GetPivot(FinanceCandleStick[] chart, int index, int neighborsRight, int neighborsLeft)
        {
            if (index - neighborsLeft < 0 || (index + neighborsRight) >= chart.Length)
            {
                return Pivot.Unchanged;
            }

            Pivot pivotLow = Pivot.Low;
            Pivot pivotHigh = Pivot.High;

            for (int i = index - neighborsLeft; i < index + neighborsRight; i++)
            {
                if (chart[index].Low > chart[i].Low)
                {
                    pivotLow = Pivot.Unchanged;
                }
                if (chart[index].High < chart[i].High)
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
}

