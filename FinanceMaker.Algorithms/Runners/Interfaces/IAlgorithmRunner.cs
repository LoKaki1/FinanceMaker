using FinanceMaker.Common;
using FinanceMaker.Common.Resolvers.Interfaces;

namespace FinanceMaker.Algorithms;

// TODO: Change this arc to be like the ideas (you didn't code for a long time broooo)
public interface IAlgorithmRunner<TInput, TOutput>: IResolveable<TInput> where TInput : class
{
    AlgorithmType AlgorithmType{ get; }

    Task<IEnumerable<TOutput>> Run(TInput input, CancellationToken cancellationToken);
}
