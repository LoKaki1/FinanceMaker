namespace FinanceMaker.Common.Models
{
    public record Ticker
	{
		public string Name { get; set; }
		public IDictionary<string, object> ExtraData { get; set; }
	}
}

