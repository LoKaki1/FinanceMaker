using FinanceMaker.Algorithms;
using FinanceMaker.Algorithms.News.Analyziers;
using FinanceMaker.Algorithms.News.Analyziers.Interfaces;
using FinanceMaker.BackTester.QCHelpers;
using FinanceMaker.Common;
using FinanceMaker.Common.Models.Finance;
using FinanceMaker.Common.Models.Ideas.IdeaInputs;
using FinanceMaker.Common.Models.Ideas.IdeaOutputs;
using FinanceMaker.Ideas.Ideas;
using FinanceMaker.Ideas.Ideas.Abstracts;
using FinanceMaker.Pullers;
using FinanceMaker.Pullers.NewsPullers;
using FinanceMaker.Pullers.NewsPullers.Interfaces;
using FinanceMaker.Pullers.PricesPullers;
using FinanceMaker.Pullers.PricesPullers.Interfaces;
using FinanceMaker.Pullers.TickerPullers;
using FinanceMaker.Pullers.TickerPullers.Interfaces;
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
        var endDate = DateTime.Now;
        SetCash(10_000);
        SetStartDate(startDate);
        SetEndDate(endDate);
        SetSecurityInitializer(security => security.SetFeeModel(new ConstantFeeModel(0.5m))); // $1 per trade
        FinanceData.StartDate = startDate;
        FinanceData.EndDate = endDate;
        var services = new ServiceCollection();
        services.AddHttpClient();
        // Can't use the extension (the service collection becomes read only after the build)
        services.AddSingleton<MarketStatus>();
        services.AddSingleton<FinvizTickersPuller>();
        services.AddSingleton<TradingViewTickersPuller>();
        services.AddSingleton<NivTickersPuller>();
        services.AddSingleton(sp => new IParamtizedTickersPuller[]
        {
            sp.GetService<FinvizTickersPuller>()!,
            sp.GetService<NivTickersPuller>()!,
            sp.GetService<TradingViewTickersPuller>()!,
        });
        services.AddSingleton(sp => new ITickerPuller[]
        {
        });
        services.AddSingleton(sp => Array.Empty<IRelatedTickersPuller>());
        services.AddSingleton<MainTickersPuller>();

        services.AddSingleton<YahooPricesPuller>();
        services.AddSingleton<YahooInterdayPricesPuller>();

        services.AddSingleton(sp => new IPricesPuller[]
        {
            // sp.GetService<YahooPricesPuller>(),
            sp.GetService<YahooInterdayPricesPuller>()!,

        });
        services.AddSingleton<IPricesPuller, MainPricesPuller>();
        services.AddSingleton<GoogleNewsPuller>();
        services.AddSingleton<YahooFinanceNewsPuller>();
        services.AddSingleton(sp => new INewsPuller[]
        {
            sp.GetService<GoogleNewsPuller>()!,
            sp.GetService<YahooFinanceNewsPuller>()!,
            sp.GetService<FinvizNewPuller>()!,

        });

        services.AddSingleton<KeyLevelsRunner>();
        services.AddSingleton<EMARunner>();
        services.AddSingleton<BreakOutDetectionRunner>();

        services.AddSingleton<IEnumerable<IAlgorithmRunner<RangeAlgorithmInput>>>(
            sp =>
            {
                var runner3 = sp.GetService<KeyLevelsRunner>();
                var runner1 = sp.GetService<EMARunner>();
                var runner2 = sp.GetService<BreakOutDetectionRunner>();


                return [runner1!, runner2!, runner3!];
            }
        );

        services.AddSingleton<RangeAlgorithmsRunner>();
        services.AddSingleton<INewsPuller, MainNewsPuller>();
        services.AddSingleton<KeywordsDetectorAnalysed>();
        services.AddSingleton<INewsAnalyzer[]>(sp => [
            sp.GetService<KeywordsDetectorAnalysed>()!
        ]);
        services.AddSingleton<INewsAnalyzer, NewsAnalyzer>();
        services.AddSingleton<IdeaBase<TechnicalIdeaInput, EntryExitOutputIdea>, OverNightBreakout>();
        services.AddSingleton<OverNightBreakout>();

        using var serviceProvider = services.BuildServiceProvider();

        var mainTickersPuller = serviceProvider.GetRequiredService<MainTickersPuller>();
        TechnicalIdeaInput[] technicalIdeaInputs = [
            TechnicalIdeaInput.BestBuyers,
                TechnicalIdeaInput.BestSellers,
            ];

        List<string> tickers = [];

        foreach (var technicalIdeaInput in technicalIdeaInputs)
        {
            var ideas = mainTickersPuller.ScanTickers(technicalIdeaInput.TechnicalParams, CancellationToken.None);
            tickers.AddRange(ideas.Result);
        }

        var rangeAlgorithm = serviceProvider.GetService<RangeAlgorithmsRunner>();
        List<Task> tickersKeyLevelsLoader = [];

        foreach (var ticker in tickers)
        {


            var tickerKeyLevelsLoader = Task.Run(async () =>
            {
                var actualTicker = ticker;
                var range = await rangeAlgorithm!.Run<FinanceCandleStick>(new RangeAlgorithmInput(new PricesPullerParameters(actualTicker, startDate, endDate, FinanceMaker.Common.Models.Pullers.Enums.Period.Daily), Algorithm.KeyLevels), CancellationToken.None);
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
            if (Math.Abs((float)data.Value - value) / value <= 0.01f)
            {
                var symbol = data.Symbol.Value;
                var holdings = Securities[symbol].Holdings.Quantity;

                if (holdings == 0)
                {
                    Buy(data.Symbol);
                }
                else
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


        SetHoldings(symbol, 0.02);

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
