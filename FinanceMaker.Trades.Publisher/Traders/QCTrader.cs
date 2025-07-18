﻿using System;
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
    private const int NUMBER_OF_OPEN_TRADES = 3;
    private const int STARTED_MONEY = 1900;
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
        tickersToTrade = tickersToTrade
            .Select(ticker => (ticker.ticker, ticker.price))
            .OrderByDescending(ticker => ticker.price)
            .ToArray();
        tickersToTrade = tickersToTrade.Where(_ => !currentPosion.OpenedPositions.Contains(_.ticker) && !currentPosion.Orders.Contains(_.ticker))
                                       .Take(NUMBER_OF_OPEN_TRADES)
                                       .ToArray();
        var byingPower = currentPosion.BuyingPower;
        var moneyForEachTrade = byingPower * 0.5f;

        if (moneyForEachTrade < STARTED_MONEY / NUMBER_OF_OPEN_TRADES) return;

        foreach (var tickerPrice in tickersToTrade)
        {
            var entryPrice = tickerPrice.price;
            var quntity = (int)(moneyForEachTrade / entryPrice);
            var ticker = tickerPrice.ticker;

            if (quntity == 0) continue;

            var stopLoss = entryPrice * 0.985f;
            var takeProfit = entryPrice * 1.02f;
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
        List<string> tickers = ["TSLA", "NVDA", "NIO", "MARA", "RIOT", "AMD", "BABA", "BA", "LI", "ENPH", "PLTR", "HUT", "PLTR", "HUT"];

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
            if (interdayCandles is not KeyLevelCandleSticks interdayCandleSticks || !interdayCandleSticks.Any())
                continue;

            foreach (var keylevel in candleSticks.KeyLevels)
            {
                var lastCandleStick = interdayCandleSticks.Last();
                var recentCandles = interdayCandleSticks[^4..]; // last 4 candles
                var averageValue = recentCandles[..2].Average(candle => candle.Close);

                var valueDivision = Math.Abs(lastCandleStick.Close) / keylevel;

                bool nearKeyLevel = valueDivision <= 1.005 && valueDivision >= 0.995;
                var previousHistory = recentCandles;
                bool hasTentativePivot = true;

                if (previousHistory is not null && previousHistory.Any())
                {
                    var previousList = previousHistory.ToList();
                    for (int i = 0; i < previousList.Count - 1; i++)
                    {
                        if (previousList[i].Close > previousList[i + 1].Close)
                        {
                            hasTentativePivot &= true;
                        }
                        else
                        {
                            hasTentativePivot = false;
                            break;
                        }
                    }
                    if (hasTentativePivot && previousList.Count > 0)
                    {
                        hasTentativePivot &= previousList.Last().Close > lastCandleStick.Open;
                    }
                }

                if (nearKeyLevel && hasTentativePivot)
                {
                    relevantTickers.Add((ticker, lastCandleStick.Close));
                    break;
                }
            }

            //if (interdayCandles is not KeyLevelCandleSticks interdayCandleSticks || !interdayCandleSticks.Any()) continue;

            //foreach (var keylevel in candleSticks.KeyLevels)
            //{
            //    var lastCandleStick = interdayCandleSticks.Last();
            //    var averageValue = interdayCandleSticks[^4..^2]
            //                                           .Average(candle => candle.Close);

            //    var valueDivision = Math.Abs(lastCandleStick.Close) / keylevel;
            //    // I am not sure the pivot will work, but the average value key level should work instead
            //    // That should also solve the problem we have with the candles at the start
            //    if (valueDivision <= 1 && valueDivision >= 0.995)
            //    {
            //        relevantTickers.Add((ticker, lastCandleStick.Close));
            //        break;
            //    }
            //}
        }

        return [.. relevantTickers];
    }
}
