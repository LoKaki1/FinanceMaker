namespace FinanceMaker.Common.Models.Pullers
{
    public class NewsPullerParameters
	{
		public string Ticker { get; set; }
		public DateTime From { get; set; }
		public DateTime To { get; set; }

		public static NewsPullerParameters GetTodayParams(string ticker)
		{
			return new NewsPullerParameters
			{
				Ticker = ticker,
				From = DateTime.Now,
				To = DateTime.Now
			};
		}
	}
}

