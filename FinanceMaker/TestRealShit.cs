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
            SetStartDate(2025, 1, 1);
            SetEndDate(2025, 3, 3);

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
            var aaa = AddData<CustomCandleData>("NIO", Resolution.Daily).Symbol;
            // AddData<CustomCandleData>("NIO");
            // var history = History<CustomCandleData>(Symbol("NIO"), TimeSpan.FromDays(10));
            // _symbols.Add("NIO");
            // foreach (string stock in _symbols)
            // {
            //     AddEquity(stock, Resolution.Daily);

            //     _macd = MACD(stock, 12, 26, 9, MovingAverageType.Exponential, Resolution.Daily);
            //     _macdDic.Add(stock, _macd);
            //     _rsi = RSI(stock, 14, MovingAverageType.Exponential, Resolution.Daily);
            //     _rsiDic.Add(stock, _rsi);

            //     Securities[stock].SetLeverage(10);
            // }
            //CAPE data
            //_macd = MACD(stock, 12, 26, 9, MovingAverageType.Exponential, Resolution.Daily);
            //_macdDic.Add(stock, _macd);
            //_rsi = RSI(stock, 14, MovingAverageType.Exponential, Resolution.Daily);
            //_rsiDic.Add(stock, _rsi);

        }
        public void OnData(CustomCandleData data)
        {
            _currCape = 12m;
            _c[_counter] = _currCape;
            _counter++;
            if (_counter == 4)
            {
                _counter = 0;
                _c.CopyTo(_cCopy, 0);
                Array.Sort(_cCopy);
                if (_cCopy[0] == _currCape)
                {
                    _newLow = true;
                }
                else
                {
                    _newLow = false;
                }
            }
        }
        /// <summary>
        /// New data for our assets.
        /// </summary>
        public override void OnData(Slice slice)
        {

            if (DepenedencyInjectionCode.Contains((float)Securities["NIO"].Price))
            {

            }

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
