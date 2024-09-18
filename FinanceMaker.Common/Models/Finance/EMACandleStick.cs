using System;
using QuantConnect;

namespace FinanceMaker.Common.Models.Finance;

public class EMACandleStick : FinanceCandleStick
{
    public decimal EMA { get; set; }

    public EMACandleStick(DateTime dateTime, float open, float close, float high, float low, float volume) : base(dateTime, open, close, high, low, volume)
    {
        EMA = 0;
    }
    public EMACandleStick(DateTime dateTime, float open, float close, float high, float low, float volume, float eMA) : base(dateTime, open, close, high, low, volume)

    {
        EMA = (decimal) eMA;
    }

    public EMACandleStick(FinanceCandleStick candlestick, float ema) : base(candlestick)
    {
        EMA = (decimal) ema;
    }
}
