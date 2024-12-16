using FinanceMaker.Common.Models.Pullers.Enums;

namespace FinanceMaker.Common;

public record PricesPullerParameters
{
    public string Ticker { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public Period Period { get; set; }

    public PricesPullerParameters(string ticker, DateTime startTime, DateTime endTime, Period period)
    {
        Ticker = ticker;
        StartTime = startTime;
        EndTime = endTime;
        Period = period;
    }

    public PricesPullerParameters()
    {
    }
}
