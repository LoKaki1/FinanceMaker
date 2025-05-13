using System;
using System.Collections.Concurrent;
using System.Linq;
using FinanceMaker.Algorithms;
using FinanceMaker.Common;
using FinanceMaker.Common.Extensions;
using FinanceMaker.Common.Models.Finance;
using FinanceMaker.Common.Models.Finance.Enums;
using FinanceMaker.Common.Models.Ideas.IdeaOutputs;
using FinanceMaker.Common.Models.Pullers;
using FinanceMaker.Publisher.Orders.Trader.Interfaces;
using FinanceMaker.Publisher.Traders.Interfaces;
using FinanceMaker.Pullers.PricesPullers;
using FinanceMaker.Pullers.PricesPullers.Interfaces;
using FinanceMaker.Pullers.TickerPullers;
using QuantConnect.Indicators;

namespace FinanceMaker.Publisher.Traders;

/// <summary>
/// No need to do dynamic for now,
/// This trader would trade based on the range algorithm, which means if the current price is one precent around
/// of a "KeyLevel" price and the pivot is `Pivot.Low` then we should buy, and if the pivot is `Pivot.High` then we should short it.
/// And we should we should sell the stock with a risk of 3:2 risk reward ratio.
/// </summary>
public class QCTrader : ITrader
{
    private readonly MainTickersPuller m_TickersPullers;
    private readonly RangeAlgorithmsRunner m_RangeAlgorithmsRunner;
    private readonly IPricesPuller m_PricesPuller;
    private readonly IBroker m_Broker;
    private const int NUMBER_OF_OPEN_TRADES = 10;
    private const int STARTED_MONEY = 50_000;
    public QCTrader(MainTickersPuller pricesPuller,
                    RangeAlgorithmsRunner rangeAlgorithmsRunner,
                    IPricesPuller mainPricesPuller,
                    IBroker broker)
    {
        m_TickersPullers = pricesPuller;
        m_RangeAlgorithmsRunner = rangeAlgorithmsRunner;
        m_Broker = broker;
        m_PricesPuller = mainPricesPuller;
    }

    public async Task Trade(CancellationToken cancellationToken)
    {
        var currentPosion = await m_Broker.GetClientPosition(cancellationToken);

        if (currentPosion.BuyingPower < STARTED_MONEY / NUMBER_OF_OPEN_TRADES) return;

        var tickersToTrade = await GetRelevantTickers(cancellationToken);
        tickersToTrade = tickersToTrade.Where(_ => !currentPosion.OpenedPositions.Contains(_.ticker) && !currentPosion.Orders.Contains(_.ticker))
                                       .Take(NUMBER_OF_OPEN_TRADES)
                                       .ToArray();

        var moneyForEachTrade = currentPosion.BuyingPower * 0.1f;

        if (moneyForEachTrade < STARTED_MONEY / NUMBER_OF_OPEN_TRADES) return;

        foreach (var tickerPrice in tickersToTrade)
        {
            var entryPrice = tickerPrice.price;
            var quntity = (int)(moneyForEachTrade / entryPrice);
            var ticker = tickerPrice.ticker;

            if (quntity == 0) continue;

            var stopLoss = entryPrice * 0.985f;
            var takeProfit = entryPrice * 1.015f;
            var description = $"Entry price: {entryPrice}, Stop loss: {stopLoss}, Take profit: {takeProfit}";
            var order = new EntryExitOutputIdea(description, ticker, entryPrice, takeProfit, stopLoss, quntity);

            var trade = await m_Broker.BrokerTrade(order, cancellationToken);

        }
    }

    private async Task<IEnumerable<(string ticker, float price)>> GetRelevantTickers(CancellationToken cancellationToken)
    {
        var longTickers = TickersPullerParameters.BestBuyer;
        var shortTickers = TickersPullerParameters.BestBuyer;
        // For now only long tickers, I will implement the function of short but I don't want to
        // scanTickersTwice
        // var shortTickers = TickersPullerParameters.BestSellers;
        List<string> tickers = ["AAPL", "MSFT", "GOOGL", "AMZN", "TSLA", "META", "NVDA", "NFLX", "ADBE", "ORCL", "INTC", "AMD", "CRM", "PYPL", "CSCO", "QCOM", "AVGO", "TXN", "IBM", "SHOP", "NIO"];
        //var moreTicker = await m_TickersPullers.ScanTickers(shortTickers, cancellationToken);
        //tickers = tickers.Concat(moreTicker)
        //                 .Distinct()
        //                 .ToArray();
        //        string[] tickers = ["NIO", "BABA", "AAPL", "TSLA", "MSFT", "AMZN", "GOOGL", "FB", "NVDA", "AMD", "GME", "AMC", "BBBY", "SPCE", "NKLA", "PLTR", "RKT", "FUBO", "QS", "RIOT", "NIO", "BABA", "AAPL", "TSLA", "MSFT", "AMZN", "GOOGL", "FB", "NVDA", "AMD",
        //"GME", "AMC", "BBBY", "SPCE", "NKLA", "PLTR", "RKT", "FUBO", "QS", "RIOT",
        //"COIN", "MARA", "LCID", "SOFI", "HOOD", "AI", "UPST", "AFRM", "DNA", "PATH",
        //"RBLX", "SNAP", "PTON", "TWLO", "CRWD", "ZM", "DKNG", "CHWY", "TTD", "RUN",
        //"ENPH", "MSTR", "CVNA", "DASH", "PINS", "NET", "SHOP", "SQ", "PYPL"];
        tickers = tickers.Distinct().ToList();
        // Now we've got the stocks, we should analyze them
        var relevantTickers = new ConcurrentBag<(string ticker, float price)>();

        foreach (var ticker in tickers)
        {
            var range = await m_RangeAlgorithmsRunner.Run<EMACandleStick>(
                new RangeAlgorithmInput(new PricesPullerParameters(
                    ticker,
                    DateTime.Now.AddYears(-2),
                    DateTime.Now,
                    Common.Models.Pullers.Enums.Period.Daily), Algorithm.KeyLevels), cancellationToken);

            if (range is not KeyLevelCandleSticks candleSticks || !candleSticks.Any()) continue;

            var interdayCandles = await m_RangeAlgorithmsRunner.Run<EMACandleStick>(
                new RangeAlgorithmInput(PricesPullerParameters.GetTodayParams(ticker), Algorithm.KeyLevels),
                                                                                    cancellationToken);

            if (interdayCandles is not KeyLevelCandleSticks interdayCandleSticks || !interdayCandleSticks.Any()) continue;

            foreach (var keylevel in candleSticks.KeyLevels)
            {
                var lastCandleStick = interdayCandleSticks.Last();
                var averageValue = interdayCandleSticks.TakeLast(5)
                                                       .Average(candle => candle.Close);

                var valueDivision = Math.Abs(lastCandleStick.Close) / keylevel;
                // I am not sure the pivot will work, but the average value key level should work instead
                // That should also solve the problem we have with the candles at the start
                if (valueDivision <= 1 && valueDivision >= 0.995)
                {
                    relevantTickers.Add((ticker, lastCandleStick.Close));
                    break;
                }
            }
        }

        return [.. relevantTickers];
    }
}
