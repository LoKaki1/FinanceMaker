using System;
using FinanceMaker.Common.Models.Finance;
using FinanceMaker.Common.Models.Finance.Enums;
using FinanceMaker.Common.Extensions;

namespace FinanceMaker.Algorithms.Chart
{
	public static class TrendDetection
	{
		public static IEnumerable<TrendTypes>
			CaluclateCandlesTrend(IEnumerable< FinanceCandleStick> financeCandleSticks, int backCandles = 15)	
		{
			var count = financeCandleSticks.GetNonEnumeratedCount();
			var trendTypes = new TrendTypes[count];

			if (financeCandleSticks.First().EMA == 0)
			{
                CalculateEMA.CalculateEMA(financeCandleSticks);
			}

            for (int i = backCandles; i < count; i++)
			{
				var up = true;
				var down = true;

				for (int j = i - backCandles; j < i + 1; j++)
				{
					up = Math.Max(financeCandleSticks.ElementAt(i).Open,
								  financeCandleSticks.ElementAt(i).Close) >= financeCandleSticks.ElementAt(i).EMA;

                    down = Math.Min(financeCandleSticks.ElementAt(i).Open,
                                  financeCandleSticks.ElementAt(i).Close) <= financeCandleSticks.ElementAt(i).EMA;
                }

				if (up && down)
				{
					financeCandleSticks.ElementAt(i).EMASignal = TrendTypes.NoChange;
				}
				else if (up)
				{
                    financeCandleSticks.ElementAt(i).EMASignal = TrendTypes.Bulish;
                }
				else if(down)
				{
					financeCandleSticks.ElementAt(i).EMASignal = TrendTypes.Berish;
				}

				trendTypes[i] = financeCandleSticks.ElementAt(i).EMASignal;
            }

			return trendTypes;
		}
	}
}

