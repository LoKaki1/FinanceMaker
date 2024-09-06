using System;
using FinanceMaker.Algorithms;
using FinanceMaker.Common;
using FinanceMaker.Common.Models.Ideas.Enums;
using FinanceMaker.Common.Models.Ideas.IdeaInputs;
using FinanceMaker.Common.Models.Ideas.IdeaOutputs;
using FinanceMaker.Common.Models.Pullers;
using FinanceMaker.Common.Models.Pullers.Enums;
using FinanceMaker.Ideas.Ideas.Abstracts;
using FinanceMaker.Pullers.TickerPullers.Interfaces;

namespace FinanceMaker.Ideas.Ideas;

public sealed class OvernightIdea : IdeaBase<TechnicalIDeaInput, GeneralOutputIdea>
{
    private readonly RangeAlgorithmsRunner m_AlgoRunner;
    private readonly IParamtizedTickersPuller m_Puller;
    private readonly Func<string, PricesPullerParameters> m_PricesPullerParams;

    public OvernightIdea(IParamtizedTickersPuller puller, RangeAlgorithmsRunner algoRunner)
    {
        m_Puller = puller;
        m_AlgoRunner = algoRunner;
        m_PricesPullerParams = (ticker) => new PricesPullerParameters(ticker,
                                                                      DateTime.Now.AddYears(-2),
                                                                      DateTime.Now,
                                                                      Period.Daily);
    }

    public override IdeaTypes Type => IdeaTypes.Overnight;

    protected override async Task<GeneralOutputIdea> CreateIdea(TechnicalIDeaInput input, CancellationToken cancellationToken)
    {
        //TODO: I think we should find a good query for finding those using Finviz API (for now it will stay 100% custom)
        var scannerParams = input.TechnicalParams;

        var relevantTickers = await m_Puller.ScanTickers(scannerParams, cancellationToken);

        // Now we want to run some algos on the relevant stocks 

        foreach (var ticker in relevantTickers)
        {
            var rangeParams = m_PricesPullerParams.Invoke(ticker);
            var algoInput = new RangeAlgorithmInput(rangeParams, Algorithm.KeyLevels);

            var keyLevelRunner = m_AlgoRunner.Resolve(algoInput);
            var keyLevels = await keyLevelRunner.Run(algoInput, cancellationToken);

            // Do some intersting logic (like this shit)
            // Now when I think about it lets let all the algos to return the candlestick (because now
            // I need to get the candlesticks data to continue this amazing logic,
            // For example: 
            // Now I wanna check if the last price is closed to one of the keylevels
            // There fore we need to warp the candlestick in a base class which contains them and each 
            // algorithm is gonna add its own data to the class and its dervitives 
        }
    }
}
