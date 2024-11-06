// using System;
// using FinanceMaker.Common.Models.Finance.Enums;
// using QuantConnect;

namespace FinanceMaker.Common.Models.Finance;

public class KeyLevelCandleSticks : List<EMACandleStick>
{
    public float[] KeyLevels { get; set; }

    public KeyLevelCandleSticks(IEnumerable<EMACandleStick> eMACandleSticks,
                                IEnumerable<float> keyLevels)
        : base(eMACandleSticks)
    {
        KeyLevels = keyLevels.ToArray();
    }
}
// // public class KeyLevelCandleStick : FinanceCandleStick
// {
//     public decimal KeyLevel { get; set; }
// public Pivot Pivot { get; set; }
// public KeyLevelCandleStick(FinanceCandleStick candleStick, decimal keyLevel, Pivot pivot) : base(candleStick)
// {
//     KeyLevel = keyLevel;
//     Pivot = pivot;
// }

// public KeyLevelCandleStick(Candlestick candlestick, decimal keyLevel, Pivot pivot) : base(candlestick)
// {
//     KeyLevel = keyLevel;
//     Pivot = pivot;
// }

// public KeyLevelCandleStick(DateTime dateTime, float open, float close, float high, float low, float volume, decimal keyLevel, Pivot pivot) : base(dateTime, open, close, high, low, volume)
// {
//     KeyLevel = keyLevel;
//     Pivot = pivot;
// }

// public KeyLevelCandleStick(DateTime dateTime, decimal open, decimal close, decimal high, decimal low, decimal volume, decimal keyLevel, Pivot pivot) : base(dateTime, open, close, high, low, volume)
// {
//     KeyLevel = keyLevel;
//     Pivot = pivot;
// }
// }
