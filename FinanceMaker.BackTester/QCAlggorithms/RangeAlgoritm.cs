using FinanceMaker.Algorithms;
using FinanceMaker.BackTester.QCHelpers;
using FinanceMaker.Common;
using FinanceMaker.Common.Models.Finance;
using FinanceMaker.Common.Models.Finance.Enums;
using FinanceMaker.Common.Models.Ideas.IdeaInputs;
using FinanceMaker.Pullers.TickerPullers;
using Microsoft.Extensions.DependencyInjection;
using QuantConnect;
using QuantConnect.Algorithm;
using QuantConnect.Orders.Fees;

namespace FinanceMaker.BackTester.QCAlggorithms;

public class RangeAlgoritm : QCAlgorithm
{
    private Dictionary<string, float[]> m_TickerToKeyLevels = new();

    public override void Initialize()
    {
        var startDate = new DateTime(2021, 1, 1);
        var endDate = new DateTime(2025, 1, 1);
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

        List<string> tickers = [];

        // foreach (var technicalIdeaInput in technicalIdeaInputs)
        // {
        //     var ideas = mainTickersPuller.ScanTickers(technicalIdeaInput.TechnicalParams, CancellationToken.None);
        //     tickers.AddRange(ideas.Result);
        // }
        tickers = ["NIO", "BABA", "AAPL", "TSLA", "MSFT", "AMZN", "GOOGL", "FB", "NVDA", "AMD"];
        var rangeAlgorithm = serviceProvider.GetService<RangeAlgorithmsRunner>();
        List<Task> tickersKeyLevelsLoader = [];

        foreach (var ticker in tickers)
        {


            var tickerKeyLevelsLoader = Task.Run(async () =>
            {
                var actualTicker = ticker;
                var range = await rangeAlgorithm!.Run<FinanceCandleStick>(new RangeAlgorithmInput(new PricesPullerParameters(
                    actualTicker,
                    startDate,
                    endDate.AddYears(-1), // I removed some years which make the algorithm to be more realistic
                    Common.Models.Pullers.Enums.Period.Daily), Algorithm.KeyLevels), CancellationToken.None);
                if (range is not KeyLevelCandleSticks candleSticks) return;
                m_TickerToKeyLevels[actualTicker] = candleSticks.KeyLevels;
            });

            tickersKeyLevelsLoader.Add(tickerKeyLevelsLoader);
        }

        Task.WhenAll(tickersKeyLevelsLoader).Wait();

        var actualTickers = m_TickerToKeyLevels.OrderByDescending(_ => _.Value?.Length ?? 0)
                                               .Take(10)
                                               .Select(_ => _.Key)
                                               .ToArray();
        foreach (var ticker in actualTickers)
        {
            AddEquity(ticker, Resolution.Daily);
            AddData<FinanceData>(ticker, Resolution.Daily);
        }
    }

    public void OnData(FinanceData data)
    {
        if (!m_TickerToKeyLevels.TryGetValue(data.Symbol.Value, out var keyLevels)) return;

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


        SetHoldings(symbol, 0.99);

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
