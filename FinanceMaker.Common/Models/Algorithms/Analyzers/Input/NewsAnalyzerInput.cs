using FinanceMaker.Common.Models.Pullers;
using System.Collections.Generic;

namespace FinanceMaker.Common.Models.Algorithms.Analyzers.Input;

public class NewsAnalyzerInput : NewsPullerParameters
{
    public IEnumerable<string> Urls { get; set; }

    public NewsAnalyzerInput(string ticker, DateTime from, DateTime to, IEnumerable<string> urls)
    : base(ticker, from, to)
    {
        Urls = urls;
    }

    public NewsAnalyzerInput(NewsPullerParameters puller, IEnumerable<string> urls)
        : this(puller.Ticker, puller.From, puller.To, urls)
    { }
}
