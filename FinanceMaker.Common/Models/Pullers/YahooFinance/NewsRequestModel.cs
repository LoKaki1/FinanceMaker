namespace FinanceMaker.Common.Models.Pullers.YahooFinance
{
    public class NewsRequestModel
	{
		public Service serviceConfig { get; set; }

		public static NewsRequestModel CreateCloneToYahoo()
		{
			return new NewsRequestModel
			{
				serviceConfig = new Service
				{
					count = 40
				}
			};
		}
    }

	public class Service
	{
		public int count { get; set; }
	}
}

