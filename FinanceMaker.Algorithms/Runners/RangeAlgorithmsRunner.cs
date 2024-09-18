using FinanceMaker.Common;
using FinanceMaker.Common.Resolvers.Abstracts;

namespace FinanceMaker.Algorithms;

public class RangeAlgorithmsRunner : ResolverBase<IAlgorithmRunner<RangeAlgorithmInput, object>, RangeAlgorithmInput>
{
    public RangeAlgorithmsRunner(IEnumerable<IAlgorithmRunner<RangeAlgorithmInput, object>> logicsToResolve) : base(logicsToResolve)
    {
    }

}
