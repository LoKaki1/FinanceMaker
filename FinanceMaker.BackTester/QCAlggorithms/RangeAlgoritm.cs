// Licensed to the .NET Foundation under one or more agreements.

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
using QuantConnect.Indicators;
using QuantConnect.Orders.Fees;

namespace FinanceMaker.BackTester.QCAlggorithms;

public class RangeAlgoritm : QCAlgorithm
{
    private Dictionary<string, float[]> m_TickerToKeyLevels = new();
    private Dictionary<string, RelativeStrengthIndex> m_RsiIndicators = new();

    public override void Initialize()
    {
        // Now we can test for last month minutely
        var startDate = DateTime.Now.AddDays(-29);
        var startDateForAlgo = new DateTime(2020, 1, 1);
        var endDate = DateTime.Now;
        var endDateForAlgo = endDate.AddYears(-1).AddMonths(11);
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
        // var random = new Random();
        // tickers = ["AAPL"], "MSFT", "GOOGL", "AMZN", "TSLA", "META", "NVDA", "NFLX", "ADBE", "ORCL", "INTC", "AMD", "CRM", "PYPL", "CSCO", "QCOM", "AVGO", "TXN", "IBM", "SHOP"];
        tickers = ["AAPL", "MSFT", "GOOGL", "AMZN", "TSLA", "META", "NVDA", "NIO", "MARA", "RIOT", "HUT", "AMD", "BABA", "BA"];
        // var tickersNumber = 20;
        // tickers = tickers.OrderBy(_ => random.Next()).Take(tickersNumber).ToList();
        //foreach (var technicalIdeaInput in technicalIdeaInputs)
        //{
        //    var ideas = mainTickersPuller.ScanTickers(technicalIdeaInput.TechnicalParams, CancellationToken.None);
        //    tickers.AddRange(ideas.Result);
        //}
        // tickers.AddRange(["NIO", "BABA", "AAPL", "TSLA", "MSFT", "AMZN", "GOOGL", "FB", "NVDA", "AMD", "GME", "AMC", "BBBY", "SPCE", "NKLA", "PLTR", "RKT", "FUBO", "QS", "RIOT"]);
        // tickers = ["RSLS", "APVO", "DGLY", "IBG", "SOBR", "ICCT", "CNTM", "VMAR", "SYRA", "PCSA"];
        // tickers = ["TSM", "COST", "BBY", "BAC", "AMZN", "INTC", "PLTR", "AMTM", "RKT", "MRX", "SMMT", "CMPX", "GRRR", "SHAK", "HOLO"];
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
                                               .Select(_ => _.Key)
                                               .ToArray();
        foreach (var ticker in actualTickers)
        {
            if (string.IsNullOrEmpty(ticker) || !m_TickerToKeyLevels.TryGetValue(ticker, out var keyLevels) || keyLevels.Length == 0) continue;
            var symbol = AddEquity(ticker, Resolution.Minute, extendedMarketHours: true);
            AddData<FinanceData>(ticker, Resolution.Minute);

        }
    }
    public void OnData(FinanceData data)
    {
        FinanceData.CounterData++;
        var ticker = data.Symbol.Value;

        if (!m_TickerToKeyLevels.TryGetValue(ticker, out var keyLevels)) return;

        // Check if we have enough cash for a single trade (StartMoney / 10)
        var minTradeAmount = Portfolio.CashBook.TotalValueInAccountCurrency / 10m;
        if (Portfolio.Cash < minTradeAmount)
        {
            return;
        }

        foreach (var value in keyLevels)
        {
            var valueDivision = Math.Abs((float)data.CandleStick.Close) / value;
            var pivot = data.CandleStick.Pivot;

            if (valueDivision <= 1 && valueDivision >= 0.995 && pivot == Pivot.Low)
            {
                {
                    var symbol = data.Symbol.Value;
                    var holdingsq = Securities[symbol].Holdings.Quantity;

                    if (holdingsq == 0)
                    {
                        // var previousHistory = History<FinanceData>(data.Symbol, 1, Resolution.Minute);

                        // if (previousHistory is not null && previousHistory.Any())
                        {
                            // // var previous = previousHistory.First();
                            // if (previous.CandleStick.Open > data.CandleStick.Close &&
                            //     data.CandleStick.Open > data.CandleStick.Close &&
                            //     valueDivision <= 1 && valueDivision >= 0.995)
                            if (valueDivision <= 1 && valueDivision >= 0.995)
                            {
                                Buy(data.Symbol);
                            }
                        }

                        return;
                    }
                }
            }
            //if (valueDivision <= 1 && valueDivision >= 0.995 && pivot == Pivot.High)
            //{
            //    {
            //        var symbol = data.Symbol.Value;
            //        var holdingsq = Securities[symbol].Holdings.Quantity;

            //        if (holdingsq == 0)
            //        {
            //            // var previousHistory = History<FinanceData>(data.Symbol, 1, Resolution.Minute);

            //            // if (previousHistory is not null && previousHistory.Any())
            //            {
            //                // // var previous = previousHistory.First();
            //                // if (previous.CandleStick.Open > data.CandleStick.Close &&
            //                //     data.CandleStick.Open > data.CandleStick.Close &&
            //                //     valueDivision <= 1 && valueDivision >= 0.995)
            //                if (valueDivision <= 1 && valueDivision >= 0.995)
            //                {
            //                    Short(data.Symbol);
            //                }
            //            }

            //            return;
            //        }
            //    }
            //}

            var holdings = Securities[data.Symbol].Holdings;
            var avgPrice = holdings.AveragePrice;
            var currentPrice = (decimal)data.CandleStick.Close;

            if (holdings.Quantity > 0)
            {
                if (currentPrice >= avgPrice * 1.025m || currentPrice <= avgPrice * 0.985m)
                {
                    Sell(data.Symbol);
                }
            }
            if (holdings.Quantity < 0)
            {
                if (currentPrice >= avgPrice * 1.015m || currentPrice <= avgPrice * 0.975m)
                {
                    Sell(data.Symbol);
                }
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
    public void Short(Symbol symbol)
    {
        //var s = Securities[symbol].Holdings;
        //if (s.Quantity > 0 && _macdDic[symbol] < 0m)
        //{
        SetHoldings(symbol, 0.5);

        //Debug("Selling: " + symbol + " at sell MACD: " + _macdDic[symbol] + "   RSI: " + _rsiDic[symbol]
        //    + "   Price: " + Math.Round(Securities[symbol].Price, 2) + "   Profit from sale: " + s.LastTradeProfit);
        //}
    }
}

// Be sure to replace <YOUR_ALPACA_API_KEY> and <YOUR_ALPACA_SECRET_KEY> with your actual Alpaca credentials before use.
public static class LiveTradingRunner
{
    public static void RunAlpacaLive()
    {
        var config = new Dictionary<string, string>
        {
            ["environment"] = "live",
            ["live-mode-brokerage"] = "Alpaca",
            ["alpaca-key-id"] = "<YOUR_ALPACA_API_KEY>",
            ["alpaca-secret-key"] = "<YOUR_ALPACA_SECRET_KEY>",
            ["alpaca-trading-mode"] = "live",
            ["live-data-provider"] = "Alpaca",
            ["job-project-id"] = "RangeAlgorithm",
            ["algorithm-type-name"] = "FinanceMaker.BackTester.QCAlggorithms.RangeAlgoritm",
            ["algorithm-location"] = "FinanceMaker.BackTester.dll"
        };

        var configFilePath = Path.Combine(Directory.GetCurrentDirectory(), "config.json");
        File.WriteAllText(configFilePath, System.Text.Json.JsonSerializer.Serialize(config, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));

        var leanPath = "lean"; // assuming LEAN CLI is installed and in PATH
        var psi = new System.Diagnostics.ProcessStartInfo
        {
            FileName = leanPath,
            Arguments = "live start --config config.json",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = System.Diagnostics.Process.Start(psi);
        if (process != null)
        {
            Console.WriteLine(process.StandardOutput.ReadToEnd());
            Console.Error.WriteLine(process.StandardError.ReadToEnd());
            process.WaitForExit();
        }
    }
}
