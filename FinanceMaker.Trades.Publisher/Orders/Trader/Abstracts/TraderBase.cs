using System;
using Alpaca.Markets;
using FinanceMaker.Common.Models.Ideas.IdeaOutputs;
using FinanceMaker.Common.Models.Trades.Enums;
using FinanceMaker.Publisher.Orders.Trader.Interfaces;

namespace FinanceMaker.Publisher.Orders.Trader.Abstracts;

public abstract class TraderBase<T> : ITrader
    where T : GeneralOutputIdea
{
    public abstract TraderType Type { get; }
    public Task<ITrade> Trade(GeneralOutputIdea idea, CancellationToken cancellationToken)
    {
        if (idea is not T realIdea)
        {
            throw new ArgumentException($"Trader got {idea.GetType()} but need {typeof(T)}");
        }

        return TradeInternal(realIdea, cancellationToken);
    }

    protected abstract Task<ITrade> TradeInternal(T idea, CancellationToken cancellationToken);
}
