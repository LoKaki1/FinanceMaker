using FinanceMaker.Common.Models.Finance.Enums;
using QuantConnect;
using System.Text.Json.Serialization;

namespace FinanceMaker.Common.Models.Finance
{
    public sealed class FinanceCandleStick
	{
		#region Boring Data

		public DateTime Time => Candlestick?.Time ?? DateTime.MaxValue;
		public decimal Open => Candlestick?.Open ?? 0;
		public decimal Close => Candlestick?.Close ?? 0;
		public decimal High => Candlestick?.High ?? 0;
		public decimal Low => Candlestick?.Low ?? 0;
		public decimal Volume { get; set; }

		#endregion

		#region Amazing Data

		public decimal EMA { get; set; }
		public TrendTypes EMASignal { get; set; }
		public TrendTypes BreakThrough { get; set; } 
		public Pivot Pivot { get; set; }

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
			float volume)
        {
            Candlestick = new Candlestick(
                dateTime,
                Convert.ToDecimal(open),
                Convert.ToDecimal(high),
                Convert.ToDecimal(low),
                Convert.ToDecimal(close));
            Volume = Convert.ToDecimal(volume);
			EMASignal = TrendTypes.NoChange;
			BreakThrough = TrendTypes.NoChange;
            EMA = 0;
			Pivot = Pivot.Unchanged;
        }

        public FinanceCandleStick(
            DateTime dateTime,
            decimal open,
            decimal close,
            decimal high,
            decimal low,
			decimal volume)
		{
			Candlestick = new Candlestick(dateTime, open, high, low, close);
			Volume = volume; 
		}


		public FinanceCandleStick(Candlestick candlestick)
		{
			Candlestick = candlestick;
		}
	}
}

