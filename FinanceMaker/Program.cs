// See https://aka.ms/new-console-template for more information
using FinanceMaker.Common.Models.Pullers;
using FinanceMaker.Common.Models.Tickers;
using FinanceMaker.Pullers.NewsPullers;
using FinanceMaker.Pullers.NewsPullers.Interfaces;
using FinanceMaker.Pullers.PricesPullers;
using FinanceMaker.Pullers.PricesPullers.Interfaces;
using FinanceMaker.Pullers.TickerPullers;
using FinanceMaker.Pullers.TickerPullers.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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

var builder = Host.CreateApplicationBuilder();
var services = builder.Services;
services.AddHttpClient();
services.AddSingleton<FinvizTickersPuller>();
services.AddSingleton(sp => new IParamtizedTickersPuller[]
{
    sp.GetService<FinvizTickersPuller>()
});
services.AddSingleton(sp => Array.Empty<ITickerPuller>());
services.AddSingleton(sp => Array.Empty<IRelatedTickersPuller>());
services.AddSingleton<MainTickersPuller>();

services.AddSingleton<YahooPricesPuller>();
services.AddSingleton(sp => new IPricesPuller[]
{
    sp.GetService<YahooPricesPuller>()
});
services.AddSingleton<MainPricesPuller>();
services.AddSingleton<GoogleNewsPuller>();
services.AddSingleton(sp => new INewsPuller[]
{
    sp.GetService<GoogleNewsPuller>()
});
services.AddSingleton<MainNewsPuller>();

var app = builder.Build();
var tickersPuller = app.Services.GetService<MainTickersPuller>();
var pricesPuller = app.Services.GetService<MainPricesPuller>();
var newsPuller = app.Services.GetService<MainNewsPuller>();

if (tickersPuller is null ||
    pricesPuller is null ||
    newsPuller is null) return;

var result = await tickersPuller.ScanTickers(new TickersPullerParameters()
{
    MinAvarageVolume = 100_000,
    MaxAvarageVolume = 1_000_000,
    MaxPrice = 20,
    MinPrice = 3,
    PresentageOfChange = 20
}, CancellationToken.None);

var data = new List<(string, TickerChart, TickerNews)>();

foreach (var ticker in result)
{
    var prices = await pricesPuller.GetTickerPrices(ticker,
                                                    FinanceMaker.Common.Models.Pullers.Enums.Period.Daily,
                                                    DateTime.Now.AddYears(-1),
                                                    DateTime.Now,
                                                    CancellationToken.None);
    var chart = await newsPuller.PullNews(ticker, CancellationToken.None);

    data.Add((ticker, prices, chart));
}

Console.Write(data);


