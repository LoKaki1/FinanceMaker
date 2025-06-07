using System.Diagnostics;
using FinanceMaker.BackTester.QCHelpers;
using Newtonsoft.Json.Linq;
using QuantConnect;
using QuantConnect.Algorithm.CSharp.Benchmarks;
using QuantConnect.Brokerages;
using QuantConnect.Configuration;
using QuantConnect.Lean.Engine;
using QuantConnect.Packets;
using QuantConnect.Util;

namespace FinanceMaker.BackTester;

public class RealTimeTester
{
    /// <summary>
    /// This project is a backtester for the FinanceMaker project.
    /// Foreach algorithm we want to backtest it using the quantconnect SDK, 
    /// 
    /// Problem -
    ///     QuantConnect does not support the creation of algorithm instances using
    ///     a factory pattern
    ///
    /// Solution -
    ///     1. We may add them as submodule and change 
    ///        it (I really don't want to do this)
    ///     
    ///    2. For each algorithm we want to backtest, 
    ///        we initiaze the container for it like we did in the csv function
    ///       
    /// Now I'm not exactly sure what will be the future for this project
    /// So I need to decide if we want to continue with the dynamic Traders 
    /// By choosing the best algorithm for the current stock 
    /// (by running on it multiple algorithms and choosing the best one) 
    /// And then starts trading with this, 
    ///  
    /// Or 
    /// 
    /// Run do it by hand and not automate it,
    /// 
    /// I think the best aproch is to create the echo system which allow me
    /// To get the intersting tickers for today run on each of them all the algorithms
    /// and the backtesting, and then choose the best algorithm, 
    /// get the buy and sell recommendations and then trade on them by hand
    /// 
    /// input - Nothing
    /// Output - List of stocks and the ideas to how to trade them
    /// </summary>
    /// 

    public static void Runner(Type algorithm)
    {
        var config = new Dictionary<string, string>
        {
            ["environment"] = "live",
            ["live-mode-brokerage"] = "Alpaca",
            ["alpaca-key-id"] = "PK6OYIS35FWJ6PRIEL9Q",
            ["alpaca-secret-key"] = "gusmuJp0mFgYz8LTooQs4N806Rc8DjPb386u6PaN",
            ["alpaca-trading-mode"] = "live",
            ["live-data-provider"] = "Alpaca",
            ["job-project-id"] = "RangeAlgorithmLiveRun",
            ["algorithm-type-name"] = "FinanceMaker.BackTester.QCAlggorithms.RangeAlgoritm",
            ["algorithm-location"] = "FinanceMaker.BackTester.dll"
        };

        var configFilePath = Path.Combine(Directory.GetCurrentDirectory(), "alpaca_config.json");
        File.WriteAllText(configFilePath, System.Text.Json.JsonSerializer.Serialize(config, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));

        Config.Set("algorithm-type-name", algorithm.Name);
        Config.Set("data-folder", "../../../../FinanceMaker.BackTester/Data");
        Config.Set("algorithm-language", "CSharp");
        Config.Set("algorithm-location", "FinanceMaker.BackTester.dll");
        Config.Set("environment", "live-alpaca");
        Config.Set("live-mode", true);
        Config.Set("brokerage", "AlpacaBrokerage");
        Config.Set("data-queue-handler", JToken.FromObject(new List<string> { "FinanceMaker.BackTester.QCHelpers.YahooLiveQuoteHandler" }));
        Config.Set("history-provider", JToken.FromObject(new List<string> { "FinanceMaker.BackTester.QCHelpers.SubscriptionDataReaderHistoryProvider" }));

        //Name thread for the profiler:
        Thread.CurrentThread.Name = "Algorithm Analysis Thread";
        var b = new YahooLiveQuoteHandler();
        var c = new SubscriptionDataReaderHistoryProvider();
        Composer.Instance.AddPart(b);
        Composer.Instance.AddPart(c);

        Initializer.Start();
        var leanEngineSystemHandlers = Initializer.GetSystemHandlers();
        //var liveJob = new LiveNodePacket
        //{
        //    Type = PacketType.LiveNode,
        //    DataQueueHandler = "AlpacaDataQueueHandler",
        //    Language = Language.CSharp,
        //    Parameters = new Dictionary<string, string>(),
        //    // Add your own settings here
        //};
        //-> Pull job from QuantConnect job queue, or, pull local build:
        var liveJob = leanEngineSystemHandlers.JobQueue.NextJob(out var assemblyPath);
        //-> Pull job from QuantConnect job queue, or, pull local build:
        // var job = leanEngineSystemHandlers.JobQueue.NextJob(out var assemblyPath);

        var leanEngineAlgorithmHandlers = Initializer.GetAlgorithmHandlers();

        // Create the algorithm manager and start our engine
        var algorithmManager = new AlgorithmManager(true, liveJob);

        leanEngineSystemHandlers.LeanManager.Initialize(leanEngineSystemHandlers, leanEngineAlgorithmHandlers, liveJob, algorithmManager);

        OS.Initialize();

        var engine = new Engine(leanEngineSystemHandlers, leanEngineAlgorithmHandlers, true);
        engine.Run(liveJob, algorithmManager, "FinanceMaker.BackTester.dll", WorkerThread.Instance);

        var dataFolder = Config.Get("data-folder");
        var customDataDirectory = Path.Combine(dataFolder, "Custom");
        var aaa = engine.AlgorithmHandlers.Results;
        var data = FinanceData.CounterDataSource;

        if (Directory.Exists(customDataDirectory))
        {
            Directory.Delete(customDataDirectory, true);
        }
    }

    // All that left is to connect the algorithm to the worker, so do it in the bus,
    // We want to use the keylevel algorithm to get the key levels for the stock
    // Then buy the stock if its pivot low, that's it
    // simple as that, and the result might be amzing, so far it did like 873% on the back testing
    // therefore we must check it on real life trading.
    // We don't need to connect the backtester to the worker, we just need to implement this logic, in the worker
    // I don't know ennglish that well its just using the Copilot
}
