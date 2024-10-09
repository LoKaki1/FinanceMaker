using System;
using FinanceMaker.Algorithms.News.Analyziers.Interfaces;
using FinanceMaker.Common.Models.Algorithms.Analyzers;
using FinanceMaker.Common.Models.Algorithms.Analyzers.Input;
using FinanceMaker.Common.Models.Pullers;
using FinanceMaker.Pullers.NewsPullers.Interfaces;

namespace FinanceMaker.Algorithms.News.Analyziers.Abstracts;

public abstract class NewsAnalyzerBase<TInput, TOutput> : INewsAnalyzer
    where TInput : NewsAnalyzerInput
    where TOutput : NewsAnalyzed
{
    protected readonly INewsPuller m_Puller;

    public NewsAnalyzerBase(INewsPuller puller)
    {
        m_Puller = puller;
    }

    public async Task<NewsAnalyzed> AnalyzeNews(NewsPullerParameters newsAnalyzerInput,
                                                CancellationToken cancellationToken)
    {

        var newsAnalyzed = await AnalyzeNews(input, cancellationToken);

        return newsAnalyzed;
    }
    protected abstract Task<TOutput> AnalyzeNews(TInput input, CancellationToken cancellationToken);
    protected virtual TInput Parse(NewsPullerParameters newsAnalyzerInput, IEnumerable<string> urls)
    {
        if (typeof(TInput) == typeof(NewsAnalyzerInput))
        {
            var input = new NewsAnalyzerInput(newsAnalyzerInput, urls)
        }
    }
}
