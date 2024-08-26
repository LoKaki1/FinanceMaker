using FinanceMaker.Common;

namespace FinanceMaker.Algorithms;

public interface IAlgorithmRunner<TInput, TOutput> where TInput : class
{
    AlgorithmType Algorithm { get; }

    Task<IEnumerable<TOutput>> Run(TInput input, CancellationToken cancellationToken);
}
