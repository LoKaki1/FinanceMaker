namespace FinanceMaker.Common.Models.Tickers
{
    public record TickerNews
	{
		public Ticker Ticker { get; set; }
		public IEnumerable<string> NewsUrl { get; set; }
	}
}

