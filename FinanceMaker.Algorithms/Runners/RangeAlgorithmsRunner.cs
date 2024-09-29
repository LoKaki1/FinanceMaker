using FinanceMaker.Common;
using FinanceMaker.Common.Models.Finance;
using FinanceMaker.Common.Resolvers.Abstracts;

namespace FinanceMaker.Algorithms;

public class RangeAlgorithmsRunner : ResolverBase<IAlgorithmRunner<RangeAlgorithmInput>, RangeAlgorithmInput>
{
    public RangeAlgorithmsRunner(IEnumerable<IAlgorithmRunner<RangeAlgorithmInput>> logicsToResolve) : base(logicsToResolve)
    {
    }

}
