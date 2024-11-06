using FinanceMaker.Algorithms;
using FinanceMaker.Common.Models.Ideas.IdeaInputs;
using FinanceMaker.Common.Models.Ideas.IdeaOutputs;
using FinanceMaker.Pullers.TickerPullers;
using FinanceMaker.Pullers.TickerPullers.Interfaces;

namespace FinanceMaker.Ideas.Ideas;

public class OverNightBreakout : KeyLevelsEntryExitOutputIdea<TechnicalIDeaInput, EntryExitOutputIdea>
{
    public OverNightBreakout(MainTickersPuller puller, RangeAlgorithmsRunner algoRunner) : base(puller, algoRunner)
    {
    }

    protected override Task<IEnumerable<EntryExitOutputIdea>> CreateIdea(TechnicalIDeaInput input, CancellationToken cancellationToken)
    {
        input.TechnicalParams.MaxPresentageOfChange = 34;
        input.TechnicalParams.MinPresentageOfChange = 4;


        return base.CreateIdea(input, cancellationToken);
    }
}
