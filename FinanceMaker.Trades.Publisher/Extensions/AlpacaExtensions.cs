using System;
using Alpaca.Markets;
using FinanceMaker.Common.Models.Ideas.Enums;
using FinanceMaker.Common.Models.Ideas.IdeaOutputs;

namespace FinanceMaker.Publisher.Extensions;

public static class AlpacaExtensions
{
    public static NewOrderRequest ConvertToAlpacaRequest(this EntryExitOutputIdea idea, int quantity)
    {
        var orderSide = idea.Trade == IdeaTradeType.Long ? OrderSide.Buy : OrderSide.Sell;
        var request = new NewOrderRequest(idea.Ticker,
                                           quantity,
                                           orderSide,
                                           OrderType.Limit,
                                           TimeInForce.Gtc)
        {
            LimitPrice = (decimal)idea.Entry,
            TakeProfitLimitPrice = (decimal)idea.Exit,
            StopLossLimitPrice = (decimal)idea.Stoploss,
        };

        return request;
    }
}
