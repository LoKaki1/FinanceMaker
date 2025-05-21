using System.Net;
using System.Text.Json;
using FinanceMaker.Broker.Models.Configuration;
using FinanceMaker.Broker.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FinanceMaker.Broker.Services;

public class PolygonMarketDataService : IMarketDataService
{
    private readonly ILogger<PolygonMarketDataService> m_Logger;
    private readonly HttpClient m_HttpClient;
    private readonly PolygonOptions m_Options;
    private readonly ICacheService m_CacheService;
    private const string CACHE_KEY_PREFIX = "polygon_price_";
    private static readonly TimeSpan CACHE_DURATION = TimeSpan.FromSeconds(15);

    public PolygonMarketDataService(
        ILogger<PolygonMarketDataService> logger,
        HttpClient httpClient,
        IOptions<PolygonOptions> options,
        ICacheService cacheService)
    {
        m_Logger = logger;
        m_HttpClient = httpClient;
        m_Options = options.Value;
        m_CacheService = cacheService;

        m_HttpClient.BaseAddress = new Uri(m_Options.BaseUrl);
        m_HttpClient.Timeout = TimeSpan.FromSeconds(m_Options.TimeoutSeconds);
    }

    public async Task<decimal> GetCurrentPriceAsync(string symbol, CancellationToken cancellationToken)
    {
        try
        {
            // Try to get from cache first
            var cacheKey = $"{CACHE_KEY_PREFIX}{symbol}";
            var cachedPrice = await m_CacheService.GetAsync<decimal>(cacheKey, cancellationToken);
            if (cachedPrice != default)
            {
                m_Logger.LogDebug("Retrieved price for {Symbol} from cache", symbol);
                return cachedPrice;
            }

            var url = $"/v2/last/trade/{symbol}?apiKey={m_Options.ApiKey}";
            var response = await m_HttpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                await HandleErrorResponse(response, symbol);
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<PolygonLastTradeResponse>(content);

            if (result?.Results?.Price == null)
            {
                throw new InvalidOperationException($"No price data available for {symbol}");
            }

            // Cache the result
            await m_CacheService.SetAsync(cacheKey, result.Results.Price, CACHE_DURATION, cancellationToken);

            return result.Results.Price;
        }
        catch (HttpRequestException ex)
        {
            m_Logger.LogError(ex, "Network error while getting price for {Symbol} from Polygon", symbol);
            throw new MarketDataException($"Network error while fetching price for {symbol}", ex);
        }
        catch (JsonException ex)
        {
            m_Logger.LogError(ex, "Error parsing response for {Symbol} from Polygon", symbol);
            throw new MarketDataException($"Invalid response format for {symbol}", ex);
        }
        catch (TaskCanceledException ex)
        {
            m_Logger.LogError(ex, "Request timeout while getting price for {Symbol} from Polygon", symbol);
            throw new MarketDataException($"Request timeout for {symbol}", ex);
        }
        catch (Exception ex)
        {
            m_Logger.LogError(ex, "Unexpected error getting price for {Symbol} from Polygon", symbol);
            throw new MarketDataException($"Unexpected error for {symbol}", ex);
        }
    }

    private async Task HandleErrorResponse(HttpResponseMessage response, string symbol)
    {
        var content = await response.Content.ReadAsStringAsync();
        var error = JsonSerializer.Deserialize<PolygonErrorResponse>(content);

        switch (response.StatusCode)
        {
            case HttpStatusCode.Unauthorized:
                throw new MarketDataException($"Invalid API key for {symbol}");

            case HttpStatusCode.TooManyRequests:
                throw new MarketDataException($"Rate limit exceeded for {symbol}");

            case HttpStatusCode.NotFound:
                throw new MarketDataException($"Symbol {symbol} not found");

            case HttpStatusCode.BadRequest:
                throw new MarketDataException($"Invalid request for {symbol}: {error?.Error}");

            default:
                throw new MarketDataException($"Error {response.StatusCode} for {symbol}: {error?.Error}");
        }
    }

    private class PolygonLastTradeResponse
    {
        public string Status { get; set; } = string.Empty;
        public string Symbol { get; set; } = string.Empty;
        public LastTradeResult? Results { get; set; }
    }

    private class LastTradeResult
    {
        public decimal Price { get; set; }
        public int Size { get; set; }
        public long Timestamp { get; set; }
    }

    private class PolygonErrorResponse
    {
        public string Status { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;
    }
}

public class MarketDataException : Exception
{
    public MarketDataException(string message) : base(message) { }
    public MarketDataException(string message, Exception innerException) : base(message, innerException) { }
}
