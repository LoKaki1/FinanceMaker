using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using FinanceMaker.Algorithms;
using FinanceMaker.Algorithms.News.Analyziers;
using FinanceMaker.Algorithms.News.Analyziers.Interfaces;
using FinanceMaker.BackTester.QCAlggorithms;
using FinanceMaker.Common;
using FinanceMaker.Common.Models.Finance;
using FinanceMaker.Common.Models.Ideas.IdeaInputs;
using FinanceMaker.Common.Models.Ideas.IdeaOutputs;
using FinanceMaker.Common.Models.Pullers.Enums;
using FinanceMaker.Ideas.Ideas;
using FinanceMaker.Ideas.Ideas.Abstracts;
using FinanceMaker.Publisher.Orders.Trader;
using FinanceMaker.Publisher.Orders.Trader.Interfaces;
using FinanceMaker.Publisher.Traders;
using FinanceMaker.Publisher.Traders.Interfaces;
using FinanceMaker.Pullers;
using FinanceMaker.Pullers.NewsPullers;
using FinanceMaker.Pullers.NewsPullers.Interfaces;
using FinanceMaker.Pullers.PricesPullers;
using FinanceMaker.Pullers.PricesPullers.Interfaces;
using FinanceMaker.Pullers.TickerPullers;
using FinanceMaker.Pullers.TickerPullers.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ScottPlot;
Console.WriteLine("Hello, World!");


// My so called idea should look something like this
//
// We have the scanners like finviz and news providers like bazinga, yahoo, etc.
// From the `TickersPullers` we pull the interesting tickers, then we pull their prices news with `NewsPullers` and `PricesPullers`
// Then we filter them with the `NewsFilters` and `ChartFilters`
// Then we caluclate intersting ideas with some alogirthms we copy from the internet
// Then we create some trades from the ideas, with `TradesCreators`
// And Publish them with the trades publishers
// Overall the flow should look like this
//
//     |Finviz|  |Bazinga|  |Yahoo|
//         |         |         |
//         |         |         |
//         V         V         V
//
//             |TickersPullers| * Ticker pullers can be both from news and both from scanners
//                  /\
//                 /  \
//    |PricesPullers|  |NewsPuller|       * Those pullers pull the data from the found tickers 
//          |               |
//          |               |
//          V               V
//    |ChartFilters|     |NewsFilters|  * Remeber we already got tickers by a specific filter, so we filter them for relevant trades now (We can add some logic to save those ticker to next day and create new puller called database puller)
//         \                  /
//          \                /   
//       ---> |IdeasCreators| * Here we create an `Idea` -> Idea is a model which contains multiple outcomes about the ticker and how to handle them
//       |          |           IMPORTANT -> `Idea` is not only a stop loss and take profit calcualator it can be dynamic, change trades after publishing, 
//       |          |           add extra, take partial, listen to more news while in a trade, or even scan other related stocks.
//       |          V           That's why it is in a loop with the trade creators and publishers
//       |    |TradesCreators| * Trades creators -> A parser from an idea to trades
//       |          |
//       |          |
//       |          V
//        --- |TradesPublisher| * Publishing to the relevant brokers
// 
// Client Portal Web API usually uses self-signed certs, so bypass validation (for dev only!)
HttpClientHandler handler = new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
};

using var httpClient = new HttpClient(handler);
httpClient.BaseAddress = new Uri("https://localhost:5001/v1/api/");
httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
httpClient.DefaultRequestHeaders.Add("Host", "api.ibkr.com");
httpClient.DefaultRequestHeaders.Add("User-Agent", "api.ibkr.com");

try
{
    var response = await httpClient.GetAsync("iserver/auth/status");
    var content = await response.Content.ReadAsStringAsync();
    Console.WriteLine($"Auth Status Response:\n{content}");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
var app = Host.CreateDefaultBuilder(args)
            .ConfigureServices(services =>
            {
                services.AddHttpClient();
                // Can't use the extension (the service collection becomes read only after the build)
                services.AddSingleton<MarketStatus>();
                services.AddSingleton<FinvizTickersPuller>();
                services.AddSingleton<TradingViewTickersPuller>();
                services.AddSingleton<MostVolatiltyTickers>();
                services.AddSingleton(sp => new IParamtizedTickersPuller[]
                {
                    sp.GetService<MostVolatiltyTickers>()
                });
                services.AddSingleton(sp => Array.Empty<IRelatedTickersPuller>());
                services.AddSingleton(sp => Array.Empty<ITickerPuller>());
                services.AddSingleton<MainTickersPuller>();

                services.AddSingleton<YahooPricesPuller>();
                services.AddSingleton<YahooInterdayPricesPuller>();

                services.AddSingleton(sp => new IPricesPuller[]
                {
                    // sp.GetService<YahooPricesPuller>(),
                    sp.GetService<YahooInterdayPricesPuller>(),

                });
                services.AddSingleton<IPricesPuller, MainPricesPuller>();
                services.AddSingleton<GoogleNewsPuller>();
                services.AddSingleton<YahooFinanceNewsPuller>();
                services.AddSingleton(sp => new INewsPuller[]
                {
                    sp.GetService<GoogleNewsPuller>(),
                    sp.GetService<YahooFinanceNewsPuller>(),
                    sp.GetService<FinvizNewPuller>(),

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


                        return [runner1, runner2, runner3];
                    }
                );

                services.AddSingleton<RangeAlgorithmsRunner>();
                services.AddSingleton<INewsPuller, MainNewsPuller>();
                services.AddSingleton<KeywordsDetectorAnalysed>();
                services.AddSingleton<INewsAnalyzer[]>(sp => [
                    sp.GetService<KeywordsDetectorAnalysed>()
                ]);
                services.AddSingleton<INewsAnalyzer, NewsAnalyzer>();
                services.AddSingleton<IdeaBase<TechnicalIdeaInput, EntryExitOutputIdea>, OverNightBreakout>();
                services.AddSingleton<OverNightBreakout>();
                services.AddSingleton<IBroker, AlpacaBroker>();
                services.AddSingleton<ITrader, QCTrader>();


            })
            .Build();

// var startDateForAlgo = new DateTime(2020, 1, 1);
// var endDate = DateTime.Now;
// var startDateForAlgo = endDate.Subtract(TimeSpan.FromDays(2));
// var endDateForAlgo = endDate;
// var rangeAlgorithm = app.Services.GetService<RangeAlgorithmsRunner>();
// var range = await rangeAlgorithm!.Run<EMACandleStick>(new RangeAlgorithmInput(new PricesPullerParameters(
//                     "TSLA",
//                     startDateForAlgo,
//                     endDateForAlgo, // I removed some years which make the algorithm to be more realistic
//                     Period.OneMinute), Algorithm.KeyLevels), CancellationToken.None);

// var candles = range; ;
// var keyLevels = range as KeyLevelCandleSticks;

// // Convert to OHLC arrays
// OHLC[] ohlcs = candles.Select(c => new OHLC(c.Open, c.High, c.Low, c.Close, c.Time, TimeSpan.FromMinutes(1))).ToArray();


// ScottPlot.Plot myPlot = new();
// myPlot.Add.Candlestick(ohlcs);
// if (keyLevels != null)
// {
//     foreach (var level in keyLevels.KeyLevels)
//     {
//         myPlot.Add.HorizontalLine(level, color: Color.FromColor(System.Drawing.Color.Aqua), width: 2);
//     }
// }

// myPlot.SavePng("quickstart.png", 2560, 1440);
// BackTester.Runner(typeof(RangeAlgoritm));
LeanLauncher.StartLiveAlpaca();