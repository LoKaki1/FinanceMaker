using FinanceMaker.Common;
using FinanceMaker.Common.Models.Pullers.Enums;

namespace FinanceMaker.Algorithms;

public interface ITickerRangeAlgorithmRunner<T>: IAlgorithmRunner<PricesPullerParameters
{
    Task<IEnumerable<T>> RunAlgorithm(PricesPullerParameters pricesPullerParameters);
}
