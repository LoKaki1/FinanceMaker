using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinanceMaker.Algorithms.Chart;
using FinanceMaker.Common;
using FinanceMaker.Common.Models.Pullers.Enums;
using FinanceMaker.Common.Models.Tickers;
using FinanceMaker.Pullers.PricesPullers;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FinanaceMaker.Server.Controllers
{
    [Route("api/[controller]")]
    public class AlgorithmController : Controller
    {
        private readonly MainPricesPuller m_PricesPuller;

        public AlgorithmController(MainPricesPuller pricesPuller)
        {
            m_PricesPuller = pricesPuller;
        }

        // GET: api/values
        [HttpGet]
        public async Task<IEnumerable<double>> SupportAndResistance([FromQuery] string ticker,
                                                          [FromQuery] DateTime start,
                                                          [FromQuery] DateTime end,
                                                          [FromQuery] Period period,
                                                          CancellationToken cancellationToken)
        {
            var parameters = new PricesPullerParameters(ticker, start, end, period);

            var prices = await m_PricesPuller.GetTickerPrices(parameters, cancellationToken);
            var levels = SupportAndResistanceLevels.GetSupportResistanceLevels(new TickerChart(ticker, prices));

            return levels;
        }
    }
}

