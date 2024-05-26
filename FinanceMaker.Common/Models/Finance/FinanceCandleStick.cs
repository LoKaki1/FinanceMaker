using QuantConnect;

namespace FinanceMaker.Common.Models.Finance
{
	public sealed class FinanceCandleStick
	{
		public DateTime Time => Candlestick?.Time ?? DateTime.MaxValue;
		public decimal Open => Candlestick?.Open ?? 0;
		public decimal Close => Candlestick?.Close ?? 0;
		public decimal High => Candlestick?.High ?? 0;
		public decimal Low => Candlestick?.Low ?? 0;

		public decimal Volume { get; set; }
		public Candlestick Candlestick { get; set; }

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

