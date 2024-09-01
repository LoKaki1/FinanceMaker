using FinanceMaker.Common;
using FinanceMaker.Common.Resolvers.Interfaces;

namespace FinanceMaker.Algorithms;

public interface IAlgorithmRunner<TInput, TOutput>: IResolveable<TInput> where TInput : class
{
    AlgorithmType AlgorithmType{ get; }

    Task<IEnumerable<TOutput>> Run(TInput input, CancellationToken cancellationToken);
}
