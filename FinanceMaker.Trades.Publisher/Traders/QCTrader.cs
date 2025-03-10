using System;
using FinanceMaker.Algorithms;
using FinanceMaker.Common;
using FinanceMaker.Common.Models.Finance;
using FinanceMaker.Common.Models.Pullers;
using FinanceMaker.Publisher.Traders.Interfaces;
using FinanceMaker.Pullers.PricesPullers;
using FinanceMaker.Pullers.TickerPullers;

namespace FinanceMaker.Publisher.Traders;

/// <summary>
/// No need to do dynamic for now,
/// This trader would trade based on the range algorithm, which means if the current price is one precent around
/// of a "KeyLevel" price and the pivot is `Pivot.Low` then we should buy, and if the pivot is `Pivot.High` then we should short it.
/// And we should we should sell the stock with a risk of 3:2 risk reward ratio.
/// </summary>
public class QCTrader : ITrader
{
    private readonly MainTickersPuller m_TickersPullers;
    private readonly RangeAlgorithmsRunner m_RangeAlgorithmsRunner;

    public QCTrader(MainTickersPuller pricesPuller,
                    RangeAlgorithmsRunner rangeAlgorithmsRunner)
    {
        m_TickersPullers = pricesPuller;
        m_RangeAlgorithmsRunner = rangeAlgorithmsRunner;
    }

    public async Task Trade(CancellationToken cancellationToken)
    {
        var longTickers = TickersPullerParameters.BestBuyer;
        // For now only long tickers, I will implement the function of short but I don't want to
        // scanTickersTwice
        // var shortTickers = TickersPullerParameters.BestSellers;
        var tickers = await m_TickersPullers.ScanTickers(longTickers, cancellationToken);
        tickers = tickers.Distinct()
                         .ToArray();

        // Now we've got the stocks, we should analyze them
        foreach (var ticker in tickers)
        {
            var range = await m_RangeAlgorithmsRunner.Run<EMACandleStick>(
                new RangeAlgorithmInput(new PricesPullerParameters(
                    ticker,
                    DateTime.Now.AddYears(-1),
                    DateTime.Now,
                    Common.Models.Pullers.Enums.Period.Daily), Algorithm.KeyLevels), cancellationToken);
        }


    }
}
