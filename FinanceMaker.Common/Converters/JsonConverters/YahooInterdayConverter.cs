using System.Text.Json;
using System.Text.Json.Serialization;
using FinanceMaker.Common.Models.Finance;

namespace FinanceMaker.Common;

public sealed class YahooInterdayConverter : JsonConverter<FinanceCandleStick>
{
    public override FinanceCandleStick? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        while(reader.Read())
        {
            var current = reader.GetString();

            if (current == "timestamp")
            {
                while(reader.GetInt32()
            }
        }
    }

    public override void Write(Utf8JsonWriter writer, FinanceCandleStick value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}
