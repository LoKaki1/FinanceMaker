using System;
using Accord;
using Alpaca.Markets;
using FinanceMaker.Common.Models.Ideas.IdeaOutputs;
using FinanceMaker.Common.Models.Trades.Enums;
using FinanceMaker.Common.Models.Trades.Trader;
using FinanceMaker.Publisher.Extensions;
using FinanceMaker.Publisher.Orders.Trader.Abstracts;
using FinanceMaker.Publisher.Orders.Trades;
using ITrade = FinanceMaker.Trades.Publisher.Orders.Trades.Interfaces.ITrade;

namespace FinanceMaker.Publisher.Orders.Trader;

public class AlpacaTrader : TraderBase<EntryExitOutputIdea>
{
    // I really need to create both secrets and configs 
    const string API_KEY = "PKP2DNI64YYAGWF8KJS3";
    const string API_SECRET = "jTe3fzyj6bDEhpb7h7Rfq7qE6HxmLcss9ONuNQnR";
    const string ENDPOIONT_URL = "https://paper-api.alpaca.markets/v2";

    private readonly IAlpacaTradingClient m_Client;

    public override TraderType Type => TraderType.EntryExit | TraderType.StopLoss;

    public AlpacaTrader()
    {
        m_Client = Environments.Paper
                .GetAlpacaTradingClient(new SecretKey(API_KEY, API_SECRET));
    }
    protected override async Task<ITrade> TradeInternal(EntryExitOutputIdea idea, CancellationToken cancellationToken)
    {
        var request = idea.ConvertToAlpacaRequest();

        var order = await m_Client.PostOrderAsync(request, cancellationToken);
        var trade = new Trade(idea, order.OrderId, true);

        if (cancellationToken.IsCancellationRequested)
        {
            await CancelTrade(trade, CancellationToken.None);
        }

        return trade;

    }

    private async Task CancelTrade(Trade trade, CancellationToken cancellationToken)
    {
        await trade.Cancel(cancellationToken);

        await m_Client.CancelOrderAsync(trade.TradeId, cancellationToken);
    }

    public override async Task<Position> GetClientPosition(CancellationToken cancellationToken)
    {
        var accountData = await m_Client.GetAccountAsync(cancellationToken);
        var accountPosition = await m_Client.ListPositionsAsync(cancellationToken);
        float buyingPower = accountData.BuyingPower is null ? 0f : (float)accountData.BuyingPower.Value;

        var poposition = new Position()
        {
            BuyingPower = buyingPower,
            OpenedPositions = accountPosition.Select(_ => _.Symbol).ToArray()
        };

        return poposition;
    }
}
