using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using FinanceMaker.Algorithms;
using FinanceMaker.Algorithms.News.Analyziers;
using FinanceMaker.Algorithms.News.Analyziers.Interfaces;
using FinanceMaker.BackTester;
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

//BackTester.Runner(typeof(RangeAlgoritm));
RealTimeTester.Runner(typeof(RangeAlgoritm));
