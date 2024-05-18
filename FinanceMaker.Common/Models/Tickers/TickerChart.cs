namespace FinanceMaker.Common.Models.Tickers
{
    public record TickerChart
	{
		public string Ticker { get; set; }
		public IEnumerable<double> Price { get; set; } // Todo: Change to IEnumable<Candle>
		public IDictionary<string, object> ExtraData { get; set; }
	}
}

