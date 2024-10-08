﻿using FinanceMaker.Algorithms.Chart;
using FinanceMaker.Common;
using FinanceMaker.Common.Models.Finance;
using FinanceMaker.Common.Models.Finance.Enums;
using FinanceMaker.Pullers.PricesPullers.Interfaces;

namespace FinanceMaker.Algorithms;

public class BreakOutDetectionRunner : TickerRangeAlgorithmRunnerBase<EMACandleStick>
{
    public override Algorithm Algorithm => Algorithm.BreakoutDetection;
    
    public BreakOutDetectionRunner(IPricesPuller pricesPuller) : base(pricesPuller)
    {

    }

    public override Task<IEnumerable<EMACandleStick>> Run(IEnumerable<FinanceCandleStick> input, CancellationToken cancellationToken)
    {
        var breakoutResult = BreakoutDetection.DetectBreakoutCandles(input, 30, 30, 10 );
        
        
        return Task.FromResult(breakoutResult);
    }
}
