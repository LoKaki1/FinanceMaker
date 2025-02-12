using System;
using System.Threading;
using System.Threading.Tasks;
using Fasterflect;
using FinanceMaker.Common.Models.Pullers.YahooFinance;
using QuantConnect;
using QuantConnect.Algorithm;
using QuantConnect.Brokerages;
using QuantConnect.Configuration;
using QuantConnect.Data;
using QuantConnect.Interfaces;
using QuantConnect.Lean.Engine;
using QuantConnect.Lean.Engine.DataFeeds;
using QuantConnect.Lean.Engine.RealTime;
using QuantConnect.Lean.Engine.Results;
using QuantConnect.Lean.Engine.Setup;
using QuantConnect.Lean.Engine.TransactionHandlers;
using QuantConnect.Orders;
using QuantConnect.Orders.Fees;
using QuantConnect.Packets;
using QuantConnect.Securities;
using QuantConnect.Util;

public static class BacktestRunner
{
    public static async Task<decimal> RunBacktestAsync(
        QCAlgorithm algorithm,
        string ticker,
        DateTime startDate,
        DateTime endDate,
        decimal initialCapital,
        decimal commissionPerTrade,
        CancellationToken cancellationToken)
    {
        return await Task.Run(() =>
        {
            // Configure Lean environment
            Config.Set("data-folder", "path/to/your/data"); // Update with your data path
            Config.Set("environment", "backtesting");
            // Initialize algorithm
            algorithm.SetStartDate(startDate);
            algorithm.SetEndDate(endDate);
            algorithm.SetCash(initialCapital);
            algorithm.SetBrokerageModel(BrokerageName.InteractiveBrokersBrokerage, AccountType.Margin);
            algorithm.SetSecurityInitializer(security => security.SetFeeModel(new ConstantFeeModel(commissionPerTrade)));
            // Set up Lean engine components
            var systemHandlers = LeanEngineSystemHandlers.FromConfiguration(Composer.Instance);
            var algorithmHandlers = LeanEngineAlgorithmHandlers.FromConfiguration(Composer.Instance);

            // Create backtest job
            var job = new BacktestNodePacket(
            );
            // Initialize Lean engine
            var engine = new Engine(systemHandlers, algorithmHandlers, false);
            var m = new AlgorithmManager(false, job);
            // Run the algorithm
            engine.Run(job, m, "", WorkerThread.Instance);

            // Calculate profit/loss
            return algorithm.Portfolio.TotalPortfolioValue - initialCapital;
        }, cancellationToken);
    }
}

public class AlgorithmRandom : QCAlgorithm
{
    private Random _random;
    private string _symbol = "NVDA";
    private decimal _initialCash = 5000m;

    public override void Initialize()
    {
        _random = new Random();
        _symbol = "NVDA";
    }

    public override void OnData(Slice data)
    {
        if (!data.Bars.ContainsKey(_symbol)) return;

        if (Portfolio[_symbol].Invested)
        {
            if (_random.NextDouble() < 0.5)  // 50% chance to sell
            {
                Liquidate(_symbol);
            }
        }
        else
        {
            if (_random.NextDouble() < 0.5)  // 50% chance to buy
            {
                SetHoldings(_symbol, 1.0);  // Invest all available capital
            }
        }
    }

    public override void OnEndOfAlgorithm()
    {
        Debug($"Final Portfolio Value: {Portfolio.TotalPortfolioValue}");
    }

}
