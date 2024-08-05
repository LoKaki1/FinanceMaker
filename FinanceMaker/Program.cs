// See https://aka.ms/new-console-template for more information
using FinanceMaker.Pullers.NewsPullers;
using FinanceMaker.Pullers.NewsPullers.Interfaces;
using FinanceMaker.Pullers.PricesPullers;
using FinanceMaker.Pullers.PricesPullers.Interfaces;
using FinanceMaker.Pullers.TickerPullers;
using FinanceMaker.Pullers.TickerPullers.Interfaces;
using FinanceMaker.Common.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading;
using HtmlAgilityPack;

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

//var builder = Host.CreateApplicationBuilder();
//var services = builder.Services;
//services.AddHttpClient();
//services.AddSingleton<FinvizTickersPuller>();
//services.AddSingleton(sp => new IParamtizedTickersPuller[]
//{
//    sp.GetService<FinvizTickersPuller>()
//});
//services.AddSingleton(sp => Array.Empty<ITickerPuller>());
//services.AddSingleton(sp => Array.Empty<IRelatedTickersPuller>());
//services.AddSingleton<MainTickersPuller>();

//services.AddSingleton<YahooPricesPuller>();
//services.AddSingleton(sp => new IPricesPuller[]
//{
//    sp.GetService<YahooPricesPuller>()
//});
//services.AddSingleton<MainPricesPuller>();
//services.AddSingleton<GoogleNewsPuller>();
//services.AddSingleton(sp => new INewsPuller[]
//{
//    sp.GetService<GoogleNewsPuller>()
//});
//services.AddSingleton<MainNewsPuller>();

//var app = builder.Build();
//var tickersPuller = app.Services.GetService<MainTickersPuller>();
//var pricesPuller = app.Services.GetService<MainPricesPuller>();
//var newsPuller = app.Services.GetService<MainNewsPuller>();

//if (tickersPuller is null ||
//    pricesPuller is null ||
//    newsPuller is null) return;

//var result = (await tickersPuller.ScanTickers(new TickersPullerParameters()
//{
//    MinAvarageVolume = 100_000,
//    MaxAvarageVolume = 1_000_000,
//    MaxPrice = 20,
//    MinPrice = 3,
//    PresentageOfChange = 20
//}, CancellationToken.None)).ToList();

//result.Add("NIO");
//var data = new List<(string ticker, TickerChart chart, TickerNews news)>();

//foreach (var ticker in result)
//{
//    var prices = await pricesPuller.GetTickerPrices(ticker,
//                                                    FinanceMaker.Common.Models.Pullers.Enums.Period.Daily,
//                                                    DateTime.Now.AddYears(-7),
//                                                    DateTime.Now,
//                                                    CancellationToken.None);
//    var chart = await newsPuller.PullNews(ticker, CancellationToken.None);

//    data.Add((ticker, prices, chart));
//}

//var gon = new List<(string s, IEnumerable<double> a)>();
//foreach(var tickerData in data)
//{
//    var supportAndResitance = SupportAndResistanceLevels.GetSupportResistanceLevels(tickerData.chart);
//    gon.Add((tickerData.ticker, supportAndResitance));
//}

//Chart chart1 = new Chart("NIO");
//for (int i = 0; i < gon.Count; i++)
//{
//    var series = new CandlestickSeries(data[i].ticker, i);

//    foreach(var point in data[i].chart.Prices)
//    {
//        series.AddPoint(point.Candlestick);
//    }


//    chart1.AddSeries(series);
//}

// var plotModel = new PlotModel { Title = "Test" };
// var dateAxis = new DateTimeAxis { Position = AxisPosition.Bottom, StringFormat = "YY/MM/dd", IntervalType = DateTimeIntervalType.Days, MajorGridlineStyle = LineStyle.Solid, MinorGridlineStyle = LineStyle.Dot };
// var valueAxis = new LinearAxis
// {
//     Position = AxisPosition.Left,
//     Title = "Price",
//     MajorGridlineStyle = LineStyle.Solid,
//     MinorGridlineStyle = LineStyle.Dot
// };
// plotModel.Axes.Add(dateAxis);
// plotModel.Axes.Add(valueAxis);

// var candleStickSeries = new CandlestickSeriesOxy
// {
//     Color = OxyColors.Black,
//     IncreasingColor = OxyColors.DarkGreen,
//     DecreasingColor = OxyColors.DarkRed,
//     DataFieldHigh = "High",
//     DataFieldX = "Date",
//     DataFieldOpen = "Open",
//     DataFieldClose = "Close",
//     DataFieldLow = "Low"
// };

// var dg = data.Last().chart.Prices.Select(_ => new HighLowItem
// {
//     High = (double)_.High,
//     Low = (double)_.Low,
//     Close = (double)_.Close,
//     Open = (double)_.Open,
//     X = _.Time.ToOADate()
// });
// candleStickSeries.Items.AddRange(dg);
// plotModel.Series.Add(candleStickSeries);
// var plotView = new PlotView()
// {
//     Model = plotModel
// };
var finvizUrl = "https://finviz.com/screener.ashx?v=111&f=news_date_today&ft=4&ah_change_10to100";
var httpClient = new HttpClient();
httpClient.AddBrowserUserAgent();

var finvizResult = await httpClient.GetAsync(finvizUrl);
if (!finvizResult.IsSuccessStatusCode)
{
    throw new NotSupportedException($"Something went wrong with finviz {finvizResult.RequestMessage}");
}

var finvizHtml = await finvizResult.Content.ReadAsStringAsync();
var node = new HtmlDocument();
node.Load($"<tbody{finvizHtml.Split("<tbody>").Last().Split("</tbody>").First()}</tbody>");

var technicalFinviz = node.DocumentNode.SelectNodes("//*[contains(@id,\"ta_\"]")
                                       .Select(_ => _.Attributes["id"].Value.Split("ta_")
                                                                            .Last())
                                       .ToArray();

Console.ReadLine();
