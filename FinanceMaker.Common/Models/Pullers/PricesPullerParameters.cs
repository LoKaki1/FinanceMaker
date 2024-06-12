using FinanceMaker.Common.Models.Pullers.Enums;

namespace FinanceMaker.Common;

public record PricesPullerParameters
{
    public string Ticker { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public Period Period { get; set; }
}
