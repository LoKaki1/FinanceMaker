using System;
using QuantConnect;

namespace FinanceMaker.Common.Models.Finance;

public class KeyLevelCandleStick : FinanceCandleStick
{
    public decimal KeyLevel { get; set; }
    public KeyLevelCandleStick(FinanceCandleStick candleStick, decimal keyLevel) : base(candleStick)
    {
        KeyLevel = keyLevel;
    }

    public KeyLevelCandleStick(Candlestick candlestick, decimal keyLevel) : base(candlestick)
    {
        KeyLevel = keyLevel;
    }

    public KeyLevelCandleStick(DateTime dateTime, float open, float close, float high, float low, float volume, decimal keyLevel) : base(dateTime, open, close, high, low, volume)
    {
        KeyLevel = keyLevel;
    }

    public KeyLevelCandleStick(DateTime dateTime, decimal open, decimal close, decimal high, decimal low, decimal volume, decimal keyLevel) : base(dateTime, open, close, high, low, volume)
    {
        KeyLevel = keyLevel;
    }
}
