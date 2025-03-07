using QuantConnect;
using QuantConnect.Configuration;
using QuantConnect.Lean.Engine;
using QuantConnect.Util;

namespace FinanceMaker.BackTester;

public class BackTester
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
        Config.Set("algorithm-type-name", algorithm.Name);
        Config.Set("data-folder", "../../../../FinanceMaker.BackTester/Data");
        Config.Set("algorithm-language", "CSharp");
        Config.Set("algorithm-location", "FinanceMaker.BackTester.dll");

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

        var dataFolder = Config.Get("data-folder");
        var customDataDirectory = Path.Combine(dataFolder, "Custom");
        Directory.Delete(customDataDirectory, true);
    }
}
