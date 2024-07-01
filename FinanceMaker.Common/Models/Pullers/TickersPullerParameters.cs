namespace FinanceMaker.Common.Models.Pullers
{
    public record TickersPullerParameters
	{
		public double MinPrice { get; set; }
		public double MaxPrice { get; set; }
		public int MinAvarageVolume { get; set; }
		public int MaxAvarageVolume { get; set; }
		public float MinPresentageOfChange { get; set; }
		public float MaxPresentageOfChange { get; set; }
	}
}

