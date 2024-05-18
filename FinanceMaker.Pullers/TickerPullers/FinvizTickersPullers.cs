
using FinanceMaker.Common.Models;
using FinanceMaker.Common.Models.Pullers;
using FinanceMaker.Pullers.TickerPullers.Interfaces;

namespace FinanceMaker.Pullers.TickerPullers
{
	public sealed class Finviz: IParamtizedTickersPuller
	{
        private readonly IHttpClientFactory m_RequestService;

        public Finviz(IHttpClientFactory requestService)
        {
            m_RequestService = requestService;
        }

        public Task<IEnumerable<Ticker>> ScanTickers(TickersPullerParameters scannerParams)
        {
            throw new NotImplementedException();
        }
    }
}

