using System;
using FinanceMaker.Common.Models.Ideas.IdeaOutputs;
using FinanceMaker.Common.Models.Trades.Enums;
using FinanceMaker.Common.Models.Trades.Trader;
using FinanceMaker.Publisher.Orders.Trader.Interfaces;
using FinanceMaker.Publisher.Orders.Trades;
using FinanceMaker.Trades.Publisher.Orders.Trades.Interfaces;

namespace FinanceMaker.Publisher.Orders.Trader.Abstracts;

public abstract class TraderBase<T> : ITrader
    where T : GeneralOutputIdea
{
    public abstract TraderType Type { get; }
    public async Task<ITrade> Trade(GeneralOutputIdea idea, CancellationToken cancellationToken)
    {

        if (idea is not T realIdea)
        {
            throw new ArgumentException($"Trader got {idea.GetType()} but need {typeof(T)}");
        }

        try
        {
            return await TradeInternal(realIdea, cancellationToken);
        }
        catch (Exception ex)
        {
            idea.Description += $"\n{ex.Message}";
            return new Trade(idea, Guid.NewGuid(), true);
        }
    }

    protected abstract Task<ITrade> TradeInternal(T idea, CancellationToken cancellationToken);
    public abstract Task<Position> GetClientPosition(CancellationToken cancellationToken);
}
