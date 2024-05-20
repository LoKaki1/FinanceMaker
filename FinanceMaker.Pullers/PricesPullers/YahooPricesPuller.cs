using System;
using FinanceMaker.Common.Models.Tickers;
using FinanceMaker.Pullers.PricesPullers.Interfaces;

namespace FinanceMaker.Pullers.PricesPullers
{
	public class YahooPricesPuller: IPricesPuller
	{

		public YahooPricesPuller()
		{
		}

        public Task<TickerChart> GetTickerPrices(string ticker)
        {
           YahooFinanceApi.Yahoo.GetHistoricalAsync() 
        }
    }
}

