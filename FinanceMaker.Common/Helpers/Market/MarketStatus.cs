using System.Text.Json;
using System.Text.Json.Serialization;
public class MarketStatus
{
    private static readonly string apiUrl = "https://finnhub.io/api/v1/stock/market-status?exchange=US";
    private static readonly string apiKey = "cu4benhr01qp6s4kbgu0cu4benhr01qp6s4kbgug"; // Replace with your Finnhub API key
    private readonly IHttpClientFactory m_HttpClientFactory;

    public MarketStatus(IHttpClientFactory httpClientFactory)
    {
        m_HttpClientFactory = httpClientFactory;
    }

    public async Task<bool> IsMarketOpenAsync(CancellationToken cancellationToken)
    {
        var httpClient = m_HttpClientFactory.CreateClient();
        var response = await httpClient.GetStreamAsync($"{apiUrl}&token={apiKey}", cancellationToken);
        var marketData = await JsonSerializer.DeserializeAsync<MarketOpenResponse>(response, cancellationToken: cancellationToken);

        if (marketData is null) return false;

        return marketData!.IsOpen;
    }
}

public sealed class MarketOpenResponse
{
    [JsonPropertyName("isOpen")]
    public bool IsOpen { get; set; }
}
