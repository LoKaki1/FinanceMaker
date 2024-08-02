using Accord;
using FinanceMaker.Common.Extensions;
using FinanceMaker.Common.Models.Finance;
using FinanceMaker.Common.Models.Finance.Enums;

namespace FinanceMaker.Algorithms.Chart
{
    public static class BreakoutDetection
	{
		public static IEnumerable<TrendTypes> DetectBreakoutCandles(IEnumerable<FinanceCandleStick> financeCandleSticks,
															 int backCandles,
															 int window,
                                                             int numOfCandlesToBeConsideredAsBreakout)
		{
			if (financeCandleSticks.All(_ => _.Pivot == Pivot.Unchanged))
			{
				KeyLevels.GetKeyLevels(financeCandleSticks, window - 1);
            }

			var arr = financeCandleSticks.ToArray();
			var breakouts = new TrendTypes[arr.Length];

			for (int i = 0; i < arr.Length; i++)
			
			{
				var breakout = IsItBreakoutCandle(arr, i, backCandles, window, numOfCandlesToBeConsideredAsBreakout);
				breakouts[i] = breakout;
				arr[i].BreakThrough = breakout;
			}

			return breakouts;
		}

		private static TrendTypes IsItBreakoutCandle(FinanceCandleStick[] financeCandleSticks,
                                        int index,
                                        int backCandles,
                                        int window,
                                        int numOfCandlesToBeConsideredAsBreakout)
		{
			if (index <= (backCandles + window) || (index + window > financeCandleSticks.Length)) return TrendTypes.NoChange;


			var smallArray = financeCandleSticks.Skip(index - backCandles - window)
												.Take(financeCandleSticks.Length - index  - window)
												.ToArray();

			var highs = smallArray.Where(_ => _.Pivot == Pivot.High)
								  .Select(_ => _.High)
								  .TakeLast(numOfCandlesToBeConsideredAsBreakout);
			var lows = smallArray.Where(_ => _.Pivot == Pivot.Low)
                                  .Select(_ => _.Low)
                                  .TakeLast(numOfCandlesToBeConsideredAsBreakout);

			var levelBreak = TrendTypes.NoChange;
			var zoneWidth = 0.0002M;

			if (lows.GetNonEnumeratedCount() == numOfCandlesToBeConsideredAsBreakout)
			{
				var supportCondition = true;
				var lowAverage = lows.Average();

				foreach(var low in lows)
				{
					if (Math.Abs(lowAverage - low) > zoneWidth)
					{
						supportCondition = false;
						break;
					}
				}

				if (supportCondition && (lowAverage - financeCandleSticks[index].Close) > zoneWidth)
				{
					levelBreak = TrendTypes.Berish;
				} 
			}

			if (highs.GetNonEnumeratedCount() == numOfCandlesToBeConsideredAsBreakout)
			{
                var resistanceCondition = true;
                var highAverage = highs.Average();

                foreach (var high in highs)
                {
                    if (Math.Abs(high - highAverage) > zoneWidth)
                    {
                        resistanceCondition = false;
                        break;
                    }
                }

                if (resistanceCondition && (financeCandleSticks[index].Close - highAverage) > zoneWidth)
                {
                    levelBreak = TrendTypes.Bulish;
                }
            }


			return levelBreak;
        }
	}
}

