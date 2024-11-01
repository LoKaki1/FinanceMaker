using System;
using FinanceMaker.Common.Models.Finance.Enums;
using QuantConnect;

namespace FinanceMaker.Common.Models.Finance;

public class EMACandleStick : FinanceCandleStick
{
    public float EMA { get; set; }

    #region Amazing Data

    public TrendTypes EMASignal { get; set; }
    public TrendTypes BreakThrough { get; set; }
    public Pivot Pivot { get; set; }

    #endregion

    public EMACandleStick(DateTime dateTime, float open, float close, float high, float low, int volume)
    : this(new FinanceCandleStick(dateTime, open, close, high, low, volume), 0)
    {
    }



    public EMACandleStick(FinanceCandleStick candlestick, float ema) : base(candlestick)
    {
        EMA = ema;
        EMASignal = TrendTypes.NoChange;
        BreakThrough = TrendTypes.NoChange;
        Pivot = Pivot.Unchanged;
    }
}
