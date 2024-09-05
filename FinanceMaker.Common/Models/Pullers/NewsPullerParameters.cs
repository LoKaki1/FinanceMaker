namespace FinanceMaker.Common.Models.Pullers
{
	#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
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
	#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
}

