using FinanceMaker.Common.Models.Ideas.IdeaInputs;
using FinanceMaker.Common.Models.Ideas.IdeaOutputs;
using FinanceMaker.Common.Models.Pullers;
using FinanceMaker.Ideas.Ideas;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuantConnect.Interfaces;

namespace FinanaceMaker.Server.Controllers.Ideas
{
    [Route("api/[controller]")]
    [ApiController]
    public class OverNightBreakoutController : ControllerBase
    {

        private readonly OverNightBreakout m_Idea;

        public OverNightBreakoutController(OverNightBreakout idea)
        {
            m_Idea = idea;
        }


        [HttpGet]
        public async Task<IEnumerable<EntryExitOutputIdea>> Get(CancellationToken token)
        {
            TechnicalIDeaInput input = new TechnicalIDeaInput()
            {
                TechnicalParams = new TickersPullerParameters
                {
                    MinPrice = 5,
                    MaxPrice = 40,
                    MaxAvarageVolume = 1_000_000_000,
                    MinAvarageVolume = 1_000_000,
                    MinPresentageOfChange = 10,
                    MaxPresentageOfChange = 30
                }
            };


            IEnumerable<EntryExitOutputIdea> result = (IEnumerable<EntryExitOutputIdea>)await m_Idea.CreateIdea(input, token);

            return result;
        }
    }
}