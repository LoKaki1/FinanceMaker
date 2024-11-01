using System;
using FinanceMaker.Common.Models.Finance;
using FinanceMaker.Common.Models.Finance.Enums;
using FinanceMaker.Common.Extensions;

namespace FinanceMaker.Algorithms.Chart
{
    public static class TrendDetection
    {
        public static IEnumerable<TrendTypes>
            CaluclateCandlesTrend(IEnumerable<FinanceCandleStick> financeCandleSticks, CancellationToken token, int backCandles = 15)
        {
            var count = financeCandleSticks.GetNonEnumeratedCount();
            var trendTypes = new TrendTypes[count];

            if (financeCandleSticks is not IEnumerable<EMACandleStick> emaCandleStick)
            {
                emaCandleStick = EMACaluclator.CalculateEMA(financeCandleSticks, token);
            }

            for (int i = backCandles; i < count; i++)
            {
                var up = true;
                var down = true;

                for (int j = i - backCandles; j < i + 1; j++)
                {
                    up = Math.Max(emaCandleStick.ElementAt(i).Open,
                                  emaCandleStick.ElementAt(i).Close) >= emaCandleStick.ElementAt(i).EMA;

                    down = Math.Min(emaCandleStick.ElementAt(i).Open,
                                  emaCandleStick.ElementAt(i).Close) <= emaCandleStick.ElementAt(i).EMA;
                }

                if (up && down)
                {
                    emaCandleStick.ElementAt(i).EMASignal = TrendTypes.NoChange;
                }
                else if (up)
                {
                    emaCandleStick.ElementAt(i).EMASignal = TrendTypes.Bulish;
                }
                else if (down)
                {
                    emaCandleStick.ElementAt(i).EMASignal = TrendTypes.Berish;
                }

                trendTypes[i] = emaCandleStick.ElementAt(i).EMASignal;
            }

            return trendTypes;
        }
    }
}

