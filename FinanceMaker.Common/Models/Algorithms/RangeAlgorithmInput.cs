using FinanceMaker.Common.Models.Pullers.Enums;

namespace FinanceMaker.Common;

public record RangeAlgorithmInput : PricesPullerParameters
{
    public Algorithm Algorithm {get; set;}
    public RangeAlgorithmInput(string ticker, DateTime startTime, DateTime endTime, Period period, Algorithm algorithm) : base(ticker, startTime, endTime, period)
    {
        Algorithm = algorithm;
    }
}
