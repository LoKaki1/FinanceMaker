namespace FinanceMaker.Common.Models.Pullers
{
    public record TickersPullerParameters
	{
		public double MinPrice { get; set; }
		public double MaxPrice { get; set; }
		public int Volume { get; set; }
	
		public float PresentageOfChange { get; set; }
	}
}

