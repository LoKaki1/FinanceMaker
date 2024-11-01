using System;
using FinanceMaker.Common.Extensions;
using FinanceMaker.Common.Models.Finance;
using QuantConnect.Orders.Fills;

namespace FinanceMaker.Algorithms.Chart
{
    public static class EMACaluclator

    {
        public static IEnumerable<EMACandleStick> CalculateEMA(IEnumerable<FinanceCandleStick> financeCandleSticks, CancellationToken token, int period = 10)
        {

        }
    }
}

