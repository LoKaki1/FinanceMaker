using FinanceMaker.Common.Models.Pullers;
using FinanceMaker.Pullers.TickerPullers;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FinanaceMaker.Server.Controllers
{
    [Route("api/[controller]")]
    public class ScannerController : Controller
    {
        private readonly MainTickersPuller m_Scanner;

        public ScannerController(MainTickersPuller scanner)
        {
            m_Scanner = scanner;
        }

        // GET: api/values
        [HttpGet]
        public Task<IEnumerable<string>> Get(CancellationToken token)
        {
            return m_Scanner.ScanTickers(new TickersPullerParameters
            {
                MinAvarageVolume = 100_000,
                MaxAvarageVolume = 1_000_000,
                MaxPrice = 20,
                MinPrice = 3,
                PresentageOfChange = 20
            }, token);
        }
    }
}

