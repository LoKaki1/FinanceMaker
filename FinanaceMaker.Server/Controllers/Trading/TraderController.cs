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
        public Task<IEnumerable<ITrade>> TradeOvernight(CancellationToken cancellationToken)
        {
            return ShouldMoveToAnotherClass(cancellationToken);
        }
        [HttpGet]
        public bool TradeOvernightForHours(CancellationToken cancellationToken)
        {
            var timer = new System.Timers.Timer(TimeSpan.FromHours(1));

            timer.Elapsed += async (sender, e) =>
            {
                var now = DateTime.Now;
                if (now.TimeOfDay.Hours >= 11 && now.TimeOfDay.Hours <= 23)
                {
                    await ShouldMoveToAnotherClass(cancellationToken);

                }
            };

            timer.Start();

            return true;
        }
        private async Task<IEnumerable<ITrade>> ShouldMoveToAnotherClass(CancellationToken cancellationToken)
        {
            TechnicalIDeaInput input = new TechnicalIDeaInput()
            {
                TechnicalParams = new TickersPullerParameters
                {
                    MinPrice = 5,
                    MaxPrice = 40,
                    MaxAvarageVolume = 1_000_000_000,
                    MinAvarageVolume = 1_000_000,
                    MinVolume = 3_000_000,
                    MaxVolume = 3_000_000_000,
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
                    MinVolume = 3_000_000,
                    MaxVolume = 3_000_000_000,
                    MinPresentageOfChange = -5,
                    MaxPresentageOfChange = -40
                }
            };
            var result2 = await m_Idea.CreateIdea(input, cancellationToken);

            result.AddRange(result2);

            var tradesResult = new List<ITrade>();
            var position = await m_Trader.GetClientPosition(cancellationToken);
            var openedPositoins = position.OpenedPositions;
            var moneyForEachTrade = position.BuyingPower / result.Count;
            var actualResult = result.Where(_ => openedPositoins.Contains(_.Ticker));
            foreach (var idea in actualResult)
            {
                if (idea is EntryExitOutputIdea entryExitOutputIdea)
                {
                    entryExitOutputIdea.Quantity = (int)(moneyForEachTrade / entryExitOutputIdea.Entry);
                }
                var trade = await m_Trader.Trade(idea, cancellationToken);

                tradesResult.Add(trade);
            }
            return tradesResult;
        }
    }
}
