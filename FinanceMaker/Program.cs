// See https://aka.ms/new-console-template for more information

//using FinanceMaker.Algorithms.QuantConnectAlgorithms;
using Microsoft.Extensions.Hosting;
using QuantConnect;
using QuantConnect.Configuration;
using QuantConnect.Lean.Engine;
using QuantConnect.Util;

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
//   "algorithm-type-name": "ZeroFeeRegressionAlgorithm",

//   // Algorithm language selector - options CSharp, Python
//   "algorithm-language": "CSharp",

//   //Physical DLL location
//   "algorithm-location": "QuantConnect.Algorithm.CSharp.dll",
Config.Set("algorithm-type-name", "AlgorithmBaseForTestingOnly");
Config.Set("data-folder", "../../../../Lean/Data");
Config.Set("algorithm-language", "CSharp");
Config.Set("algorithm-location", "FinanceMaker.dll");


//Name thread for the profiler:
Thread.CurrentThread.Name = "Algorithm Analysis Thread";

Initializer.Start();
var leanEngineSystemHandlers = Initializer.GetSystemHandlers();

//-> Pull job from QuantConnect job queue, or, pull local build:
var job = leanEngineSystemHandlers.JobQueue.NextJob(out var assemblyPath);

var leanEngineAlgorithmHandlers = Initializer.GetAlgorithmHandlers();

// Create the algorithm manager and start our engine
var algorithmManager = new AlgorithmManager(QuantConnect.Globals.LiveMode, job);

leanEngineSystemHandlers.LeanManager.Initialize(leanEngineSystemHandlers, leanEngineAlgorithmHandlers, job, algorithmManager);

OS.Initialize();

var engine = new Engine(leanEngineSystemHandlers, leanEngineAlgorithmHandlers, QuantConnect.Globals.LiveMode);
engine.Run(job, algorithmManager, assemblyPath, WorkerThread.Instance);
Console.WriteLine(engine.AlgorithmHandlers.Results);

// Now everything works using the library instead of the cloned code
// What we need to do now is first oranize all this shit 
// then understand how we take all the out from this
// but before don't forget to add the data folder (They need it)
// 
