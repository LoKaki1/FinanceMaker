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
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

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
static async Task<string> GetAuthTokenAsync()
{
    var client = new HttpClient();
    string signInUrl = "https://www.tradingview.com/accounts/signin/";
    var formData = new MultipartFormDataContent
        {
            { new StringContent("shahartheking22"), "username" },
            { new StringContent("sm_B2w#Ec-G2LBVx"), "password" },
            { new StringContent("on"), "remember" }
        };

    client.DefaultRequestHeaders.Referrer = new Uri("https://www.tradingview.com");

    HttpResponseMessage response = await client.PostAsync(signInUrl, formData);

    var a = await response.Content.ReadAsStringAsync();
    response.EnsureSuccessStatusCode();

    string responseBody = await response.Content.ReadAsStringAsync();
    using JsonDocument doc = JsonDocument.Parse(responseBody);
    return doc.RootElement.GetProperty("user").GetProperty("auth_token").GetString();
}
var token = await GetAuthTokenAsync();
var client = new ClientWebSocket();
client.Options.SetRequestHeader("origin", "https://www.tradingview.com");
await client.ConnectAsync(new Uri("wss://data.tradingview.com/socket.io/websocket?from=chart%2Ff3h7BAE7%2F&date=2025_01_28-14_33&type=chart"), CancellationToken.None);
var p = @"{
    ""m"": ""create_study"",
    ""p"": [
        ""cs_iVW7fUjZwvxx"",
        ""st10"",
        ""st1"",
        ""sds_1"",
        ""Script@tv-scripting-101!"",
        {
            ""text"": ""bmI9Ks46_IitsHcSiEDOvDJSMF0jUqg==_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"",
            ""pineId"": ""PUB;0ded7d1e366849b381499bbb2b2ce9a4"",
            ""pineVersion"": ""1.0"",
            ""pineFeatures"": {
                ""v"": ""{\""indicator\"":1,\""plot\"":1,\""str\"":1,\""ta\"":1,\""math\"":1,\""box\"":1,\""label\"":1,\""user_methods\"":1,\""builtin_methods\"":1}"",
                ""f"": true,
                ""t"": ""text""
            },
            ""in_0"": { ""v"": 15, ""f"": true, ""t"": ""integer"" },
            ""in_1"": { ""v"": true, ""f"": true, ""t"": ""bool"" },
            ""in_2"": { ""v"": 4283683888, ""f"": true, ""t"": ""color"" },
            ""in_3"": { ""v"": 4294001472, ""f"": true, ""t"": ""color"" }
        }
    ]
}";
ArraySegment<byte> message = new ArraySegment<byte>(Encoding.UTF8.GetBytes(p));
var buffer = new ArraySegment<byte>(new byte[1024 * 20000]);
string json = @"{
  ""m"": ""create_study"",
  ""p"": [
    ""cs_iVW7fUjZwvxx"",
    ""st9"",
    ""st1"",
    ""sds_1"",
    ""Script@tv-scripting-101!"",
    {
      ""text"": ""bmI9Ks46_IitsHcSiEDOvDJSMF0jUqg==_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"",
      ""pineId"": ""PUB;0ded7d1e366849b381499bbb2b2ce9a4"",
      ""pineVersion"": ""1.0"",
      ""pineFeatures"": {
        ""v"": ""{\""indicator\"":1,\""plot\"":1,\""str\"":1,\""ta\"":1,\""math\"":1,\""box\"":1,\""label\"":1,\""user_methods\"":1,\""builtin_methods\"":1}"",
        ""f"": true,
        ""t"": ""text""
      }
    }
  ]
}";
var buffer2 = new ArraySegment<byte>(new byte[1024 * 20000]);
var message2 = new ArraySegment<byte>(Encoding.UTF8.GetBytes(json));
await client.SendAsync(message2, WebSocketMessageType.Text, true, CancellationToken.None);
await client.SendAsync(message, WebSocketMessageType.Text, true, CancellationToken.None);

await client.ReceiveAsync(buffer, CancellationToken.None);
await client.ReceiveAsync(buffer2, CancellationToken.None);
var result1 = Encoding.UTF8.GetString(buffer.Array);
var result2 = Encoding.UTF8.GetString(buffer2.Array);
File.WriteAllText("result1.txt", result1);
File.WriteAllText("result2.txt", result2);

// await client.ReceiveAsync(buffer, CancellationToken.None);
// var result = Encoding.UTF8.GetString(buffer.Array);
// File.WriteAllText("result.txt", result);


// string json = "{ \"m\": \"quote_fast_symbols\", \"p\": [ \"qs_O7U5E6BE46a7\", \"=\\\"{\\\\\\\"adjustment\\\\\\\":\\\\\\\"splits\\\\\\\",\\\\\\\"currency-id\\\\\\\":\\\\\\\"USD\\\\\\\",\\\\\\\"session\\\\\\\":\\\\\\\"extended\\\\\\\",\\\\\\\"symbol\\\\\\\":\\\\\\\"BATS:NVDA\\\\\\\"}\\\"\", \"NASDAQ:NVDA\" ] }";
// ArraySegment<byte> message2 = new ArraySegment<byte>(Encoding.UTF8.GetBytes(json));
// await client.SendAsync(message2, WebSocketMessageType.Text, true, CancellationToken.None);
// var buffer2 = new ArraySegment<byte>(new byte[1024 * 200]);
// await client.ReceiveAsync(buffer2, CancellationToken.None);
// var result2 = Encoding.UTF8.GetString(buffer2);
// File.WriteAllText("result2.txt", result2);

Console.WriteLine(result1);
