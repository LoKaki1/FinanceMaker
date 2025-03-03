using System;
using System.Globalization;
using Accord.IO;
using CloneExtensions;
using FinanceMaker.Common;
using FinanceMaker.Common.Models.Finance;
using FinanceMaker.Pullers;
using Microsoft.Extensions.DependencyInjection;
using QuantConnect;
using QuantConnect.Configuration;
using QuantConnect.Data;
using YahooFinanceApi;

namespace FinanceMaker;


public class CustomCandleData : BaseData
{
    // Properties for Open, High, Low, Close, Volume
    public FinanceCandleStick CandleStick { get; set; }
    public static async Task<string> SaveCandlestickDataToCsv(
    string ticker,
    FinanceMaker.Common.Models.Pullers.Enums.Period period,
    DateTime startTime,
    DateTime endTime)
    {
        // Ensure the directory exists
        var dataDirectory = Config.Get("data-folder") + "/Custom";
        var filePath = Path.Combine(dataDirectory, $"{ticker}.csv");
        if (File.Exists(filePath)) return filePath;


        Directory.CreateDirectory(dataDirectory);
        var services = new ServiceCollection();
        services.AddHttpClient(); // Registers IHttpClientFactory
        services.AddSingleton<YahooInterdayPricesPuller>();
        using var serviceProvider = services.BuildServiceProvider();
        var finanaceMaker = serviceProvider.GetRequiredService<YahooInterdayPricesPuller>();
        // Define the file path
        // Create and write to the CSV file
        var candlesticks = await finanaceMaker.GetTickerPrices(new PricesPullerParameters(ticker, startTime, endTime, period), CancellationToken.None);
        using var writer = new StreamWriter(filePath, false);

        foreach (var candle in candlesticks)
        {
            var line = string.Format(
                CultureInfo.InvariantCulture,
                "{0:yyyyMMdd HH:mm:ss},{1},{2},{3},{4},{5}",
                candle.Time,
                candle.Open,
                candle.High,
                candle.Low,
                candle.Close,
                candle.Volume
            );
            writer.WriteLine(line);
        }

        return filePath;
    }
    // Override GetSource to specify the source of your data
    public override SubscriptionDataSource GetSource(SubscriptionDataConfig config, DateTime date, bool isLiveMode)
    {
        // Define the path to your custom data file
        var filePath = SaveCandlestickDataToCsv(config.Symbol.Value, Common.Models.Pullers.Enums.Period.Daily, DateTime.Now.Subtract(TimeSpan.FromDays(100)), DateTime.Now).Result;
        return new SubscriptionDataSource(filePath, SubscriptionTransportMedium.LocalFile);
    }

    // Override Reader to parse data into your CustomCandleData object
    public override BaseData Clone()
    {
        return new CustomCandleData
        {
            Symbol = this.Symbol,
            Time = this.Time,
            EndTime = this.EndTime,
            Value = this.Value,
            CandleStick = this.CandleStick
        };
    }

    public override BaseData Reader(SubscriptionDataConfig config, string line, DateTime date, bool isLiveMode)

    {
        // Parse the CSV line
        var data = line.Split(',');
        var aaa = DateTime.ParseExact(data[0], "yyyyMMdd HH:mm:ss", CultureInfo.InvariantCulture);
        return new CustomCandleData
        {
            Symbol = config.Symbol,
            EndTime = aaa.AddDays(1),
            Time = aaa,

            Value = Convert.ToDecimal(data[4], CultureInfo.InvariantCulture),
            CandleStick = new FinanceCandleStick

                (
                    DateTime.ParseExact(data[0], "yyyyMMdd HH:mm:ss", CultureInfo.InvariantCulture),
                    Convert.ToSingle(data[1], CultureInfo.InvariantCulture),
                    Convert.ToSingle(data[2], CultureInfo.InvariantCulture),
                    Convert.ToSingle(data[3], CultureInfo.InvariantCulture),
                    Convert.ToSingle(data[4], CultureInfo.InvariantCulture),
                    Convert.ToInt32(data[5], CultureInfo.InvariantCulture)
                )
        };
    }
}
