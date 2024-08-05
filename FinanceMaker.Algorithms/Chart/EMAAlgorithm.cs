using System;
using FinanceMaker.Common.Extensions;
using FinanceMaker.Common.Models.Finance;
using QuantConnect.Orders.Fills;

namespace FinanceMaker.Algorithms.Chart
{
	public static class EMACaluclator

	{
		public static IEnumerable<decimal> CalculateEMA(IEnumerable<FinanceCandleStick> financeCandleSticks, int period = 10)
		{
            var count = financeCandleSticks.GetNonEnumeratedCount();
            decimal[] emaValues = new decimal[count];
            decimal[] prices = financeCandleSticks.Select(_ => _.Close)
                                                  .ToArray();

            decimal multiplier = (decimal) (2.0 / (period + 1));
            decimal ema = prices[0]; // Start with the first price

            emaValues[0] = ema; // Add the first EMA value

            for (int i = 1; i < prices.Length; i++)
            {
                ema = ((prices[i] - ema) * multiplier) + ema;
                emaValues[i] = ema;
                financeCandleSticks.ElementAt(i).EMA = ema;
            }


            return emaValues;
        }
	}
}

