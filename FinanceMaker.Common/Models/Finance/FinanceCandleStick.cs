using CloneExtensions;
using FinanceMaker.Common.Models.Finance.Enums;
using QuantConnect;
using System.Text.Json.Serialization;

namespace FinanceMaker.Common.Models.Finance
{
    public class FinanceCandleStick
    {
        #region Boring Data

        public DateTime Time => Candlestick?.Time ?? DateTime.MaxValue;
        public float Open => (float?)(Candlestick?.Open) ?? 0f;
        public float Close => (float?)(Candlestick?.Close) ?? 0f;
        public float High => (float?)Candlestick?.High ?? 0;
        public float Low => (float?)Candlestick?.Low ?? 0;
        public int Volume { get; set; }

        #endregion



        #region Why Do I do stuff Data

        [JsonIgnore]
        public Candlestick Candlestick { get; set; }

        #endregion

        public FinanceCandleStick(
            DateTime dateTime,
            float open,
            float close,
            float high,
            float low,
            int volume)
        {
            Candlestick = new Candlestick(
                dateTime,
                Convert.ToDecimal(open),
                Convert.ToDecimal(high),
                Convert.ToDecimal(low),
                Convert.ToDecimal(close));
            Volume = volume;
            // EMASignal = TrendTypes.NoChange;
            // BreakThrough = TrendTypes.NoChange;
            // Pivot = Pivot.Unchanged;
        }

        public FinanceCandleStick(
            DateTime dateTime,
            decimal open,
            decimal close,
            decimal high,
            decimal low,
            int volume)
        {
            Candlestick = new Candlestick(dateTime, open, high, low, close);
            Volume = volume;
        }
        public FinanceCandleStick(
            FinanceCandleStick candleStick)
        {
            Candlestick = candleStick.Candlestick.GetClone();
            Volume = candleStick.Volume;
        }

        public FinanceCandleStick(Candlestick candlestick)
        {
            Candlestick = candlestick;
        }
    }
}

