using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinanceMaker.Pullers.NewsPullers;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FinanaceMaker.Server.Controllers
{
    [Route("api/[controller]")]
    public class NewsController : Controller
    {
        private readonly MainNewsPuller m_NewsPuller;

        public NewsController(MainNewsPuller newsPuller)
        {
            m_NewsPuller = newsPuller;
        }

        [HttpGet]
        public Task<IEnumerable<string>> GetTickerNews([FromQuery] string ticker, CancellationToken cancellationToken)
        {
            return m_NewsPuller.PullNews(ticker, cancellationToken);
        }
    }
}

