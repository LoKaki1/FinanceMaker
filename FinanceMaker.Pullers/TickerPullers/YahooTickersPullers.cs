using System;
using FinanceMaker.Common.Models;
using FinanceMaker.Common.Models.Pullers;
using FinanceMaker.Pullers.TickerPullers.Interfaces;

namespace FinanceMaker.Pullers.TickerPullers
{
	public sealed class Yahoo: IParamtizedTickersPuller
	{
        private readonly IHttpClientFactory m_HttpClientFactory;

        public Yahoo(IHttpClientFactory httpClientFactory)
        {
            m_HttpClientFactory = httpClientFactory;
        }

        public Task<IEnumerable<Ticker>> ScanTickers(TickersPullerParameters scannerParams)
        {
            throw new NotImplementedException();
            // Here is more about news
        }
    }
}

