namespace FinanceMaker.Broker.Models.Configuration;

public class PolygonOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.polygon.io";
    public int TimeoutSeconds { get; set; } = 30;
}
