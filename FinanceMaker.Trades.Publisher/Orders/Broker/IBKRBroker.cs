using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Alpaca.Markets;
using FinanceMaker.Common.Models.Ideas.Enums;
using FinanceMaker.Common.Models.Ideas.IdeaOutputs;
using FinanceMaker.Common.Models.Interactive;
using FinanceMaker.Common.Models.Trades.Enums;
using FinanceMaker.Common.Models.Trades.Trader;
using FinanceMaker.Publisher.Orders.Trader.Abstracts;
using FinanceMaker.Publisher.Orders.Trader.Interfaces;
using FinanceMaker.Publisher.Orders.Trades;
using IBApi;
using ITrade = FinanceMaker.Trades.Publisher.Orders.Trades.Interfaces.ITrade;

namespace FinanceMaker.Publisher.Orders.Broker;

public class IBKRBroker : BrokerrBase<EntryExitOutputIdea>
{
    private readonly IBKRClient m_IbkrClient;

    public override TraderType Type => TraderType.EntryExit | TraderType.StopLoss;

    public IBKRBroker(IBKRClient ibkrClient)
    {
        m_IbkrClient = ibkrClient;
        m_IbkrClient.Connect("127.0.0.1", 4002, 0);
    }

    protected override async Task<ITrade> TradeInternal(EntryExitOutputIdea idea, CancellationToken cancellationToken)
    {
        var contract = new Contract
        {
            Symbol = idea.Ticker,
            SecType = "STK",
            Exchange = "SMART",
            Currency = "USD"
        };

        var entryOrder = new Order
        {
            Action = idea.Trade == IdeaTradeType.Long ? "BUY" : "SELL",
            OrderType = "MKT",
            TotalQuantity = idea.Quantity,
            Tif = "GTC"
        };

        var takeProfitOrder = new Order
        {
            Action = idea.Trade == IdeaTradeType.Long ? "SELL" : "BUY",
            OrderType = "LMT",
            TotalQuantity = idea.Quantity,
            LmtPrice = Math.Round(idea.Exit, 2),
            Tif = "GTC"
        };

        var stopLossOrder = new Order
        {
            Action = idea.Trade == IdeaTradeType.Long ? "SELL" : "BUY",
            OrderType = "STP",
            TotalQuantity = idea.Quantity,
            AuxPrice = Math.Round(idea.Stoploss, 2),
            Tif = "GTC"
        };
        int id = m_IbkrClient.GetNextOrderId();

        m_IbkrClient.PlaceBracketOrder(id, contract, entryOrder, takeProfitOrder, stopLossOrder);
        await Task.Delay(5_000, cancellationToken); // Wait for the orders to be placed

        return new Trade(idea, Guid.NewGuid(), true);
    }

    public override async Task<Position> GetClientPosition(CancellationToken cancellationToken)
    {
        m_IbkrClient.RequestAccountSummary();
        m_IbkrClient.RequestCurrentPositions();
        m_IbkrClient.RequestOpenOrders();
        await Task.Delay(10_000, cancellationToken); // Wait for the data to be populated

        var positions = m_IbkrClient.GetCurrentPositions();
        var buyingPower = m_IbkrClient.GetBuyingPower();
        var openOrders = m_IbkrClient.GetOpenOrders();

        return new Position
        {
            BuyingPower = (float)buyingPower,
            OpenedPositions = positions.Select(p => p.Symbol).ToArray(),
            Orders = openOrders.Select(o => o.Symbol).ToArray()
        };
    }

    public override async Task CancelTrade(ITrade trade, CancellationToken cancellationToken)
    {
        // Use the m_IbkrClient directly instead of HTTP requests
        // Implement cancellation using IBKRClient functionality
        await trade.Cancel(cancellationToken);
    }
}
