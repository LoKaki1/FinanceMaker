using FinanceMaker.Common.Models.Ideas.IdeaInputs;
using FinanceMaker.Common.Models.Ideas.IdeaOutputs;
using FinanceMaker.Common.Models.Pullers;
using FinanceMaker.Ideas.Ideas;
using FinanceMaker.Publisher.Orders.Trader.Interfaces;
using FinanceMaker.Trades.Publisher.Orders.Trades.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinanaceMaker.Server.Controllers.Trading
{
    [Route("api/[controller]")]
    [ApiController]
    public class TraderController : ControllerBase
    {
        private readonly OverNightBreakout m_Idea;
        private readonly ITrader m_Trader;
        public TraderController(OverNightBreakout ideal, ITrader trader)
        {
            m_Idea = ideal;
            m_Trader = trader;
        }

        [HttpGet]
        public async Task<IEnumerable<ITrade>> TradeOvernight(CancellationToken cancellationToken)
        {
            TechnicalIDeaInput input = new TechnicalIDeaInput()
            {
                TechnicalParams = new TickersPullerParameters
                {
                    MinPrice = 5,
                    MaxPrice = 40,
                    MaxAvarageVolume = 1_000_000_000,
                    MinAvarageVolume = 1_000_000,
                    MinPresentageOfChange = 5,
                    MaxPresentageOfChange = 120
                }
            };


            var result = (await m_Idea.CreateIdea(input, cancellationToken)).ToList();

            input = new TechnicalIDeaInput()
            {
                TechnicalParams = new TickersPullerParameters
                {
                    MinPrice = 5,
                    MaxPrice = 40,
                    MaxAvarageVolume = 1_000_000_000,
                    MinAvarageVolume = 1_000_000,
                    MinPresentageOfChange = -5,
                    MaxPresentageOfChange = -40
                }
            };
            var result2 = await m_Idea.CreateIdea(input, cancellationToken);

            result.AddRange(result2);

            var trades = new List<Task<ITrade>>();
            foreach (var idea in result)
            {
                var trade = m_Trader.Trade(idea, cancellationToken);

                trades.Add(trade);
            }

            var tradesResult = await Task.WhenAll(trades);

            return tradesResult;
        }
    }
}