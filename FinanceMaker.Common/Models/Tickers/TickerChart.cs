using FinanceMaker.Common.Models.Finance;

namespace FinanceMaker.Common.Models.Tickers
{
    public record TickerChart
	{
		public string Ticker { get; set; }
		public IEnumerable<FinanceCandleStick> Price { get; set; } 
		public IDictionary<string, object> ExtraData { get; set; }
	}
}

