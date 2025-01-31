using System;
using QuantConnect;
using QuantConnect.Algorithm;
using QuantConnect.Data;
using QuantConnect.Data.Market;
using QuantConnect.Indicators;


namespace FinanceMaker.QuanConnect;


public class RandomTradingAlgorithm : QCAlgorithm
{
    private Symbol _symbol;
    private Random _random;
    private decimal _initialCash = 5000m;

    public override void Initialize()
    {
        SetStartDate(2020, 1, 1);  // Backtest start date
        SetEndDate(2021, 1, 1);    // Backtest end date
        SetCash(_initialCash);      // Set initial capital

        _symbol = AddEquity("SPY", Resolution.Daily).Symbol;
        _random = new Random();
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
