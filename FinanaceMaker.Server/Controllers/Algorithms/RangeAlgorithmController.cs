using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinanceMaker.Algorithms;
using FinanceMaker.Algorithms.Chart;
using FinanceMaker.Common;
using FinanceMaker.Common.Models.Finance;
using FinanceMaker.Common.Models.Pullers.Enums;
using FinanceMaker.Common.Models.Tickers;
using FinanceMaker.Pullers.PricesPullers;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FinanaceMaker.Server.Controllers.Algorithms
{
    [Route("api/[controller]")]
    public class RangeAlgorithmController : Controller
    {
        private readonly RangeAlgorithmsRunner m_AlgorithmRunner;
        public RangeAlgorithmController(RangeAlgorithmsRunner algorithmRunner)
        {
            m_AlgorithmRunner = algorithmRunner;
        }

        // GET: api/values
        [HttpPost]
        public Task<IEnumerable<FinanceCandleStick>> Post([FromBody] RangeAlgorithmInput algorithmInput,
                                                          CancellationToken cancellationToken)
        {
            // This may cause some problems

            return m_AlgorithmRunner.Run<FinanceCandleStick>(algorithmInput, cancellationToken);
        }
    }
}

