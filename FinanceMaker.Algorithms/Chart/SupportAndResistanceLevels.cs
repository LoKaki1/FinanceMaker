using System;
using FinanceMaker.Common.Models.Finance.Enums;
using FinanceMaker.Common.Models.Tickers;

namespace FinanceMaker.Algorithms.Chart
{
	public static class SupportAndResistanceLevels
	{

		public static IEnumerable<double> GetSupportResistanceLevels(TickerChart tickerChart)
		{
			var pivots = new Pivot[tickerChart.Prices.Length];
			var levels = new List<double>();

			for (int i = 0; i < tickerChart.Prices.Length; i++)
			{
				var pivot = GetPivot(tickerChart, i, 10, 10);

				pivots[i] = pivot;

				if (pivot == Pivot.High)
				{
					levels.Add((double)tickerChart.Prices[i].High);
				}

                if (pivot == Pivot.Low)
                {
                    levels.Add((double)tickerChart.Prices[i].Low);
                }
            }
			List<double> distinctedLevels = new List<double>();
			var epsilon = 0.1;
			for(int i = 0; i < levels.Count; i++)
			{
				if (!distinctedLevels.Any(level => level + level * epsilon >= levels[i] && levels[i] >= level - level * epsilon))
				{
					distinctedLevels.Add(levels[i]);
				}
			}

			return distinctedLevels;
		}


		private static Pivot GetPivot(TickerChart chart, int index, int neighborsRight, int neighborsLeft)
		{
			if (index - neighborsLeft < 0|| (index + neighborsRight) >= chart.Prices.Length)
			{
				return Pivot.Unchanged;
			}

			Pivot pivotLow = Pivot.Low;
			Pivot pivotHigh = Pivot.High;

			for (int i = index - neighborsLeft; i < index + neighborsRight; i++)
			{
				if (chart.Prices[index].Low > chart.Prices[i].Low)
				{
					pivotLow = Pivot.Unchanged;
				}
				if (chart.Prices[index].High < chart.Prices[i].High)
				{
					pivotHigh = Pivot.Unchanged;
				}
			}

			if(pivotLow == pivotHigh)
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

