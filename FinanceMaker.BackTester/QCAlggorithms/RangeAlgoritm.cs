﻿using FinanceMaker.Algorithms;
using FinanceMaker.BackTester.QCHelpers;
using FinanceMaker.Common;
using FinanceMaker.Common.Models.Finance;
using FinanceMaker.Common.Models.Finance.Enums;
using FinanceMaker.Common.Models.Ideas.IdeaInputs;
using FinanceMaker.Pullers.TickerPullers;
using Microsoft.Extensions.DependencyInjection;
using QuantConnect;
using QuantConnect.Algorithm;
using QuantConnect.Data;
using QuantConnect.Indicators;
using QuantConnect.Orders.Fees;

namespace FinanceMaker.BackTester.QCAlggorithms;

public class RangeAlgoritm : QCAlgorithm
{
    private Dictionary<string, float[]> m_TickerToKeyLevels = new();
    private Dictionary<string, RelativeStrengthIndex> m_RsiIndicators = new();

    public override void Initialize()
    {
        var startDate = DateTime.Now.AddDays(-3);
        var startDateForAlgo = new DateTime(2019, 1, 1);
        var endDate = DateTime.Now;
        var endDateForAlgo = endDate;
        SetCash(3_000);
        SetStartDate(startDate);
        SetEndDate(endDate);
        SetSecurityInitializer(security => security.SetFeeModel(new ConstantFeeModel(1m))); // $1 per trade
        FinanceData.StartDate = startDate;
        FinanceData.EndDate = endDate;

        var serviceProvider = StaticContainer.ServiceProvider;

        var mainTickersPuller = serviceProvider.GetRequiredService<MainTickersPuller>();
        TechnicalIdeaInput[] technicalIdeaInputs = [
            TechnicalIdeaInput.BestBuyers,
            TechnicalIdeaInput.BestSellers,
        ];

        List<string> tickers = mainTickersPuller.ScanTickers(TechnicalIdeaInput.BestBuyers.TechnicalParams, CancellationToken.None).Result.ToList();
        //foreach (var technicalIdeaInput in technicalIdeaInputs)
        //{
        //    var ideas = mainTickersPuller.ScanTickers(technicalIdeaInput.TechnicalParams, CancellationToken.None);
        //    tickers.AddRange(ideas.Result);
        //}
        // tickers.AddRange(["NIO", "BABA", "AAPL", "TSLA", "MSFT", "AMZN", "GOOGL", "FB", "NVDA", "AMD", "GME", "AMC", "BBBY", "SPCE", "NKLA", "PLTR", "RKT", "FUBO", "QS", "RIOT"]);
        // tickers = ["NIO"];
        var rangeAlgorithm = serviceProvider.GetService<RangeAlgorithmsRunner>();
        List<Task> tickersKeyLevelsLoader = [];

        foreach (var ticker in tickers)
        {
            var tickerKeyLevelsLoader = Task.Run(async () =>
            {
                var actualTicker = ticker;
                var range = await rangeAlgorithm!.Run<FinanceCandleStick>(new RangeAlgorithmInput(new PricesPullerParameters(
                    actualTicker,
                    startDateForAlgo,
                    endDateForAlgo, // I removed some years which make the algorithm to be more realistic
                    Common.Models.Pullers.Enums.Period.Daily), Algorithm.KeyLevels), CancellationToken.None);
                if (range is not KeyLevelCandleSticks candleSticks) return;
                m_TickerToKeyLevels[actualTicker] = candleSticks.KeyLevels;
            });

            tickersKeyLevelsLoader.Add(tickerKeyLevelsLoader);
        }

        Task.WhenAll(tickersKeyLevelsLoader).Wait();

        var actualTickers = m_TickerToKeyLevels.OrderByDescending(_ => _.Value?.Length ?? 0)
                                                .Take(16)
                                               .Select(_ => _.Key)
                                               .ToArray();
        foreach (var ticker in actualTickers)
        {
            if (string.IsNullOrEmpty(ticker) || !m_TickerToKeyLevels.TryGetValue(ticker, out var keyLevels) || keyLevels.Length == 0) continue;
            var symbol = AddEquity(ticker, Resolution.Minute);
            AddData<FinanceData>(ticker, Resolution.Minute);
            m_RsiIndicators[ticker] = RSI(symbol.Symbol, 14, MovingAverageType.Wilders, Resolution.Minute);

        }

    }
    public void OnData(FinanceData data)
    {
        var ticker = data.Symbol.Value;
        if (!m_TickerToKeyLevels.TryGetValue(ticker, out var keyLevels)) return;
        if (m_RsiIndicators[ticker].IsReady)
        {
            decimal rsiValue = m_RsiIndicators[ticker].Current.Value;
            bool hasPosition = Portfolio[ticker].Invested;

            if (rsiValue < 30 && !hasPosition)
            {
                SetHoldings(ticker, 1.0); return;
            }
            else if (rsiValue > 70 && hasPosition)
            {
                Liquidate(ticker); return;

            }

        }
        foreach (var value in keyLevels)
        {
            if (Math.Abs((float)data.Value - value) / value <= 0.005f)
            {
                var symbol = data.Symbol.Value;
                var holdingsq = Securities[symbol].Holdings.Quantity;

                if (holdingsq == 0 && Pivot.Low == data.CandleStick.Pivot)
                {
                    Buy(data.Symbol);

                    return;
                }
            }
        }
        var holdings = Securities[data.Symbol].Holdings;
        var avgPrice = holdings.AveragePrice;
        var currentPrice = data.Price;

        if (holdings.Quantity > 0)
        {
            if (currentPrice >= avgPrice * 1.03m || currentPrice <= avgPrice * 0.98m)
            {
                Sell(data.Symbol);
            }
        }
    }


    /// <summary>
    /// Buy this symbol
    /// </summary>
    public void Buy(Symbol symbol)
    {
        //if (_macdDic[symbol] > 0m)
        //{


        SetHoldings(symbol, 0.5);

        //Debug("Purchasing: " + symbol + "   MACD: " + _macdDic[symbol] + "   RSI: " + _rsiDic[symbol]
        //    + "   Price: " + Math.Round(Securities[symbol].Price, 2) + "   Quantity: " + s.Quantity);
        //}
    }

    /// <summary>
    /// Sell this symbol
    /// </summary>
    /// <param name="symbol"></param>
    public void Sell(Symbol symbol)
    {
        //var s = Securities[symbol].Holdings;
        //if (s.Quantity > 0 && _macdDic[symbol] < 0m)
        //{
        Liquidate(symbol);

        //Debug("Selling: " + symbol + " at sell MACD: " + _macdDic[symbol] + "   RSI: " + _rsiDic[symbol]
        //    + "   Price: " + Math.Round(Securities[symbol].Price, 2) + "   Profit from sale: " + s.LastTradeProfit);
        //}
    }
}
