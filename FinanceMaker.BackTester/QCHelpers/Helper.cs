using System.Globalization;
using FinanceMaker.Common;
using FinanceMaker.Common.Models.Pullers.Enums;
using FinanceMaker.Pullers;
using Microsoft.Extensions.DependencyInjection;
using QuantConnect.Configuration;

namespace FinanceMaker.BackTester.QCHelpers;

public static class Helper
{
    public static async Task<string> SaveCandlestickDataToCsv(
     string ticker,
     Period period,
     DateTime startTime,
     DateTime endTime)
    {
        // Ensure the directory exists
        var dataDirectory = Config.Get("data-folder") + "/Custom";
        var filePath = Path.Combine(dataDirectory,
                                    $"{ticker}_{period}_{startTime.Ticks}_{endTime.Ticks}.csv");
        if (File.Exists(filePath)) return filePath;

        Directory.CreateDirectory(dataDirectory);
        // Replace this code with static container
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
}
