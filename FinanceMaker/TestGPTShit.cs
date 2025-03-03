using System;

namespace FinanceMaker;

using System;
using System.Globalization;
using QuantConnect;
using QuantConnect.Algorithm;
using QuantConnect.Data;
using QuantConnect.Logging;

public class NIOCustomData : BaseData
{
    public decimal OpenPrice;
    public decimal HighPrice;
    public decimal LowPrice;
    public decimal ClosePrice;
    public long Volume;

    // GetSource tells LEAN where to find the CSV data.
    // Here we assume one file per day located at a remote URL.
    public override SubscriptionDataSource GetSource(SubscriptionDataConfig config, DateTime date, bool isLiveMode)
    {
        // Example: CSV file URL with date in its name, adjust as needed.
        var filePath = CustomCandleData.SaveCandlestickDataToCsv("NIO", Common.Models.Pullers.Enums.Period.Daily, DateTime.Now.Subtract(TimeSpan.FromDays(100)), DateTime.Now).Result;
        return new SubscriptionDataSource(filePath, SubscriptionTransportMedium.LocalFile);
    }

    // Reader parses each line of the CSV.
    // Expected CSV format: Date,Open,High,Low,Close,Volume
    public override BaseData Reader(SubscriptionDataConfig config, string line, DateTime date, bool isLiveMode)
    {
        // Skip empty lines or header line
        if (string.IsNullOrWhiteSpace(line) || line.StartsWith("Date"))
        {
            return null;
        }

        var data = line.Split(',');
        try
        {
            // Parse the date assuming format "yyyy-MM-dd"
            var time = DateTime.ParseExact(data[0], "yyyy-MM-dd", CultureInfo.InvariantCulture);

            return new NIOCustomData
            {
                Time = time,
                EndTime = time.AddDays(1),  // EndTime can be set as needed
                OpenPrice = Convert.ToDecimal(data[1], CultureInfo.InvariantCulture),
                HighPrice = Convert.ToDecimal(data[2], CultureInfo.InvariantCulture),
                LowPrice = Convert.ToDecimal(data[3], CultureInfo.InvariantCulture),
                ClosePrice = Convert.ToDecimal(data[4], CultureInfo.InvariantCulture),
                Volume = Convert.ToInt64(data[5], CultureInfo.InvariantCulture),
                Value = Convert.ToDecimal(data[4], CultureInfo.InvariantCulture),
                Symbol = config.Symbol
            };
        }
        catch (Exception ex)
        {
            Log.Error($"Error parsing line: {line} | Exception: {ex.Message}");
            return null;
        }
    }

    public override BaseData Clone()
    {
        return new NIOCustomData
        {
            Time = this.Time,
            EndTime = this.EndTime,
            OpenPrice = this.OpenPrice,
            HighPrice = this.HighPrice,
            LowPrice = this.LowPrice,
            ClosePrice = this.ClosePrice,
            Volume = this.Volume,
            Value = this.Value,
            Symbol = this.Symbol
        };
    }
}

public class NIOCustomDataAlgorithm : QCAlgorithm
{
    private Symbol _nioSymbol;

    public override void Initialize()
    {
        SetStartDate(2023, 1, 1);
        SetEndDate(2023, 1, 31);
        SetCash(100000);

        // Subscribe to the custom data. The string "NIO" is used as the ticker.
        _nioSymbol = AddData<CustomCandleData>("NIO", Resolution.Daily).Symbol;
    }

    public override void OnData(Slice data)
    {
        if (data.ContainsKey(_nioSymbol))
        {
            var customData = data.Get<CustomCandleData>(_nioSymbol);
            Log($"{Time}: NIO Data => Open: {customData.Value}");

            // Example trade logic: Buy if the close price is below a threshold, otherwise liquidate.
            if (customData.Value < 30)
            {
                SetHoldings("NIO", 1);
            }
            else
            {
                Liquidate("NIO");
            }
        }
    }
}
