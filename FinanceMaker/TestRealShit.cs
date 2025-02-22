using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using QuantConnect;
using QuantConnect.Algorithm;
using QuantConnect.Data;
using QuantConnect.Indicators;

namespace FinanceMaker
{
    class TestRealShit : QCAlgorithm
    {
        private readonly float[] DepenedencyInjectionCode = [  13.01f, 23.98f, 11.67f, 24.43f, 18.59f, 21.97f, 16.54f, 22.74f, 8.38f, 12.38f,
  14.03f, 9.5f, 13.22f, 8.03f, 10.75f, 8.75f, 7.33f, 8.54f, 8.85f, 7f,
  10.21f, 16.18f, 7.93f, 7.11f, 9.57f, 5.3f, 6.34f, 4.78f, 6.3f, 3.61f,
  6.05f, 4.71f, 5.63f, 4.14f, 5.04f, 3.63f, 3.68f, 7.71f, 4.28f, 5.36f,
  4.91f, 4.01f, 4.5f];
        private decimal _currCape;
        private readonly decimal[] _c = new decimal[4];
        private readonly decimal[] _cCopy = new decimal[4];
        private bool _newLow;
        private int _counter;
        private int _counter2;
        private MovingAverageConvergenceDivergence _macd;
        private RelativeStrengthIndex _rsi = new RelativeStrengthIndex(14);
        private readonly ArrayList _symbols = new ArrayList();
        private readonly Dictionary<string, RelativeStrengthIndex> _rsiDic = new Dictionary<string, RelativeStrengthIndex>();
        private readonly Dictionary<string, MovingAverageConvergenceDivergence> _macdDic = new Dictionary<string, MovingAverageConvergenceDivergence>();
        public override void Initialize()
        {
            SetCash(100000);
            SetStartDate(2021, 1, 1);
            SetEndDate(2024, 2, 22);

            //Present Social Media Stocks:
            // symbols.Add("FB");symbols.Add("LNKD");symbols.Add("GRPN");symbols.Add("TWTR");
            // SetStartDate(2011, 1, 1);
            // SetEndDate(2014, 12, 1);

            //2008 Financials:
            // symbols.Add("C");symbols.Add("AIG");symbols.Add("BAC");symbols.Add("HBOS");
            // SetStartDate(2003, 1, 1);
            // SetEndDate(2011, 1, 1);

            //2000 Dot.com:
            // symbols.Add("IPET");symbols.Add("WBVN");symbols.Add("GCTY");
            // SetStartDate(1998, 1, 1);
            // SetEndDate(2000, 1, 1);

            //2000 Dot.com:
            // symbols.Add("IPET");symbols.Add("WBVN");symbols.Add("GCTY");
            // SetStartDate(1998, 1, 1);
            // SetEndDate(2000, 1, 1);

            //CAPE data
            AddData<CAPE>("CAPE");

            foreach (string stock in _symbols)
            {
                AddSecurity(SecurityType.Equity, stock, Resolution.Minute);

                _macd = MACD(stock, 12, 26, 9, MovingAverageType.Exponential, Resolution.Daily);
                _macdDic.Add(stock, _macd);
                _rsi = RSI(stock, 14, MovingAverageType.Exponential, Resolution.Daily);
                _rsiDic.Add(stock, _rsi);

                Securities[stock].SetLeverage(10);
            }
            //CAPE data
                //_macd = MACD(stock, 12, 26, 9, MovingAverageType.Exponential, Resolution.Daily);
                //_macdDic.Add(stock, _macd);
                //_rsi = RSI(stock, 14, MovingAverageType.Exponential, Resolution.Daily);
                //_rsiDic.Add(stock, _rsi);

        }

        /// <summary>
        /// Trying to find if current Cape is the lowest Cape in three months to indicate selling period
        /// </summary>
        public void OnData(CAPE data)
        {
            //_newLow = false;
            ////Adds first four Cape Ratios to array c
            //_currCape = data.Cape;
            //if (_counter < 4)
            //{
            //    _c[_counter++] = _currCape;
            //}
            ////Replaces oldest Cape with current Cape
            ////Checks to see if current Cape is lowest in the previous quarter
            ////Indicating a sell off
            //else
            //{
            //    Array.Copy(_c, _cCopy, 4);
            //    Array.Sort(_cCopy);
            //    if (_cCopy[0] > _currCape) _newLow = true;
            //    _c[_counter2++] = _currCape;
            //    if (_counter2 == 4) _counter2 = 0;
            //}

            //Debug("Current Cape: " + _currCape + " on " + data.Time);
            //if (_newLow) Debug("New Low has been hit on " + data.Time);
        }

        /// <summary>
        /// New data for our assets.
        /// </summary>
        public override void OnData(Slice slice)
        {

            if (DepenedencyInjectionCode.Contains((float)Securities["NIO"].Price))
            {

            }
            //try
            //{
            //    //Bubble territory
            //    if (_currCape > 20 && _newLow == false)
            //    {
            //        foreach (string stock in _symbols)
            //        {
            //            //Order stock based on MACD
            //            //During market hours, stock is trading, and sufficient cash
            //            if (Securities[stock].Holdings.Quantity == 0 && _rsiDic[stock] < 70
            //                && Securities[stock].Price != 0 && Portfolio.Cash > Securities[stock].Price * 100
            //                && Time.Hour == 9 && Time.Minute == 31)
            //            {
            //                Buy(stock);
            //            }
            //            //Utilize RSI for overbought territories and liquidate that stock
            //            if (_rsiDic[stock] > 70 && Securities[stock].Holdings.Quantity > 0
            //                    && Time.Hour == 9 && Time.Minute == 31)
            //            {
            //                Sell(stock);
            //            }
            //        }
            //    }

            //    // Undervalued territory
            //    else if (_newLow)
            //    {
            //        foreach (string stock in _symbols)
            //        {

            //            //Sell stock based on MACD
            //            if (Securities[stock].Holdings.Quantity > 0 && _rsiDic[stock] > 30
            //                && Time.Hour == 9 && Time.Minute == 31)
            //            {
            //                Sell(stock);
            //            }
            //            //Utilize RSI and MACD to understand oversold territories
            //            else if (Securities[stock].Holdings.Quantity == 0 && _rsiDic[stock] < 30
            //                && Securities[stock].Price != 0 && Portfolio.Cash > Securities[stock].Price * 100
            //                && Time.Hour == 9 && Time.Minute == 31)
            //            {
            //                Buy(stock);
            //            }
            //        }

            //    }
            //    // Cape Ratio is missing from original data
            //    // Most recent cape data is most likely to be missing
            //    else if (_currCape == 0)
            //    {
            //        Debug("Exiting due to no CAPE!");
            //        Quit("CAPE ratio not supplied in data, exiting.");
            //    }
            //}
            //catch (RegressionTestException err)
            //{
            //    Error(err.Message);
            //}
        }


        /// <summary>
        /// Buy this symbol
        /// </summary>
        public void Buy(string symbol)
        {
            var s = Securities[symbol].Holdings;
            //if (_macdDic[symbol] > 0m)
            //{
                SetHoldings(symbol, 1);

                //Debug("Purchasing: " + symbol + "   MACD: " + _macdDic[symbol] + "   RSI: " + _rsiDic[symbol]
                //    + "   Price: " + Math.Round(Securities[symbol].Price, 2) + "   Quantity: " + s.Quantity);
            //}
        }

        /// <summary>
        /// Sell this symbol
        /// </summary>
        /// <param name="symbol"></param>
        public void Sell(string symbol)
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
}
